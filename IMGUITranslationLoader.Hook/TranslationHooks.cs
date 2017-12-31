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
            if (string.IsNullOrEmpty(text))
                return;

            StackFrame frame = new StackFrame(2);
            StackTrace trace = new StackTrace(frame);
            Type t = frame.GetMethod().DeclaringType;
            if (t == null)
                return;
            string pluginName = t.Assembly.GetName().Name.ToLowerInvariant();
            if (pluginName == "unityengine") // Most likely we're in TextEditor; Ignore the call in that case.
                return;

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

            StackFrame frame = new StackFrame(2);
            StackTrace trace = new StackTrace(frame);
            Type t = frame.GetMethod().DeclaringType;
            if (t == null)
                return;
            string pluginName = t.Assembly.GetName().Name.ToLowerInvariant();

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
            bool textEmpty = string.IsNullOrEmpty(GUIContent.s_Text.m_Text);
            bool tooltipEmpty = string.IsNullOrEmpty(GUIContent.s_Text.m_Tooltip);
            if (textEmpty && tooltipEmpty)
                return;

            StackFrame frame = new StackFrame(3); // Since Temp is called indirectly by UnityEngine, the original plugin is one frame lower
            StackTrace trace = new StackTrace(frame);
            Type t = frame.GetMethod().DeclaringType;
            if (t == null)
                return;
            string pluginName = t.Assembly.GetName().Name.ToLowerInvariant();

            if (!textEmpty)
            {
                string textTr = OnTranslate(GUIContent.s_Text.m_Text, pluginName);
                if (!string.IsNullOrEmpty(textTr))
                    GUIContent.s_Text.m_Text = textTr;
            }

            if (!tooltipEmpty)
            {
                string textTp = OnTranslate(GUIContent.s_Text.m_Tooltip, pluginName);
                if (!string.IsNullOrEmpty(textTp))
                    GUIContent.s_Text.m_Tooltip = textTp;
            }
        }

        private static string OnTranslate(string text, string plugin)
        {
            StringTranslationEventArgs args = new StringTranslationEventArgs {PluginName = plugin, Text = text};

            TranslateText?.Invoke(null, args);

            return args.Translation;
        }
    }
}