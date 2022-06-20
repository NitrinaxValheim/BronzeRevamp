// System
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Globalization;

// UnityEngine
using UnityEngine;
using UnityEngine.SceneManagement;

// BepInEx
using BepInEx;
using BepInEx.Configuration;

// Jotunn
using Jotunn;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.GUI;
using Jotunn.Managers;
using Jotunn.Utils;
using Logger = Jotunn.Logger;

using Plugin;

namespace Common
{
    public class FileOperations
    {

        #region[checkPluginDependencies]
        public static Boolean checkPluginDependencies(string[] plugins)
        {

            bool allPluginsFound = true;

            foreach (string plugin in plugins)
            {

#if (DEBUG)
                Logger.LogInfo("plugin = " + plugin);
#endif

                string[] entries = Directory.GetFiles(@BepInEx.Paths.PluginPath, plugin, SearchOption.AllDirectories);

                foreach (string entry in entries)
                {

#if (DEBUG)
                    Logger.LogInfo("entry = " + entry);
#endif

                    if (!entry.Contains(plugin))
                    {

                        Logger.LogInfo("Needed plugin " + plugin + " not installed.");

                        allPluginsFound = false;

                    }

                }

            }

            return allPluginsFound;

        }
        #endregion

        #region[checkFileDependencies]
        public static Boolean checkFileDependencies(string[] files)
        {

            bool allFilesFound = true;

            foreach (string file in files)
            {

                var fileName = BepInEx.Paths.PluginPath + Path.DirectorySeparatorChar + Data.Namespace + Path.DirectorySeparatorChar + file;

#if (DEBUG)
                Logger.LogInfo("fileName = " + fileName);
#endif

                if (File.Exists(fileName) == false)
                {

                    Logger.LogError("File " + fileName + " not exists.");

                    allFilesFound = false;

                }

            }

            return allFilesFound;

        }
        #endregion

    }

}
