namespace PluginDependencies
{ // start tag

    class PluginDependencies
    { // class start

        #region[PluginDependencies]

        // define needed Unity name
        public const string EngineName = "Unity";

        // define needed Unity version
        public const string EngineVersion = "2020.3.33f1";

        // define needed Valheim name
        public const string GameName = "Valheim";

        // define needed Valheim version
        public const string GameVersion = "0.209.5";

        // define needed BepInEx name
        public const string BepInExName = "BepInExPack Valheim";

        // define needed BepInEx version
        public const string BepInExVersion = "5.4.19.0";

        #endregion

        #region[JotunnDependencies]

        // set to true if Jotunn required
        public const bool IsJotunnRequired = true;

        // define needed Jotunn name
        public const string JotunnName = "Jotunn, the Valheim Library";

        // define needed Jotunn version
        public const string JotunnVersion = "2.6.7";

        #endregion

        #region[OtherPluginsDependencies]

        // set to true are other plugins required
        public const bool IsOtherPluginsRequired = true;

        // define mods that are needed
        public static string[] RequiredPlugins = { 
            "",
        };

        #endregion

        #region[CustomFileDependencies]

        // set to true are other files required
        public const bool IsFilesRequired = true;

        // define needed files list
        public static string[] RequiredFiles = {
            "",
        };

        #endregion

    }

} // end tag