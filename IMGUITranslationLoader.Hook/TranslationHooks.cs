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
        public delegate string Translator(string pluginName, string inputText);

        public static bool GlobalMode = false;
        public static Translator Translate;

        public static void OnTranslateText(ref string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            string pluginName = null;

            if (!GlobalMode)
            {
                StackFrame frame = new StackFrame(2);
                StackTrace trace = new StackTrace(frame);
                Type t = frame.GetMethod().DeclaringType;
                if (t == null)
                    return;
                pluginName = t.Assembly.GetName().Name.ToLowerInvariant();
                if (pluginName == "unityengine") // Most likely we're in TextEditor; Ignore the call in that case.
                    return;
            }

            text = Translate?.Invoke(pluginName, text);
        }

        public static void OnTranslateTextTooltip(ref string text, ref string tooltip)
        {
            bool textEmpty = string.IsNullOrEmpty(text);
            bool tooltipEmpty = string.IsNullOrEmpty(tooltip);
            if (textEmpty && tooltipEmpty)
                return;

            string pluginName = null;

            if (!GlobalMode)
            {
                StackFrame frame = new StackFrame(2);
                StackTrace trace = new StackTrace(frame);
                Type t = frame.GetMethod().DeclaringType;
                if (t == null)
                    return;
                pluginName = t.Assembly.GetName().Name.ToLowerInvariant();
                if (pluginName == "unityengine") // Some dropdowns are created with Temp(string[])
                {
                    // Creat a new stack frame an trace it
                    // Copy-paste to reduce additional frames
                    StackFrame frame2 = new StackFrame(4);
                    StackTrace trace2 = new StackTrace(frame2);
                    Type tt = frame2.GetMethod().DeclaringType;
                    if (tt == null)
                        return;
                    pluginName = tt.Assembly.GetName().Name.ToLowerInvariant();
                }
            }

            if (!textEmpty)
                text = Translate?.Invoke(pluginName, text);

            if (!tooltipEmpty)
                tooltip = Translate?.Invoke(pluginName, text);
        }

        public static void OnTranslateTempText()
        {
            bool textEmpty = string.IsNullOrEmpty(GUIContent.s_Text.m_Text);
            bool tooltipEmpty = string.IsNullOrEmpty(GUIContent.s_Text.m_Tooltip);
            if (textEmpty && tooltipEmpty)
                return;

            string pluginName = null;

            if (!GlobalMode)
            {
                StackFrame
                        frame = new StackFrame(3); // Since Temp is called indirectly by UnityEngine, the original plugin is one frame lower
                StackTrace trace = new StackTrace(frame);
                Type t = frame.GetMethod().DeclaringType;
                if (t == null)
                    return;
                pluginName = t.Assembly.GetName().Name.ToLowerInvariant();
            }

            if (!textEmpty)
                GUIContent.s_Text.m_Text = Translate?.Invoke(pluginName, GUIContent.s_Text.m_Text);

            if (!tooltipEmpty)
                GUIContent.s_Text.m_Tooltip = Translate?.Invoke(pluginName, GUIContent.s_Text.m_Tooltip);
        }
    }
}