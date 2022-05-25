// System
using System;
using System.IO;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

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

using Logger = Jotunn.Logger;

////  functions
using BronzeRevamp.Settings;

namespace BronzeRevamp
{

    // setup plugin data
    [BepInPlugin(ModData.Guid, ModData.ModName, ModData.Version)]

    // check for running valheim process
    [BepInProcess("valheim.exe")]

    // check for jotunn dependency
    [BepInDependency(Jotunn.Main.ModGuid, BepInDependency.DependencyFlags.HardDependency)]

    // check compatibility level
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Patch)]

    internal class BronzeRevamp : BaseUnityPlugin
    {

        private static Boolean showModInfo = true;
        private static Boolean showModDetailedInfo = false;
        private static Boolean showModErrorInfo = true;

        private static Boolean showEventInfo = false;
        private static Boolean showEventDetailedInfo = false;
        private static Boolean showEventErrorInfo = true;

        private static Boolean showSubInfo = false;
        private static Boolean showSubDetailedInfo = false;
        private static Boolean showSubErrorInfo = true;

        private readonly string CRLF = Environment.NewLine;
        private readonly string CRLF2 = Environment.NewLine + Environment.NewLine;

        // config strings
        private readonly string nameOfGeneralCategory = "General";
        private readonly string nameOfBronzeCategory = "Bronze";

        private readonly string nameOfShowChangesAtStartupConfigEntry = "show changes at startup";
        private readonly string nameOfBronzeConfigEntry = "Bronze quantity";
        private readonly string nameOfCopperConfigEntry = "Copper quantity";
        private readonly string nameOfTinConfigEntry = "Tin quantity";

        private bool defaultShowChangesAtStartup = false;
        private int defaultRequirementCopper = 2;
        private int defaultRequirementTin = 1;
        private int defaultQuantityBronze = 1;

        private int usedRequirementCopper = 0;
        private int usedRequirementTin = 0;
        private int usedQuantityBronze = 0;

        // Configuration values
        private ConfigEntry<bool> configShowChangesAtStartup;
        private ConfigEntry<int> configQuantityCopper;
        private ConfigEntry<int> configQuantityTin;
        private ConfigEntry<int> configQuantityBronze;

        #region[Awake]
        private void Awake()
        {

            // ##### plugin startup logic #####

            if (showModDetailedInfo == true) { Jotunn.Logger.LogInfo("Loading start"); }

            CreateConfigValues();

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

            if (showModDetailedInfo == true) { Jotunn.Logger.LogInfo("Loading done"); }

            // Game data
            if (showModInfo == true) { Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} is active."); }

        }
        #endregion

        #region[Update]
        private void Update()
        {
        }
        #endregion

        // Create some sample configuration values
        private void CreateConfigValues()
        {
            
            Config.SaveOnConfigSet = true;

            // Add client config which can be edited in every local instance independently

            // check if one of the quantity is 0, sets the quantities to the original values
            if (usedRequirementCopper == 0 || usedRequirementTin == 0 || usedQuantityBronze == 0) { ReadAndWriteConfigValues(); }

            configShowChangesAtStartup = Config.Bind(nameOfGeneralCategory, nameOfShowChangesAtStartupConfigEntry, defaultShowChangesAtStartup,
                new ConfigDescription("If this option is activated, the changes to the recipe for the production of bronze are displayed in the BepinEx log."));

            configQuantityBronze = Config.Bind(nameOfBronzeCategory, nameOfBronzeConfigEntry, usedQuantityBronze,
                new ConfigDescription("Sets quantity of created Bronze. (Default quantity is " + defaultQuantityBronze + ")" + CRLF2 +
                "If you want an alloy like in World of Warcraft, set Bronze quantity to 2, Copper and Tin requirement to 1 each (50 percent alloy)." + CRLF2 +
                "If you want a more realistic alloy, set Bronze quantity to 3, Copper requirement to 2, and Tin requirement to 1 (60 percent alloy)." + CRLF2 +
                "If you want to reset the quantities to their original values, set Bronze quantity to 1, Copper requirement to 2 and Tin requirement to 1." + CRLF2 +
                "If you change the quantities while you are in the game, you must log in again for the new quantities to be applied."));

            configQuantityCopper = Config.Bind(nameOfBronzeCategory, nameOfCopperConfigEntry, usedRequirementCopper,
                new ConfigDescription("Sets requirement of Copper. (Default requirement is " + defaultRequirementCopper + ")"));
            
            configQuantityTin = Config.Bind(nameOfBronzeCategory, nameOfTinConfigEntry, usedRequirementTin,
                new ConfigDescription("Sets requirement of Tin. (Default quantity is " + defaultRequirementTin + ")"));

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

        // Examples for reading and writing configuration values
        private void ReadAndWriteConfigValues()
        {

            // if used quantity is 0
            if (usedRequirementCopper == 0)
            {
                // set it to the base quantity
                usedRequirementCopper = defaultRequirementCopper;
            }
            else
            {
                // else get quantity from config
                usedRequirementCopper = (int)Config[nameOfBronzeCategory, nameOfCopperConfigEntry].BoxedValue;
            }

            // if used quantity is 0
            if (usedRequirementTin == 0)
            {
                // set it to the base quantity
                usedRequirementTin = defaultRequirementTin;
            }
            else
            {
                // else get quantity from config
                usedRequirementTin = (int)Config[nameOfBronzeCategory, nameOfTinConfigEntry].BoxedValue;
            }

            // if used quantity is 0
            if (usedQuantityBronze == 0)
            {
                // set it to the base quantity
                usedQuantityBronze = defaultQuantityBronze;
            }
            else
            {
                // else get quantity from config
                usedQuantityBronze = (int)Config[nameOfBronzeCategory, nameOfBronzeConfigEntry].BoxedValue;
            }

        }

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
                ReadAndWriteConfigValues();

                // change recipe
                ChangeBronzeRecipe(usedRequirementCopper, usedRequirementTin, usedQuantityBronze);

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
                ReadAndWriteConfigValues();

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
        private void ChangeBronzeRecipe(int RequirementCopper, int RequirementTin, int QuantityBronze)
        {

            bool showChangesAtStartup = (bool)Config[nameOfGeneralCategory, nameOfShowChangesAtStartupConfigEntry].BoxedValue;

            //[Info: BronzeRevamp.BronzeRevamp] Recipe_Bronze
            foreach (Recipe instanceMRecipe in ObjectDB.instance.m_recipes.Where(r => r.m_item?.name == "Bronze"))
            {

                if (instanceMRecipe.name == "Recipe_Bronze")
                {

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

                //[Info: BronzeRevamp.BronzeRevamp] Recipe_Bronze5
                else if (instanceMRecipe.name == "Recipe_Bronze5")
                {

                    RequirementCopper = RequirementCopper * 5;
                    RequirementTin = RequirementTin * 5;
                    QuantityBronze = QuantityBronze * 5;

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

                if (showChangesAtStartup == true) {

                    Jotunn.Logger.LogInfo($"changes " + instanceMRecipe.name + 
                        ", set created quantity to " + QuantityBronze +
                        ", set Copper requirement to " + RequirementCopper +
                        ", set Tin requirement to " + RequirementTin
                        );

                }

            }

        }
        #endregion

    }

}