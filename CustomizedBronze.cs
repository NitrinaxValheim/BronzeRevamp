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

        private readonly static string CRLF = Environment.NewLine;
        private readonly static string CRLF2 = Environment.NewLine + Environment.NewLine;

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
        private readonly static int[] DefaultAlloy = { 2, 1, 1 };
        private readonly static int[] WoWlikeAlloy = { 1, 1, 2 };
        private readonly static int[] RealisticAlloy = { 2, 1, 3 };

        // config strings
        private readonly string Config_Category_Bronze = "Bronze";

        private readonly string Config_Entry_Bronze_Preset = "Bronze preset";
        private readonly string Config_Entry_Bronze_PresetDescription =
            "Alloy composition:" + CRLF +
            "(If you change this, you will need to log in again for the changes to take effect.)" + CRLF2 +

            "Default = the standard alloy from Valheim" + CRLF +
            "(2 Copper + 1 Tin = 1 Bronze)" + CRLF +
            "[If you select this preset, your custom settings will be ignored.]" + CRLF2 +

            "WoWlike = an alloy like in World of Warcraft" + CRLF +
            "(50 percent alloy, 1 Copper + 1 Tin = 2 Bronze)" + CRLF +
            "[If you select this preset, your custom settings will be ignored.]" + CRLF2 +

            "Realistic = a more realistic alloy" + CRLF +
            "(60 percent alloy, 2 Copper + 1 Tin = 3 Bronze)" + CRLF +
            "[If you select this preset, your custom settings will be ignored.]" + CRLF2 +

            "Custom = a custom alloy with the mixing ratio you set " + CRLF +
            "[If you select this , your custom settings will be used.]";

        private readonly string Config_Category_Custom = "Custom";

        private readonly string Config_Entry_Custom_Copper_Requirement = "Copper requirement";
        private readonly string Config_Entry_Custom_Copper_Requirement_Description =
            "Sets requirement of Copper." + CRLF +
            "(Default requirement is " + DefaultAlloy[0] + ")";
        private readonly string Config_Entry_Custom_Tin_Requirement = "Tin requirement";
        private readonly string Config_Entry_Custom_Tin_Requirement_Description =
            "Sets requirement of Tin." + CRLF +
            "(Default requirement is " + DefaultAlloy[1] + ")";
        private readonly string Config_Entry_Custom_Bronze_Quantity = "Bronze quantity";
        private readonly string Config_Entry_Custom_Bronze_Quantity_Description =
            "Sets quantity of created Bronze." + CRLF +
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

            bool modEnabled = (bool)Config[PluginData.Config_Category_General, PluginData.Config_Entry_Enabled].BoxedValue;

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

            configModEnabled = Config.Bind(PluginData.Config_Category_General, PluginData.Config_Entry_Enabled, PluginData.Config_Entry_Enabled_DefaultState,
                new ConfigDescription(PluginData.Config_Entry_Enabled_Description, null, new ConfigurationManagerAttributes { Order = 0 })
            );

            configNexusID = Config.Bind(
                PluginData.Config_Category_General,
                PluginData.Config_Entry_NexusID,
                PluginData.Config_Entry_NexusID_ID,
                new ConfigDescription(PluginData.Config_Entry_NexusID_Description,
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

            configShowChangesAtStartup = Config.Bind(PluginData.Config_Category_Plugin, PluginData.Config_Entry_ShowChangesAtStartup, PluginData.Config_Entry_ShowChangesAtStartup_DefaultState,
                new ConfigDescription(PluginData.Config_Entry_ShowChangesAtStartup_Description, null, new ConfigurationManagerAttributes { Order = 2 })
            );

            configBronze = Config.Bind(Config_Category_Bronze, Config_Entry_Bronze_Preset, BronzePreset.Default,
                new ConfigDescription(Config_Entry_Bronze_PresetDescription, null, new ConfigurationManagerAttributes { Order = 3 }));

            configBronzeRequirementCopper = Config.Bind(Config_Category_Custom, Config_Entry_Custom_Copper_Requirement, DefaultAlloy[0],
                new ConfigDescription(Config_Entry_Custom_Copper_Requirement_Description, null, new ConfigurationManagerAttributes { Order = 4 }));

            configBronzeRequirementTin = Config.Bind(Config_Category_Custom, Config_Entry_Custom_Tin_Requirement, DefaultAlloy[1],
                new ConfigDescription(Config_Entry_Custom_Tin_Requirement_Description, null, new ConfigurationManagerAttributes { Order = 5 }));

            configBronzeQuantityBronze = Config.Bind(Config_Category_Custom, Config_Entry_Custom_Bronze_Quantity, DefaultAlloy[2],
                new ConfigDescription(Config_Entry_Custom_Bronze_Quantity_Description, null, new ConfigurationManagerAttributes { Order = 6 }));

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
            bool showChangesAtStartup = (bool)Config[PluginData.Config_Category_Plugin, PluginData.Config_Entry_ShowChangesAtStartup].BoxedValue;

            // get BronzePreset config option
            BronzePreset bronzePreset = (BronzePreset)Config[Config_Category_Bronze, Config_Entry_Bronze_Preset].BoxedValue;

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

                    usedRequirementCopper = (int)Config[Config_Category_Custom, Config_Entry_Custom_Copper_Requirement].BoxedValue;
                    usedRequirementTin = (int)Config[Config_Category_Custom, Config_Entry_Custom_Tin_Requirement].BoxedValue;
                    usedQuantityBronze = (int)Config[Config_Category_Custom, Config_Entry_Custom_Bronze_Quantity].BoxedValue;

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

                // read config values for config menu
                //ReadAndWriteConfigValues();

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
            int RequirementCopper = 0;
            int RequirementTin = 0;
            int QuantityBronze = 0;

            // get state of showChangesAtStartup
            bool showChangesAtStartup = (bool)Config[PluginData.Config_Category_Plugin, PluginData.Config_Entry_ShowChangesAtStartup].BoxedValue;

            // Recipe_Bronze
            foreach (Recipe instanceMRecipe in ObjectDB.instance.m_recipes.Where(r => r.m_item?.name == "Bronze"))
            {

                if (instanceMRecipe.name == "Recipe_Bronze")
                {

                    RequirementCopper = usedRequirementCopper;
                    RequirementTin = usedRequirementTin;
                    QuantityBronze = usedQuantityBronze;

                    // set Quantity of produced bronze
                    instanceMRecipe.m_amount = QuantityBronze;

                    // requirements
                    instanceMRecipe.m_resources = new Piece.Requirement[]
                    {

                        // set Quantity of needed copper
                        new Piece.Requirement() { m_resItem = PrefabManager.Cache.GetPrefab<ItemDrop>("Copper"), m_amount = RequirementCopper },

                        // set Quantity of needed tin
                        new Piece.Requirement() { m_resItem = PrefabManager.Cache.GetPrefab<ItemDrop>("Tin"), m_amount = RequirementTin }

                    };

                }

                // Recipe_Bronze5
                else if (instanceMRecipe.name == "Recipe_Bronze5")
                {

                    RequirementCopper = usedRequirementCopper * 5;
                    RequirementTin = usedRequirementTin * 5;
                    QuantityBronze = usedQuantityBronze * 5;

                    // set Quantity of produced bronze
                    instanceMRecipe.m_amount = QuantityBronze;

                    // requirements
                    instanceMRecipe.m_resources = new Piece.Requirement[]
                    {

                        // set Quantity of needed copper
                        new Piece.Requirement() { m_resItem = PrefabManager.Cache.GetPrefab<ItemDrop>("Copper"), m_amount = RequirementCopper },

                        // set Quantity of needed tin
                        new Piece.Requirement() { m_resItem = PrefabManager.Cache.GetPrefab<ItemDrop>("Tin"), m_amount = RequirementTin }

                    };

                }

                if (showChangesAtStartup == true)
                {

                    Jotunn.Logger.LogInfo($"changes " + instanceMRecipe.name +
                        ", set Copper requirement to " + RequirementCopper +
                        ", set Tin requirement to " + RequirementTin +
                        ", set created quantity of Bronze to " + QuantityBronze
                        );

                }

            }

        }
        #endregion

    }

}