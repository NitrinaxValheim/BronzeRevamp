namespace Plugin
{ // start tag

    class Dependencies
    { // class start

        #region[PluginDependencies]

        // define needed Unity name
        public const string EngineName = "Unity";

        // define needed Unity version
        // define needed Unity version
#if (DEBUG)
        public const string EngineVersion = "2020.3.33f1";
#else
        public const string EngineVersion = "2020.3.33f1";
#endif

        // define needed Valheim name
        public const string GameName = "Valheim";

        // define needed Valheim version
#if (DEBUG)
        public const string GameVersion = "0.209.8";
#else
        public const string GameVersion = "0.209.8";
#endif

        // define needed BepInEx name
        public const string BepInExName = "BepInExPack Valheim";

        // define needed BepInEx version
#if (DEBUG)
        public const string BepInExVersion = "5.4.19.0";
#else
        public const string BepInExVersion = "5.4.19.0";
#endif

        #endregion

        #region[JotunnDependencies]

        // set to true if Jotunn required
        public const bool IsJotunnRequired = true;

        // define needed Jotunn name
        public const string JotunnName = "Jotunn, the Valheim Library";

        // define needed Jotunn version
#if (DEBUG)
        public const string JotunnVersion = "2.6.10";
#else
        public const string JotunnVersion = "2.6.10";
#endif

        #endregion

        #region[OtherPluginsDependencies]

        // set to true are other plugins required
        public const bool IsOtherPluginsRequired = false;

        // define mods that are needed
        public static string[] RequiredPlugins = {
            "",
        };

        #endregion

        #region[CustomFileDependencies]

        // set to true are other files required
        public const bool IsFilesRequired = false;

        // define needed files list
        public static string[] RequiredFiles = {
            "",
        };

        #endregion

    }

} // end tag