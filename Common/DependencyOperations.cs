// System
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

// BepInEx
using BepInEx;

using Plugin;

namespace Common
{
    public class DependencyOperations
    {

        public static bool CheckForDependencyErrors(string pluginName) {

            bool dependenciesError = false;

            //PluginDependencies.Dependencies.EngineName;
            //PluginDependencies.Dependencies.EngineVersion;
            if (VersionOperations.CheckVersionWithOutput(
                pluginName,
                Plugin.Dependencies.EngineName,
                VersionOperations.GetEngineVersion(),
                Plugin.Dependencies.EngineVersion
                ) == false) { dependenciesError = true;  }

            //PluginDependencies.Dependencies.GameName
            //PluginDependencies.Dependencies.GameVersion
            if (VersionOperations.CheckVersionWithOutput(
                pluginName,
                Plugin.Dependencies.GameName,
                VersionOperations.GetGameVersion(),
                Plugin.Dependencies.GameVersion
                ) == false) { dependenciesError = true; }

            //PluginDependencies.Dependencies.BepInExName
            //PluginDependencies.Dependencies.BepInExVersion
            if (VersionOperations.CheckVersionWithOutput(
                pluginName,
                Plugin.Dependencies.BepInExName,
                VersionOperations.GetBepinExVersion(),
                Plugin.Dependencies.BepInExVersion
                ) == false) { dependenciesError = true; }

            //PluginDependencies.Dependencies.IsJotunnRequired
            //PluginDependencies.Dependencies.JotunnName
            //PluginDependencies.Dependencies.JotunnVersion
            if (Plugin.Dependencies.IsJotunnRequired == true)
            {
                if (VersionOperations.CheckVersionWithOutput(
                    pluginName,
                    Plugin.Dependencies.JotunnName,
                    VersionOperations.GetJotunnVersion(),
                    Plugin.Dependencies.JotunnVersion
                    ) == false) { dependenciesError = true; }
            }

            //PluginDependencies.Dependencies.IsOtherPluginsRequired
            //PluginDependencies.Dependencies.RequiredPlugins
            if (Plugin.Dependencies.IsOtherPluginsRequired == true)
            {

                if (FileOperations.checkPluginDependencies(
                    Plugin.Dependencies.RequiredPlugins
                    ) == false) { dependenciesError = true; }

            }

            //PluginDependencies.Dependencies.IsFilesRequired
            //PluginDependencies.Dependencies.RequiredFiles
            if (Plugin.Dependencies.IsFilesRequired == true)
            {
                if (FileOperations.checkPluginDependencies(
                    Plugin.Dependencies.RequiredFiles
                    ) == false) { dependenciesError = true; }
            }

            return dependenciesError;
        
        }

    }

}
