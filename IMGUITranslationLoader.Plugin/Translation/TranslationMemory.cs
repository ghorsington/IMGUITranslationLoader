using System;
using System.Collections.Generic;
using System.IO;
using IMGUITranslationLoader.Plugin.Utils;

namespace IMGUITranslationLoader.Plugin.Translation
{
    public class TranslationMemory
    {
        private const string STRINGS_FOLDER = "IMGUIStrings";

        private readonly StringTranslations globalTranslations;
        private readonly Dictionary<string, StringTranslations> stringGroups;
        private readonly Dictionary<string, string> translatedStrings;

        public bool CanLoad;

        public bool GlobalMode;

        public bool RetranslateText;

        private bool isDirectoriesChecked;
        private string stringsPath;
        private string translationsPath;

        public TranslationMemory(string translationPath)
        {
            TranslationsPath = translationPath;
            translatedStrings = new Dictionary<string, string>();
            stringGroups = new Dictionary<string, StringTranslations>();
            globalTranslations = new StringTranslations("global");
        }

        public string TranslationsPath
        {
            get => translationsPath;
            set
            {
                translationsPath = value;
                stringsPath = Path.Combine(translationsPath, STRINGS_FOLDER);
            }
        }

        public void LoadTranslations()
        {
            CheckDirectories();
            if (CanLoad)
                LoadStringTranslations();
        }

        public string GetTextTranslation(string plugin, string original)
        {
            StringTranslations translations = null;
            if (!GlobalMode && !stringGroups.TryGetValue(plugin, out translations))
                return original;
            if (GlobalMode)
                translations = globalTranslations;

            bool wasTranslated = translatedStrings.ContainsKey(original);
            string input = original;
            if (RetranslateText)
                input = wasTranslated ? translatedStrings[input] : input;
            else if (wasTranslated)
            {
                Logger.Debug(LogLevel.Minor, $"String::Skip {original} (is already translated)");
                return original;
            }

            Logger.Debug(LogLevel.Minor, $"FindString::{input.Escape()}");

            if (!translations.TryTranslate(input, out string translation))
                return input;
            translatedStrings[translation] = input;
            return translation;
        }

        public bool TryGetOriginal(string translation, out string original) =>
                translatedStrings.TryGetValue(translation, out original);

        public bool WasTranslated(string translation) => translatedStrings.ContainsKey(translation);

        private void CheckDirectories()
        {
            if (isDirectoriesChecked)
                return;
            isDirectoriesChecked = true;

            void InitDir(string dir)
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }

            try
            {
                InitDir(stringsPath);
            }
            catch (Exception e)
            {
                Logger.WriteLine(LogLevel.Error, $"Directory_Load_Fail::{e}");
            }
        }

        private void LoadStringTranslations()
        {
            globalTranslations.ClearTranslations();

            foreach (KeyValuePair<string, StringTranslations> group in stringGroups)
                group.Value.ClearTranslations();

            int loadedFiles = 0;
            foreach (string translationPath in Directory.GetFiles(stringsPath, "*.txt", SearchOption.AllDirectories))
            {
                string fileName = Path.GetFileNameWithoutExtension(translationPath);

                Logger.WriteLine($"CacheString::'{fileName}");

                if (GlobalMode)
                {
                    globalTranslations.AddTranslationFile(translationPath);
                    loadedFiles++;
                }
                else
                {
                    if (!stringGroups.TryGetValue(fileName, out StringTranslations group))
                    {
                        group = new StringTranslations(fileName);
                        stringGroups.Add(fileName.ToLowerInvariant(), group);
                    }

                    group.AddTranslationFile(translationPath);

                    loadedFiles++;
                }
            }

            Logger.WriteLine($"Strings::Loaded '{loadedFiles}' Translation files");
        }
    }
}