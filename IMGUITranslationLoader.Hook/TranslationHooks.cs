using System;
using System.Diagnostics;
using UnityEngine;

namespace IMGUITranslationLoader.Hook
{
    public class StringTranslationEventArgs : EventArgs
    {
        public string PluginName { get; internal set; }
        public string Text { get; internal set; }

        public string Translation { get; set; }
    }

    public class TranslationHooks
    {
        private const string PREFIX = "\u200B";
        private static readonly int PREFIX_LEN = PREFIX.Length;
        public static event EventHandler<StringTranslationEventArgs> TranslateText;

        public static void OnTranslateText(ref string text)
        {
            if (text.StartsWith(PREFIX))
            {
                text = text.Substring(PREFIX_LEN);
                return;
            }

            string pluginName = GetClosestExternalType(new StackTrace());

            string translation = OnTranslate(text, pluginName);
            if (!string.IsNullOrEmpty(translation))
                text = translation;
        }

        public static void OnTranslateTextTooltip(ref string text, ref string tooltip)
        {
            bool textEmpty = string.IsNullOrEmpty(text);
            bool tooltipEmpty = string.IsNullOrEmpty(tooltip);
            if (textEmpty && tooltipEmpty)
                return;

            string pluginName = GetClosestExternalType(new StackTrace());

            if (!textEmpty)
            {
                string textTr = OnTranslate(text, pluginName);
                if (!string.IsNullOrEmpty(textTr))
                    text = textTr;
            }

            if (!tooltipEmpty)
            {
                string textTp = OnTranslate(tooltip, pluginName);
                if (!string.IsNullOrEmpty(textTp))
                    tooltip = textTp;
            }
        }

        public static void OnTranslateTempText()
        {
            bool textEmpty = string.IsNullOrEmpty(GUIContent.s_Text.text);
            bool tooltipEmpty = string.IsNullOrEmpty(GUIContent.s_Text.tooltip);
            if (textEmpty && tooltipEmpty)
                return;

            string pluginName = GetClosestExternalType(new StackTrace());

            if (!textEmpty)
            {
                string textTr = OnTranslate(GUIContent.s_Text.text, pluginName);
                if (!string.IsNullOrEmpty(textTr))
                    GUIContent.s_Text.text = PREFIX + textTr;
            }

            if (!tooltipEmpty)
            {
                string textTp = OnTranslate(GUIContent.s_Text.tooltip, pluginName);
                if (!string.IsNullOrEmpty(textTp))
                    GUIContent.s_Text.tooltip = PREFIX + textTp;
            }
        }

        private static string GetClosestExternalType(StackTrace trace)
        {
            foreach (StackFrame frame in trace.GetFrames())
            {
                Type t = frame.GetMethod().DeclaringType;
                if (t == null)
                    continue;
                string assName = t.Assembly.GetName().Name;
                if (assName == "IMGUITranslationLoader.Hook" || assName == "UnityEngine")
                    continue;
                return assName.ToLowerInvariant();
            }

            return string.Empty;
        }

        private static string OnTranslate(string text, string plugin)
        {
            StringTranslationEventArgs args = new StringTranslationEventArgs {PluginName = plugin, Text = text};

            TranslateText?.Invoke(null, args);

            return args.Translation;
        }
    }
}