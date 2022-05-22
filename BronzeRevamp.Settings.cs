namespace BronzeRevamp.Settings
{ // start tag

    class ModData
    { // class start

        #region[ModData]

        public const string Guid = Company + "." + Namespace;

        public const string Namespace = "BronzeRevamp";

        public const string ModName = "BronzeRevamp";

        public const string Version = "0.0.0.3";

        public const string Description = "Allows the user to customize the bronze recipe.";

#if (DEBUG)

        public const string Configuration = "Debug";

#else

        public const string Configuration = "Release";

#endif

        public const string Company = "Nitrinax";

        public const string Copyright = "GNU GENERAL PUBLIC LICENSE Version 3";

        public const string Trademark = "";

        public const string Culture = "";

        public const string Language = "";

        #endregion

        #region[ConfigData]

        //default language for this mod. Possible options are English, German
        public const string ModDefaultLanguage = "English";

        //default state of this mod
        public const bool ModDefaultState = true;

        #endregion

    }

} // end tag