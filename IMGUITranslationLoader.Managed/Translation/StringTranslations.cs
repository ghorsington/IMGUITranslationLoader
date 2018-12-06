using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using IMGUITranslationLoader.Plugin.Utils;

namespace IMGUITranslationLoader.Plugin.Translation
{
    public class StringTranslations
    {
        private readonly Dictionary<Regex, string> loadedRegexTranslations;
        private readonly Dictionary<string, string> loadedStringTranslations;

        public StringTranslations(string pluginName)
        {
            PluginName = pluginName;
            loadedRegexTranslations = new Dictionary<Regex, string>();
            loadedStringTranslations = new Dictionary<string, string>();
        }

        public int LoadedRegexCount => loadedRegexTranslations.Count;

        public int LoadedStringCount => loadedStringTranslations.Count;

        public int LoadedTranslationCount => loadedStringTranslations.Count + loadedRegexTranslations.Count;

        public string PluginName { get; }

        public bool TranslationsLoaded { get; private set; }

        public bool TryTranslate(string original, out string result)
        {
            if (!TranslationsLoaded)
            {
                result = string.Empty;
                return false;
            }

            if (loadedStringTranslations.TryGetValue(original, out result))
                return true;

            foreach (KeyValuePair<Regex, string> regexTranslation in loadedRegexTranslations)
                if (regexTranslation.Key.IsMatch(original))
                {
                    result = regexTranslation.Key.Replace(original, regexTranslation.Value);
                    return true;
                }

            return false;
        }

        public void AddTranslationFile(string filePath)
        {
            if (LoadFromFile(filePath))
                TranslationsLoaded = true;
        }

        public void ClearTranslations()
        {
            loadedRegexTranslations.Clear();
            loadedStringTranslations.Clear();
            TranslationsLoaded = false;

            Logger.WriteLine($"StringTranslations::Unloaded translations for {PluginName}");
        }

        private bool LoadFromFile(string filePath)
        {
            IEnumerable<string> translationLines = File.ReadAllLines(filePath, Encoding.UTF8);
            int translated = 0;
            foreach (string translationLine in translationLines)
            {
                string line = translationLine.Trim();
                if (line.Length == 0 || line.StartsWith(";", StringComparison.InvariantCulture))
                    continue;

                string[] textParts = translationLine.Split(new[] {'\t'}, StringSplitOptions.RemoveEmptyEntries);
                if (textParts.Length < 2)
                    continue;
                string original = textParts[0].Unescape();
                string translation = textParts[1].Unescape().Trim();
                if (string.IsNullOrEmpty(translation))
                    continue;

                if (original.StartsWith("$", StringComparison.CurrentCulture))
                {
                    loadedRegexTranslations.AddIfNotPresent(new Regex(original.Substring(1), RegexOptions.Compiled),
                                                            translation);
                    translated++;
                }
                else
                {
                    loadedStringTranslations.AddIfNotPresent(original, translation);
                    translated++;
                }
            }

            if (translated != 0)
                return true;
            return false;
        }
    }
}