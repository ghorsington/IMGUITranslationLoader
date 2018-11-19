using System;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace IMGUITranslationLoader.Hook
{
    public class TranslationHooks
    {
        public delegate string Translator(string pluginName, string inputText);

        public static bool GlobalMode = false;
        public static bool InsideContentCtor;
        public static Translator Translate;

        public static void OnTranslateText(ref string text)
        {
            if (Translate == null)
                return;
            if (string.IsNullOrEmpty(text))
                return;

            string pluginName = null;

            if (!GlobalMode)
            {
                var frame = new StackFrame(2);
                var trace = new StackTrace(frame);
                MethodBase method = frame.GetMethod();
                if (method == null)
                    return;
                Type t = method.DeclaringType;
                pluginName = t.Assembly.GetName().Name.ToLowerInvariant();
                if (pluginName == "unityengine") // Most likely we're in TextEditor; Ignore the call in that case.
                    return;
            }

            text = Translate.Invoke(pluginName, text);
        }

        public static void OnTranslateTextMany(ref string[] texts)
        {
            if (Translate == null)
                return;
            string pluginName = null;

            if (!GlobalMode)
            {
                var frame = new StackFrame(2);
                var trace = new StackTrace(frame);
                MethodBase method = frame.GetMethod();
                if (method == null)
                    return;
                Type t = method.DeclaringType;
                pluginName = t.Assembly.GetName().Name.ToLowerInvariant();
                if (pluginName == "unityengine") // Most likely we're in TextEditor; Ignore the call in that case.
                    return;
            }

            for (int i = 0; i < texts.Length; i++)
            {
                string text = texts[i];
                if (string.IsNullOrEmpty(text))
                    continue;
                texts[i] = Translate?.Invoke(pluginName, text);
            }
        }

        public static void OnTranslateTextTooltip(ref string text, ref string tooltip)
        {
            InsideContentCtor = false;

            bool textEmpty = string.IsNullOrEmpty(text);
            bool tooltipEmpty = string.IsNullOrEmpty(tooltip);
            if (textEmpty && tooltipEmpty)
                return;

            string pluginName = null;

            if (!GlobalMode)
            {
                var frame = new StackFrame(2);
                var trace = new StackTrace(frame);
                MethodBase method = frame.GetMethod();
                if (method == null)
                    return;
                Type t = method.DeclaringType;
                pluginName = t.Assembly.GetName().Name.ToLowerInvariant();
                if (pluginName == "unityengine")
                {
                    frame = new StackFrame(3);
                    trace = new StackTrace(frame);
                    method = frame.GetMethod();
                    if (method == null)
                        return;
                    t = method.DeclaringType;
                    pluginName = t.Assembly.GetName().Name.ToLowerInvariant();
                }
            }

            if (!textEmpty && Translate != null)
                text = Translate.Invoke(pluginName, text);

            if (!tooltipEmpty && Translate != null)
                tooltip = Translate.Invoke(pluginName, text);
        }

        public static void OnTranslateGuiContent(ref string text)
        {
            if (InsideContentCtor)
                return;
            OnTranslateText(ref text);
        }
    }
}