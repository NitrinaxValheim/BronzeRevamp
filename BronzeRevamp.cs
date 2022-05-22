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

        public readonly string CRLF = Environment.NewLine;
        public readonly string CRLF2 = Environment.NewLine + Environment.NewLine;

        public readonly string nameOfCustomCategory = "Custom";

        public readonly string nameOfBronzeConfigEntry = "Bronze quantity";
        public readonly string nameOfCopperConfigEntry = "Copper quantity";
        public readonly string nameOfTinConfigEntry = "Tin quantity";

        public readonly int defaultQuantityCopper = 2;
        public readonly int defaultQuantityTin = 1;
        public readonly int defaultQuantityBronze = 1;

        public static int usedQuantityCopper = 0;
        public static int usedQuantityTin = 0;
        public static int usedQuantityBronze = 0;

        // Configuration values
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
            if (usedQuantityCopper == 0 || defaultQuantityTin == 0 || defaultQuantityBronze == 0) { ReadAndWriteConfigValues(); }

            configQuantityBronze = Config.Bind(nameOfCustomCategory, nameOfBronzeConfigEntry, usedQuantityBronze,
                new ConfigDescription("Sets quantity of needed Bronze. (Default quantity is " + defaultQuantityBronze + ")" + CRLF2 +
                "If you want an alloy like in World of Warcraft, set Bronze quantity to 2, Copper and Tin quantity to 1 each (50 percent alloy)." + CRLF2 +
                "If you want a more realistic alloy, set Bronze quantity to 3, Copper quantity to 2, and Tin quantity to 1 (60 percent alloy)." + CRLF2 +
                "If you want to reset the quantities to their original values, set Bronze quantity to 1, Copper quantity to 2 and Tin quantity to 1." + CRLF2 +
                "If you change the quantities while you are in the game, you must log in again for the new quantities to be applied."));

            configQuantityCopper = Config.Bind(nameOfCustomCategory, nameOfCopperConfigEntry, usedQuantityCopper,
                new ConfigDescription("Sets quantity of produced Copper. (Default quantity is " + defaultQuantityCopper + ")"));
            
            configQuantityTin = Config.Bind(nameOfCustomCategory, nameOfTinConfigEntry, usedQuantityTin,
                new ConfigDescription("Sets quantity of needed Tin. (Default quantity is " + defaultQuantityTin + ")"));

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
            if (usedQuantityCopper == 0)
            {
                // set it to the base quantity
                usedQuantityCopper = defaultQuantityCopper;
            }
            else
            {
                // else get quantity from config
                usedQuantityCopper = (int)Config[nameOfCustomCategory, nameOfCopperConfigEntry].BoxedValue;
            }

            // if used quantity is 0
            if (usedQuantityTin == 0)
            {
                // set it to the base quantity
                usedQuantityTin = defaultQuantityTin;
            }
            else
            {
                // else get quantity from config
                usedQuantityTin = (int)Config[nameOfCustomCategory, nameOfTinConfigEntry].BoxedValue;
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
                usedQuantityBronze = (int)Config[nameOfCustomCategory, nameOfBronzeConfigEntry].BoxedValue;
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

                // read new quantities
                ReadAndWriteConfigValues();

                // change recipe
                ChangeBronzeRecipe(usedQuantityCopper, usedQuantityTin, usedQuantityBronze);

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
        private static void ChangeBronzeRecipe(int QuantityCopper, int QuantityTin, int QuantityBronze)
        {

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
                        new Piece.Requirement() { m_resItem = PrefabManager.Cache.GetPrefab<ItemDrop>("Copper"), m_amount = QuantityCopper },

                        // set Quantity of needed tin
                        new Piece.Requirement() { m_resItem = PrefabManager.Cache.GetPrefab<ItemDrop>("Tin"), m_amount = QuantityTin }
                        
                    };

                }

                //[Info: BronzeRevamp.BronzeRevamp] Recipe_Bronze5
                else if (instanceMRecipe.name == "Recipe_Bronze5")
                {

                    QuantityCopper = QuantityCopper * 5;
                    QuantityTin = QuantityTin * 5;
                    QuantityBronze = QuantityBronze * 5;

                    // set Quantity of produced bronze
                    instanceMRecipe.m_amount = QuantityBronze;

                    // requirements
                    instanceMRecipe.m_resources = new Piece.Requirement[]
                    {

                        // set Quantity of needed copper
                        new Piece.Requirement() { m_resItem = PrefabManager.Cache.GetPrefab<ItemDrop>("Copper"), m_amount = QuantityCopper },

                        // set Quantity of needed tin
                        new Piece.Requirement() { m_resItem = PrefabManager.Cache.GetPrefab<ItemDrop>("Tin"), m_amount = QuantityTin }
                        
                    };

                }

                if (showSubDetailedInfo == true) {

                    Jotunn.Logger.LogInfo($"Updated " + instanceMRecipe.m_item.name + " of " + instanceMRecipe.name + 
                        ", set created Quantity to " + QuantityBronze +
                        ", set Copper Quantity to " + QuantityCopper +
                        ", set Tin Quantity to " + QuantityTin
                        );

                }

            }

        }
        #endregion

    }

}