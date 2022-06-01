// System
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

// UnityEngine
using UnityEngine;

// BepInEx
using BepInEx;
using BepInEx.Configuration;

// Jotunn
using Jotunn.Managers;
using Jotunn.Utils;
using Jotunn.Entities;
using Jotunn.Configs;
using Jotunn.GUI;
using Logger = Jotunn.Logger;

////  functions
using PluginSettings;

namespace CustomizedBronze
{

    // setup plugin data
    [BepInPlugin(PluginData.Guid, PluginData.ModName, PluginData.Version)]

    // check for running valheim process
    [BepInProcess("valheim.exe")]

    // check for jotunn dependency
    [BepInDependency(Jotunn.Main.ModGuid, BepInDependency.DependencyFlags.HardDependency)]

    // check compatibility level
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Patch)]

    internal class CustomizedBronze : BaseUnityPlugin
    {


        // private values

        // plugin
        private const Boolean showPluginInfo = true;
        private const Boolean showPluginDetailedInfo = false;
        private const Boolean showPluginErrorInfo = true;

        // event
        private const Boolean showEventInfo = false;
        private const Boolean showEventDetailedInfo = false;
        private const Boolean showEventErrorInfo = true;

        // sub
        private const Boolean showSubInfo = false;
        private const Boolean showSubDetailedInfo = false;
        private const Boolean showSubErrorInfo = true;

        // debug
        private const Boolean showDebugInfo = true;

        // new line

        private readonly static string s_CRLF = Environment.NewLine;
        private readonly static string s_CRLF2 = Environment.NewLine + Environment.NewLine;

        // config

        // enum for BronzePreset
        [Flags]
        private enum BronzePreset
        {
            Default = 0,
            WoWlike = 1,
            Realistic = 2,
            Custom = 4
        };

        // presets
        // Copper, Tin, Bronze
        private static int[] DefaultAlloy = { 2, 1, 1 };
        private static int[] WoWlikeAlloy = { 1, 1, 2 };
        private static int[] RealisticAlloy = { 2, 1, 3 };

        // config strings
        private string ConfigCategoryBronze = "Bronze";

        private string ConfigEntryBronzePreset = "Bronze preset";
        private string ConfigEntryBronzePresetDescription =
            "Alloy composition:" + s_CRLF +
            "(If you change this, you will need to log in again for the changes to take effect.)" + s_CRLF2 +

            "Default = the standard alloy from Valheim" + s_CRLF +
            "(2 Copper + 1 Tin = 1 Bronze)" + s_CRLF +
            "[If you select this preset, your custom settings will be ignored.]" + s_CRLF2 +

            "WoWlike = an alloy like in World of Warcraft" + s_CRLF +
            "(50 percent alloy, 1 Copper + 1 Tin = 2 Bronze)" + s_CRLF +
            "[If you select this preset, your custom settings will be ignored.]" + s_CRLF2 +

            "Realistic = a more realistic alloy" + s_CRLF +
            "(60 percent alloy, 2 Copper + 1 Tin = 3 Bronze)" + s_CRLF +
            "[If you select this preset, your custom settings will be ignored.]" + s_CRLF2 +

            "Custom = a custom alloy with the mixing ratio you set " + s_CRLF +
            "[If you select this , your custom settings will be used.]";

        private string ConfigCategoryCustom = "Custom";

        private string ConfigEntryCustomCopperRequirement = "Copper requirement";
        private string ConfigEntryCustomCopperRequirementDescription =
            "Sets requirement of Copper." + s_CRLF +
            "(Default requirement is " + DefaultAlloy[0] + ")";
        private string ConfigEntryCustomTinRequirement = "Tin requirement";
        private string ConfigEntryCustomTinRequirementDescription =
            "Sets requirement of Tin." + s_CRLF +
            "(Default requirement is " + DefaultAlloy[1] + ")";
        private string ConfigEntryCustomBronzeQuantity = "Bronze quantity";
        private string ConfigEntryCustomBronzeQuantityDescription =
            "Sets quantity of created Bronze." + s_CRLF +
            "(Default quantity is " + DefaultAlloy[2] + ")";

        // Configuration values
        private ConfigEntry<bool> configModEnabled;
        private ConfigEntry<int> configNexusID;
        private ConfigEntry<bool> configShowChangesAtStartup;

        private ConfigEntry<BronzePreset> configBronze;

        private ConfigEntry<int> configBronzeRequirementCopper;
        private ConfigEntry<int> configBronzeRequirementTin;
        private ConfigEntry<int> configBronzeQuantityBronze;

        private int usedRequirementCopper = 0;
        private int usedRequirementTin = 0;
        private int usedQuantityBronze = 0;

        #region[Awake]
        private void Awake()
        {

            CreateConfigValues();

            bool modEnabled = (bool)Config[PluginData.ConfigCategoryGeneral, PluginData.ConfigEntryEnabled].BoxedValue;

            if (modEnabled == true)
            {

                // ##### plugin startup logic #####

                if (showPluginDetailedInfo == true) { Jotunn.Logger.LogInfo("Loading start"); }

                // Event System

                // CreatureManager
                CreatureManager.OnCreaturesRegistered += OnCreaturesRegistered;
                CreatureManager.OnVanillaCreaturesAvailable += OnVanillaCreaturesAvailable;

                // GUIManager
                //GUIManager.OnPixelFixCreated - outdated
                GUIManager.OnCustomGUIAvailable += OnCustomGUIAvailable;

                // ItemManager
                ItemManager.OnItemsRegistered += OnItemsRegistered;
                ItemManager.OnItemsRegisteredFejd += OnItemsRegisteredFejd;
                //ItemManager.OnVanillaItemsAvailable - outdated

                // LocalizationManager
                LocalizationManager.OnLocalizationAdded += OnLocalizationAdded;

                // MinimapManager
                MinimapManager.OnVanillaMapAvailable += OnVanillaMapAvailable;
                MinimapManager.OnVanillaMapDataLoaded += OnVanillaMapDataLoaded;

                // PieceManager
                PieceManager.OnPiecesRegistered += OnPiecesRegistered;

                // PrefabManager
                PrefabManager.OnPrefabsRegistered += OnPrefabsRegistered;
                PrefabManager.OnVanillaPrefabsAvailable += OnVanillaPrefabsAvailable;

                // SynchronizationManager
                SynchronizationManager.OnAdminStatusChanged += OnAdminStatusChanged;
                //SynchronizationManager.OnConfigurationSynchronized += OnConfigurationSynchronized; - error

                // ZoneManager
                ZoneManager.OnVanillaLocationsAvailable += OnVanillaLocationsAvailable;

                // ##### info functions #####

                if (showPluginDetailedInfo == true) { Jotunn.Logger.LogInfo("Loading done"); }

                // Game data
                if (showPluginInfo == true) { Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} is active."); }

            }
            else
            {

                if (showPluginInfo == true) { Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} is disabled by config."); }

            }

        }
        #endregion

        #region[Update]
        /*        private void Update()
                {
                }*/
        #endregion

        #region[CreateConfigValues]
        // Create some sample configuration values
        private void CreateConfigValues()
        {

            Config.SaveOnConfigSet = true;

            configModEnabled = Config.Bind(PluginData.ConfigCategoryGeneral, PluginData.ConfigEntryEnabled, PluginData.ConfigEntryEnabledDefaultState,
                new ConfigDescription(PluginData.ConfigEntryEnabledDescription, null, new ConfigurationManagerAttributes { Order = 0 })
            );

            configNexusID = Config.Bind(
                PluginData.ConfigCategoryGeneral,
                PluginData.ConfigEntryNexusID,
                PluginData.ConfigEntryNexusIDID,
                new ConfigDescription(PluginData.ConfigEntryNexusIDDescription,
                    null,
                    new ConfigurationManagerAttributes
                    {
                        IsAdminOnly = false,
                        Browsable = false,
                        ReadOnly = true,
                        Order = 1
                    }
                )
            );

            configShowChangesAtStartup = Config.Bind(PluginData.ConfigCategoryPlugin, PluginData.ConfigEntryShowChangesAtStartup, PluginData.ConfigEntryShowChangesAtStartupDefaultState,
                new ConfigDescription(PluginData.ConfigEntryShowChangesAtStartupDescription, null, new ConfigurationManagerAttributes { Order = 2 })
            );

            configBronze = Config.Bind(ConfigCategoryBronze, ConfigEntryBronzePreset, BronzePreset.Default,
                new ConfigDescription(ConfigEntryBronzePresetDescription, null, new ConfigurationManagerAttributes { Order = 3 }));

            configBronzeRequirementCopper = Config.Bind(ConfigCategoryCustom, ConfigEntryCustomCopperRequirement, DefaultAlloy[0],
                new ConfigDescription(ConfigEntryCustomCopperRequirementDescription, null, new ConfigurationManagerAttributes { Order = 4 }));

            configBronzeRequirementTin = Config.Bind(ConfigCategoryCustom, ConfigEntryCustomTinRequirement, DefaultAlloy[1],
                new ConfigDescription(ConfigEntryCustomTinRequirementDescription, null, new ConfigurationManagerAttributes { Order = 5 }));

            configBronzeQuantityBronze = Config.Bind(ConfigCategoryCustom, ConfigEntryCustomBronzeQuantity, DefaultAlloy[2],
                new ConfigDescription(ConfigEntryCustomBronzeQuantityDescription, null, new ConfigurationManagerAttributes { Order = 6 }));

            // You can subscribe to a global event when config got synced initially and on changes
            SynchronizationManager.OnConfigurationSynchronized += (obj, attr) =>
            {
                
                if (attr.InitialSynchronization)
                {

                    Jotunn.Logger.LogMessage("Initial Config sync event received");

                }
                else
                {

                    Jotunn.Logger.LogMessage("Config sync event received");

                }

            };

        }
        #endregion

        #region[ReadConfigValues]
        private void ReadConfigValues() {

            // get state of showChangesAtStartup
            bool showChangesAtStartup = (bool)Config[PluginData.ConfigCategoryPlugin, PluginData.ConfigEntryShowChangesAtStartup].BoxedValue;

            // get BronzePreset config option
            BronzePreset bronzePreset = (BronzePreset)Config[ConfigCategoryBronze, ConfigEntryBronzePreset].BoxedValue;

            // check enum config option
            switch (bronzePreset)
            {

                case BronzePreset.Default:

                    if (showChangesAtStartup == true) { Logger.LogInfo("Default option selected"); }
                    usedRequirementCopper = DefaultAlloy[0];
                    usedRequirementTin = DefaultAlloy[1];
                    usedQuantityBronze = DefaultAlloy[2];

                break;

                case BronzePreset.WoWlike:

                    if (showChangesAtStartup == true) { Logger.LogInfo("WoWlike option selected"); }
                    usedRequirementCopper = WoWlikeAlloy[0];
                    usedRequirementTin = WoWlikeAlloy[1];
                    usedQuantityBronze = WoWlikeAlloy[2];

                break;

                case BronzePreset.Realistic:

                    if (showChangesAtStartup == true) { Logger.LogInfo("Realistic option selected"); }
                    usedRequirementCopper = RealisticAlloy[0];
                    usedRequirementTin = RealisticAlloy[1];
                    usedQuantityBronze = RealisticAlloy[2];

                break;

                case BronzePreset.Custom:
                    
                    if (showChangesAtStartup == true) { Logger.LogInfo("Custom option selected"); }

                    usedRequirementCopper = (int)Config[ConfigCategoryCustom, ConfigEntryCustomCopperRequirement].BoxedValue;
                    usedRequirementTin = (int)Config[ConfigCategoryCustom, ConfigEntryCustomTinRequirement].BoxedValue;
                    usedQuantityBronze = (int)Config[ConfigCategoryCustom, ConfigEntryCustomBronzeQuantity].BoxedValue;

                break;
                
                default:

                    if (showChangesAtStartup == true) { Logger.LogInfo("unknown option selected"); }

                break;

            }

            if (showDebugInfo == true)
            {
                Logger.LogInfo(
                        "usedRequirementCopper = " + usedRequirementCopper +
                        ", usedRequirementTin = " + usedRequirementTin +
                        ", usedQuantityBronze = " + usedQuantityBronze);
            }

        }
        #endregion

        #region[EventSystem]

        // CreatureManager
        #region[OnCreaturesRegistered]
        private void OnCreaturesRegistered()
        {

            try
            {
                if (showEventInfo == true) { Logger.LogInfo("OnCreaturesRegistered"); }
            }
            catch (Exception ex)
            {
                if (showEventErrorInfo == true) { Logger.LogError($"Error OnCreaturesRegistered : {ex.Message}"); }
            }
            finally
            {
                CreatureManager.OnCreaturesRegistered -= OnCreaturesRegistered;
            }

        }
        #endregion
        #region[OnVanillaCreaturesAvailable]
        private void OnVanillaCreaturesAvailable()
        {

            try
            {
                if (showEventInfo == true) { Logger.LogInfo("OnVanillaCreaturesAvailable"); }
            }
            catch (Exception ex)
            {
                if (showEventErrorInfo == true) { Logger.LogError($"Error OnVanillaCreaturesAvailable : {ex.Message}"); }
            }
            finally
            {
                CreatureManager.OnVanillaCreaturesAvailable -= OnVanillaCreaturesAvailable;
            }

        }
        #endregion

        // GUIManager
        #region[OnCustomGUIAvailable]
        private void OnCustomGUIAvailable()
        {

            try
            {
                if (showEventInfo == true) { Logger.LogInfo("OnCustomGUIAvailable"); }
            }
            catch (Exception ex)
            {
                if (showEventErrorInfo == true) { Logger.LogError($"Error OnCustomGUIAvailable : {ex.Message}"); }
            }
            finally
            {
                GUIManager.OnCustomGUIAvailable -= OnCustomGUIAvailable;
            }

        }
        #endregion

        // ItemManager
        #region[OnItemsRegistered]
        private void OnItemsRegistered()
        {

            try
            {

                if (showEventInfo == true) { Logger.LogInfo("OnItemsRegistered"); }

                // read new quantities
                //ReadAndWriteConfigValues();
                ReadConfigValues();

                // change recipe
                ChangeBronzeRecipe();

            }
            catch (Exception ex)
            {
                if (showEventErrorInfo == true) { Logger.LogError($"Error OnItemsRegistered : {ex.Message}"); }
            }
            finally
            {
                PrefabManager.OnPrefabsRegistered -= OnItemsRegistered;
            }

        }
        #endregion
        #region[OnItemsRegisteredFejd]
        private void OnItemsRegisteredFejd()
        {

            try
            {
                if (showEventInfo == true) { Logger.LogInfo("OnItemsRegisteredFejd"); }

            }
            catch (Exception ex)
            {
                if (showEventErrorInfo == true) { Logger.LogError($"Error OnItemsRegisteredFejd : {ex.Message}"); }
            }
            finally
            {
                ItemManager.OnItemsRegisteredFejd -= OnItemsRegisteredFejd;
            }

        }
        #endregion

        // LocalizationManager
        #region[OnLocalizationAdded]
        private void OnLocalizationAdded()
        {

            try
            {
                if (showEventInfo == true) { Logger.LogInfo("OnLocalizationAdded"); }
            }
            catch (Exception ex)
            {
                if (showEventErrorInfo == true) { Logger.LogError($"Error OnLocalizationAdded : {ex.Message}"); }
            }
            finally
            {
                LocalizationManager.OnLocalizationAdded -= OnLocalizationAdded;
            }

        }
        #endregion

        // MinimapManager
        #region[OnVanillaMapAvailable]
        private void OnVanillaMapAvailable()
        {

            try
            {
                if (showEventInfo == true) { Logger.LogInfo("OnVanillaMapAvailable"); }
            }
            catch (Exception ex)
            {
                if (showEventErrorInfo == true) { Logger.LogError($"Error OnVanillaMapAvailable : {ex.Message}"); }
            }
            finally
            {
                MinimapManager.OnVanillaMapAvailable -= OnVanillaMapAvailable;
            }

        }
        #endregion
        #region[OnVanillaMapDataLoaded]
        private void OnVanillaMapDataLoaded()
        {

            try
            {
                if (showEventInfo == true) { Logger.LogInfo("OnVanillaMapDataLoaded"); }
            }
            catch (Exception ex)
            {
                if (showEventErrorInfo == true) { Logger.LogError($"Error OnVanillaMapDataLoaded : {ex.Message}"); }
            }
            finally
            {
                MinimapManager.OnVanillaMapDataLoaded -= OnVanillaMapDataLoaded;
            }

        }
        #endregion

        // PieceManager
        #region[OnPiecesRegistered]
        private void OnPiecesRegistered()
        {

            try
            {
                if (showEventInfo == true) { Logger.LogInfo("OnPiecesRegistered"); }
            }
            catch (Exception ex)
            {
                if (showEventErrorInfo == true) { Logger.LogError($"Error OnPiecesRegistered : {ex.Message}"); }
            }
            finally
            {
                PieceManager.OnPiecesRegistered -= OnPiecesRegistered;
            }

        }
        #endregion

        // PrefabManager
        #region[OnPrefabsRegistered]
        private void OnPrefabsRegistered()
        {

            try
            {
                if (showEventInfo == true) { Logger.LogInfo("OnPrefabsRegistered"); }
            }
            catch (Exception ex)
            {
                if (showEventErrorInfo == true) { Logger.LogError($"Error OnPrefabsRegistered : {ex.Message}"); }
            }
            finally
            {
                PrefabManager.OnPrefabsRegistered -= OnPrefabsRegistered;
            }

        }
        #endregion
        #region[OnVanillaPrefabsAvailable]
        private void OnVanillaPrefabsAvailable()
        {

            try
            {
                if (showEventInfo == true) { Logger.LogInfo("OnVanillaPrefabsAvailable"); }
            }
            catch (Exception ex)
            {
                if (showEventErrorInfo == true) { Logger.LogError($"Error OnVanillaPrefabsAvailable : {ex.Message}"); }
            }
            finally
            {
                PrefabManager.OnVanillaPrefabsAvailable -= OnVanillaPrefabsAvailable;
            }

        }
        #endregion

        // SynchronizationManager
        #region[OnAdminStatusChanged]
        private void OnAdminStatusChanged()
        {

            try
            {
                if (showEventInfo == true) { Logger.LogInfo("OnAdminStatusChanged"); }
            }
            catch (Exception ex)
            {
                if (showEventErrorInfo == true) { Logger.LogError($"Error OnAdminStatusChanged : {ex.Message}"); }
            }
            finally
            {
                SynchronizationManager.OnAdminStatusChanged -= OnAdminStatusChanged;
            }

        }
        #endregion
        #region[OnConfigurationSynchronized]
        private void OnConfigurationSynchronized()
        {

            try
            {
                if (showEventInfo == true) { Logger.LogInfo("OnConfigurationSynchronized"); }
            }
            catch (Exception ex)
            {
                if (showEventErrorInfo == true) { Logger.LogError($"Error OnConfigurationSynchronized : {ex.Message}"); }
            }
            finally
            {
                //SynchronizationManager.OnConfigurationSynchronized -= OnConfigurationSynchronized;
            }

        }
        #endregion

        // ZoneManager
        #region[OnVanillaLocationsAvailable]
        private void OnVanillaLocationsAvailable()
        {

            try
            {
                if (showEventInfo == true) { Logger.LogInfo("OnVanillaLocationsAvailable"); }
            }
            catch (Exception ex)
            {
                if (showEventErrorInfo == true) { Logger.LogError($"Error OnVanillaLocationsAvailable : {ex.Message}"); }
            }
            finally
            {
                ZoneManager.OnVanillaLocationsAvailable -= OnVanillaLocationsAvailable;
            }

        }
        #endregion

        #endregion

        #region[ChangeBronzeRecipe]
        private void ChangeBronzeRecipe()
        {

            // init vars
            int requirementCopper = 0;
            int requirementTin = 0;
            int quantityBronze = 0;

            // get state of showChangesAtStartup
            bool showChangesAtStartup = (bool)Config[PluginData.ConfigCategoryPlugin, PluginData.ConfigEntryShowChangesAtStartup].BoxedValue;

            // Recipe_Bronze
            foreach (Recipe instanceMRecipe in ObjectDB.instance.m_recipes.Where(r => r.m_item?.name == "Bronze"))
            {

                if (instanceMRecipe.name == "Recipe_Bronze")
                {

                    requirementCopper = usedRequirementCopper;
                    requirementTin = usedRequirementTin;
                    quantityBronze = usedQuantityBronze;

                    // set Quantity of produced bronze
                    instanceMRecipe.m_amount = quantityBronze;

                    // requirements
                    instanceMRecipe.m_resources = new Piece.Requirement[]
                    {

                        // set Quantity of needed copper
                        new Piece.Requirement() { m_resItem = PrefabManager.Cache.GetPrefab<ItemDrop>("Copper"), m_amount = requirementCopper },

                        // set Quantity of needed tin
                        new Piece.Requirement() { m_resItem = PrefabManager.Cache.GetPrefab<ItemDrop>("Tin"), m_amount = requirementTin }

                    };

                }

                // Recipe_Bronze5
                else if (instanceMRecipe.name == "Recipe_Bronze5")
                {

                    requirementCopper = usedRequirementCopper * 5;
                    requirementTin = usedRequirementTin * 5;
                    quantityBronze = usedQuantityBronze * 5;

                    // set Quantity of produced bronze
                    instanceMRecipe.m_amount = quantityBronze;

                    // requirements
                    instanceMRecipe.m_resources = new Piece.Requirement[]
                    {

                        // set Quantity of needed copper
                        new Piece.Requirement() { m_resItem = PrefabManager.Cache.GetPrefab<ItemDrop>("Copper"), m_amount = requirementCopper },

                        // set Quantity of needed tin
                        new Piece.Requirement() { m_resItem = PrefabManager.Cache.GetPrefab<ItemDrop>("Tin"), m_amount = requirementTin }

                    };

                }

                if (showChangesAtStartup == true)
                {

                    Jotunn.Logger.LogInfo($"changes " + instanceMRecipe.name +
                        ", set Copper requirement to " + requirementCopper +
                        ", set Tin requirement to " + requirementTin +
                        ", set created quantity of Bronze to " + quantityBronze
                        );

                }

            }

        }
        #endregion

    }

}