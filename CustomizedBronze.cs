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

using Common;

using Plugin;
using System.ComponentModel;
using System.Text;

namespace CustomizedBronze
{

    // setup plugin data
    [BepInPlugin(Data.ModGuid, Data.ModName, Data.Version)]

    // check for running valheim process
    [BepInProcess("valheim.exe")]

    // check for jotunn dependency
    [BepInDependency(Jotunn.Main.ModGuid, BepInDependency.DependencyFlags.HardDependency)]

    // check compatibility level
    [NetworkCompatibility(CompatibilityLevel.ClientMustHaveMod, VersionStrictness.Patch)]

    internal class CustomizedBronze : BaseUnityPlugin
    {

        // public values

        // info for valheim/bepinex that the game is modded
        public static bool isModded = true;

        // private values

        // new line
        private readonly static string s_CRLF = Environment.NewLine;
        private readonly static string s_CRLF2 = Environment.NewLine + Environment.NewLine;

        // config

        // enum for BronzeAlloy
        public enum BronzeAlloy
        {

            [Description("Default")]
            Default = 0,

            [Description("WoWlike")]
            WoWlike,

            [Description("Realistic")]
            Realistic,

            [Description("Custom")]
            Custom

        };

        // presets
        private static int[] DefaultAlloy = { 2, 1, 1 };
        private static int[] WoWlikeAlloy = { 1, 1, 2 };
        private static int[] RealisticAlloy = { 2, 1, 3 };

        // config strings
        private static string ConfigCategoryRecipeAlloy = "Recipe Alloy";

        private static string ConfigEntryRecipeAlloyBronze = "Recipe Bronze";
        private static string ConfigEntryRecipeAlloyBronzeDescription =
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
            "[If you select this , your custom settings will be used.]" + s_CRLF;

        private static string ConfigCategoryRecipeCustom = "Recipe Custom";

        private static string ConfigEntryRecipeCustomCopper = "Recipe Custom Copper";
        private static string ConfigEntryRecipeCustomCopperDescription = "Sets amount of needed of Copper.";
        private static string ConfigEntryRecipeCustomTin = "Recipe Custom Tin";
        private static string ConfigEntryRecipeCustomTinDescription = "Sets amount of needed of Tin.";
        private static string ConfigEntryRecipeCustomBronze = "Recipe Custom Bronze";
        private static string ConfigEntryRecipeCustomBronzeDescription = "Sets the amount of bronze produced.";

        // Configuration values
        public static ConfigEntry<bool> configModEnabled;
        public static ConfigEntry<int> configNexusID;
        public static ConfigEntry<bool> configShowChangesAtStartup;

        public static ConfigEntry<BronzeAlloy> configBronzeAlloy;

        public static ConfigEntry<int> configRecipeNeededCopper;
        public static ConfigEntry<int> configRecipeNeededTin;
        public static ConfigEntry<int> configRecipeProducedBronze;

        private static int usedRequirementCopper = 0;
        private static int usedRequirementTin = 0;
        private static int usedQuantityBronze = 0;

        #region[Awake]
        private void Awake()
        {


            if (DependencyOperations.CheckForDependencyErrors(PluginInfo.PLUGIN_GUID) == false)
            {

                CreateConfigValues();

                bool modEnabled = (bool)Config[Data.ConfigCategoryGeneral, Data.ConfigEntryEnabled].BoxedValue;

                if (modEnabled == true)
                {

                    // ##### plugin startup logic #####

#if (DEBUG)
                    Jotunn.Logger.LogInfo("Loading start");
#endif

                    // Event System

                    // ItemManager
                    ItemManager.OnItemsRegistered += OnItemsRegistered;
   
                    // ##### info functions #####

#if (DEBUG)
                    Jotunn.Logger.LogInfo("Loading done");
#endif

                    // Game data
#if (DEBUG)
                    Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} is active.");
#endif

                }
                else
                {

                    Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} is disabled by config.");

                }

            }

        }
        #endregion

        #region[Update]
        private void Update()
        {
        }
        #endregion

        #region[CreateConfigValues]
        // Create some sample configuration values
        private void CreateConfigValues()
        {

#if (DEBUG)
            Logger.LogInfo("CreateConfigValues");
#endif

            Config.SaveOnConfigSet = true;

            // Add client config which can be edited in every local instance independently
            configModEnabled = Config.Bind(
                Data.ConfigCategoryGeneral,
                Data.ConfigEntryEnabled,
                Data.ConfigEntryEnabledDefaultState,
                new ConfigDescription(Data.ConfigEntryEnabledDescription,
                    null,
                    new ConfigurationManagerAttributes
                    {
                        Order = 0,
                    })
            );

            configNexusID = Config.Bind(
                Data.ConfigCategoryGeneral,
                Data.ConfigEntryNexusID,
                Data.ConfigEntryNexusIDID,
                new ConfigDescription(Data.ConfigEntryNexusIDDescription,
                    null,
                    new ConfigurationManagerAttributes
                    {
                        Browsable = false,
                        Order = 1,
                        ReadOnly = true
                    }
                )
            );

            configShowChangesAtStartup = Config.Bind(
                Data.ConfigCategoryPlugin,
                Data.ConfigEntryShowChangesAtStartup,
                Data.ConfigEntryShowChangesAtStartupDefaultState,
                new ConfigDescription(Data.ConfigEntryShowChangesAtStartupDescription,
                    null,
                    new ConfigurationManagerAttributes
                    {
                        Order = 0
                    }
                )
            );

            configBronzeAlloy = Config.Bind(
                ConfigCategoryRecipeAlloy,
                ConfigEntryRecipeAlloyBronze,
                BronzeAlloy.Default,
                new ConfigDescription(ConfigEntryRecipeAlloyBronzeDescription,
                    null,
                    new ConfigurationManagerAttributes
                    {
                        DefaultValue = BronzeAlloy.Default,
                        Order = 0
                    }
                )
            );

            configRecipeNeededCopper = Config.Bind(
                ConfigCategoryRecipeCustom,
                ConfigEntryRecipeCustomCopper,
                DefaultAlloy[0],
                new ConfigDescription(ConfigEntryRecipeCustomCopperDescription,
                    null,
                    new ConfigurationManagerAttributes
                    {
                        Order = 0
                    }
                )
            );

            configRecipeNeededTin = Config.Bind(
                ConfigCategoryRecipeCustom,
                ConfigEntryRecipeCustomTin,
                DefaultAlloy[1],
                new ConfigDescription(ConfigEntryRecipeCustomTinDescription,
                    null,
                    new ConfigurationManagerAttributes
                    {
                        Order = 1
                    }
                )
            );

            configRecipeProducedBronze = Config.Bind(
                ConfigCategoryRecipeCustom,
                ConfigEntryRecipeCustomBronze,
                DefaultAlloy[2],
                new ConfigDescription(ConfigEntryRecipeCustomBronzeDescription,
                    null,
                    new ConfigurationManagerAttributes
                    {
                        Order = 2
                    }                    
                )                
            );

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
        private void ReadConfigValues()
        {

            // get state of showChangesAtStartup
            bool showChangesAtStartup = (bool)Config[Data.ConfigCategoryPlugin, Data.ConfigEntryShowChangesAtStartup].BoxedValue;

            // get BronzeAlloy config option
            BronzeAlloy bronzePreset = (BronzeAlloy)Config[ConfigCategoryRecipeAlloy, ConfigEntryRecipeAlloyBronze].BoxedValue;

            // check enum config option
            switch (bronzePreset)
            {

                case BronzeAlloy.Default:

                    if (showChangesAtStartup == true) { Logger.LogInfo("Default option selected"); }
                    usedRequirementCopper = DefaultAlloy[0];
                    usedRequirementTin = DefaultAlloy[1];
                    usedQuantityBronze = DefaultAlloy[2];

                    break;

                case BronzeAlloy.WoWlike:

                    if (showChangesAtStartup == true) { Logger.LogInfo("WoWlike option selected"); }
                    usedRequirementCopper = WoWlikeAlloy[0];
                    usedRequirementTin = WoWlikeAlloy[1];
                    usedQuantityBronze = WoWlikeAlloy[2];

                    break;

                case BronzeAlloy.Realistic:

                    if (showChangesAtStartup == true) { Logger.LogInfo("Realistic option selected"); }
                    usedRequirementCopper = RealisticAlloy[0];
                    usedRequirementTin = RealisticAlloy[1];
                    usedQuantityBronze = RealisticAlloy[2];

                    break;

                case BronzeAlloy.Custom:

                    if (showChangesAtStartup == true) { Logger.LogInfo("Custom option selected"); }

                    usedRequirementCopper = (int)Config[ConfigCategoryRecipeAlloy, ConfigEntryRecipeCustomCopper].BoxedValue;
                    usedRequirementTin = (int)Config[ConfigCategoryRecipeAlloy, ConfigEntryRecipeCustomTin].BoxedValue;
                    usedQuantityBronze = (int)Config[ConfigCategoryRecipeAlloy, ConfigEntryRecipeCustomBronze].BoxedValue;

                    break;

                default:

                    if (showChangesAtStartup == true) { Logger.LogInfo("unknown option selected"); }

                    break;

            }

#if (DEBUG)
            Logger.LogInfo(
                        "usedRequirementCopper = " + usedRequirementCopper +
                        ", usedRequirementTin = " + usedRequirementTin +
                        ", usedQuantityBronze = " + usedQuantityBronze);
#endif

        }
        #endregion

        #region[EventSystem]

        // ItemManager
        #region[OnItemsRegistered]
        private void OnItemsRegistered()
        {

            try
            {

#if (DEBUG)
                Logger.LogInfo("OnItemsRegistered");
#endif

                // read new quantities
                ReadConfigValues();

                // change recipe
                ChangeBronzeRecipe();

            }
            catch (Exception ex)
            {
                Logger.LogError($"Error OnItemsRegistered : {ex.Message}");
            }
            finally
            {
                PrefabManager.OnPrefabsRegistered -= OnItemsRegistered;
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
            bool showChangesAtStartup = (bool)Config[Data.ConfigCategoryPlugin, Data.ConfigEntryShowChangesAtStartup].BoxedValue;

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