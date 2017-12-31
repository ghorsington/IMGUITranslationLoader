using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityInjector.ConsoleUtil;

namespace IMGUITranslationLoader.Plugin.Utils
{
    public class LogLevel
    {
        public static readonly LogLevel Error = new LogLevel(ConsoleColor.Red);
        public static readonly LogLevel Minor = new LogLevel(ConsoleColor.DarkGray);
        public static readonly LogLevel Normal = new LogLevel(ConsoleColor.Gray);
        public static readonly LogLevel Warning = new LogLevel(ConsoleColor.Yellow);

        public LogLevel(ConsoleColor color)
        {
            Color = color;
        }

        public ConsoleColor Color { get; }
    }

    public static class Logger
    {
        public const string TAG = "IMGUITranslationLoader";
        private static Dictionary<string, HashSet<string>> cachedDumps;
        private static bool dumpEnabled;
        private static Dictionary<string, TextWriter> dumpStreams;
        private static bool inited;

        public static bool DumpEnabled
        {
            get => dumpEnabled;
            set
            {
                dumpEnabled = value;
                InitDump();
            }
        }

        public static string DumpPath { get; set; }

        public static bool Enabled { get; set; }

        public static bool InitDump()
        {
            if (!DumpEnabled)
                return false;
            if (inited)
                return true;
            try
            {
                if (!Directory.Exists(DumpPath))
                    Directory.CreateDirectory(DumpPath);
            }
            catch (Exception e)
            {
                WriteLine(LogLevel.Error, $"Failed to create dump directory because {e.Message}");
                return false;
            }
            dumpStreams = new Dictionary<string, TextWriter>();
            cachedDumps = new Dictionary<string, HashSet<string>>();
            inited = true;
            WriteLine("Initialized dumping");
            return true;
        }

        public static void Dispose()
        {
            if (!inited)
                return;

            foreach (KeyValuePair<string, TextWriter> stream in dumpStreams)
            {
                stream.Value.Flush();
                stream.Value.Dispose();
            }
        }

        public static void DumpLine(string line, string plugin)
        {
            if (!InitDump())
                return;
            if (!cachedDumps.TryGetValue(plugin, out HashSet<string> logged))
            {
                logged = new HashSet<string>();
                cachedDumps.Add(plugin, logged);
            }

            if (!dumpStreams.TryGetValue(plugin, out TextWriter tw))
                try
                {
                    string dumpFile = Path.Combine(DumpPath, $"{plugin}.txt");
                    if (File.Exists(dumpFile))
                    {
                        string[] lines = File.ReadAllLines(dumpFile);
                        foreach (string s in lines)
                        {
                            string str = s.Trim();
                            if (string.IsNullOrEmpty(str) || str.StartsWith(";"))
                                continue;

                            logged.Add(str.Split(new[] {'\t'}, StringSplitOptions.RemoveEmptyEntries)[0]);
                        }
                    }

                    tw = new StreamWriter(File.Open(dumpFile, FileMode.Append, FileAccess.Write));
                    dumpStreams.Add(plugin, tw);
                }
                catch (Exception e)
                {
                    WriteLine(LogLevel.Error, $"Failed to create or open dump file {plugin}.txt. Exception: {e}");
                    return;
                }

            line = line.Replace("\n", "").Trim().Escape();
            if (logged.Contains(line))
                return;
            logged.Add(line);
            lock (tw)
            {
                tw.WriteLine(line);
                tw.Flush();
            }
        }

        [Conditional("DEBUG")]
        public static void Debug(LogLevel logLevel, string message)
        {
            ConsoleColor oldColor = SafeConsole.ForegroundColor;
            SafeConsole.ForegroundColor = logLevel.Color;
            Console.WriteLine($"{TAG}::{message}");
            SafeConsole.ForegroundColor = oldColor;
        }

        public static void WriteLine(LogLevel logLevel, string message)
        {
            if (!Enabled)
                return;
            ConsoleColor oldColor = SafeConsole.ForegroundColor;
            SafeConsole.ForegroundColor = logLevel.Color;
            Console.WriteLine($"{TAG}::{message}");
            SafeConsole.ForegroundColor = oldColor;
        }

        public static void WriteLine(string message)
        {
            WriteLine(LogLevel.Normal, message);
        }
    }
}