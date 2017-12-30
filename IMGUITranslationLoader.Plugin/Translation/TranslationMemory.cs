using System;
using System.Collections.Generic;
using System.IO;
using IMGUITranslationLoader.Plugin.Utils;

namespace IMGUITranslationLoader.Plugin.Translation
{
    public class TranslationMemory
    {
        private static readonly int[] NoLevels = new int[0];
        private const string STRINGS_FOLDER = "IMGUIStrings";

        private readonly StringTranslations globalTranslations;

        private bool isDirectoriesChecked;
        private readonly Dictionary<string, StringTranslations> stringGroups;
        private string stringsPath;
        private readonly Dictionary<string, string> translatedStrings;
        private string translationsPath;

        public TranslationMemory(string translationPath)
        {
            TranslationsPath = translationPath;
            translatedStrings = new Dictionary<string, string>();
            stringGroups = new Dictionary<string, StringTranslations>();
            globalTranslations = new StringTranslations("global");
        }

        public bool CanLoad { get; set; }

        public bool RetranslateText { get; set; }

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

        public TextTranslation GetTextTranslation(string plugin, string original)
        {
            string Translate(string text, string from)
            {
                Logger.WriteLine($"String::'{from}'->'{text}'");
                translatedStrings[text] = from;
                return text;
            }

            TextTranslation result = new TextTranslation {Result = TranslationResult.Ok};

            bool wasTranslated = translatedStrings.ContainsKey(original);
            string untranslated = original;
            if (RetranslateText)
            {
                untranslated = wasTranslated ? translatedStrings[untranslated] : untranslated;
            }
            else if (wasTranslated)
            {
                Logger.WriteLine(LogLevel.Minor, $"String::Skip {original} (is already translated)");
                result.Result = TranslationResult.Translated;
                return result;
            }
            result.Text = untranslated;
            string input = untranslated.Replace("\n", "").Trim();

            Logger.WriteLine(LogLevel.Minor, $"FindString::{untranslated}");

            if (string.IsNullOrEmpty(input))
            {
                result.Result = TranslationResult.NotFound;
                return result;
            }

            StringTranslations translations = stringGroups.TryGetValue(plugin, out StringTranslations v) ? v : globalTranslations;

            if (translations.TryTranslate(input, out string translation))
                result.Text = Translate(translation, untranslated);
            else
                result.Result = TranslationResult.NotFound;

            return result;
        }

        public bool TryGetOriginal(string translation, out string original)
        {
            return translatedStrings.TryGetValue(translation, out original);
        }

        public bool WasTranslated(string translation)
        {
            return translatedStrings.ContainsKey(translation);
        }

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

                if (fileName == "global")
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