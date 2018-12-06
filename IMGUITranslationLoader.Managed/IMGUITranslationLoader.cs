using System;
using System.IO;
using ExIni;
using IMGUITranslationLoader.Managed;
using IMGUITranslationLoader.Managed.Hooks;
using IMGUITranslationLoader.Plugin.Translation;
using IMGUITranslationLoader.Plugin.Utils;
using UnityEngine;
using Logger = IMGUITranslationLoader.Plugin.Utils.Logger;

namespace IMGUITranslationLoader.Plugin
{
    public class IMGUITranslationLoader : MonoBehaviour
    {
        public PluginConfiguration Settings { get; private set; }

        private TranslationMemory Memory { get; set; }

        private IniFile Preferences { get; set; }

        private string DataPath { get; } = Path.Combine(Environment.CurrentDirectory, "IMGUITranslationLoader");

        private string ConfigPath { get; set; }

        public void Awake()
        {
            DontDestroyOnLoad(this);

            if(!Directory.Exists(DataPath))
                Directory.CreateDirectory(DataPath);

            Preferences = !File.Exists(ConfigPath) ? new IniFile() : IniFile.FromFile(Path.Combine(DataPath, "IMGUITranslationLoader.ini"));

            Memory = new TranslationMemory(DataPath);
            
            ConfigPath = Path.Combine(DataPath, "IMGUITranslationLoader.ini");


            InitConfig();

            Memory.LoadTranslations();

            TranslationHooks.Translate = Translate;
            Logger.WriteLine("Hooking complete");
        }

        private void ReloadConfig()
        {
            if (!File.Exists(ConfigPath))
                return;
            IniFile ini = IniFile.FromFile(ConfigPath);
            Preferences.Merge(ini);
        }

        private void SaveConfig()
        {
            Preferences.Save(ConfigPath);
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Logger.WriteLine("Reloading config");
                ReloadConfig();
                InitConfig();
                if (Settings.EnableStringReload)
                {
                    Logger.WriteLine("Reloading translations");
                    Memory.LoadTranslations();
                }
                // TODO: Enable full translation reloading?
                /* 
                 * This is quite hard, as IMGUI renders everything immediately, and most of the objects that contain the GUI texts are removed almost instantly.
                 * Keeping track of all GUIContent that should be reloaded will vastly reduce the performance.
                 * Moreover, keepig track of a GUIContent does not guarantee that it is actually being used anymore (remember that GUI refreshes almost every frame).
                 * Testing the life cycle phase of an object is just too performance heavy for this real-time plug-in.
                 * 
                 * As of this writing we will just leave full translation reloading alone.
                 * Until there's a better suggestion as to how it should be done, that is.
                 */
                //TranslateExisting();
            }
        }

        public void OnDestroy()
        {
            Logger.Dispose();
        }

        private void InitConfig()
        {
            Settings = ConfigurationLoader.LoadConfig<PluginConfiguration>(Preferences);
            SaveConfig();
            TranslationHooks.GlobalMode = Settings.GlobalMode;
            Memory.GlobalMode = Settings.GlobalMode;
            Memory.CanLoad = Settings.Load;
            Memory.RetranslateText = Settings.EnableStringReload;
            Logger.DumpPath = Path.Combine(DataPath, "IMGUITranslationDumps");
            Logger.GlobalMode = Settings.GlobalMode;
            Logger.Enabled = Settings.EnableLogging;
            Logger.DumpEnabled = Settings.Dump;
        }

        private string Translate(string pluginName, string input)
        {
            string translation = Memory.GetTextTranslation(pluginName, input);
            if (Settings.Dump && input == translation)
                Logger.DumpLine(input, pluginName);
            return translation;
        }
    }
}