using IMGUITranslationLoader.Plugin.Utils;

namespace IMGUITranslationLoader.Plugin
{
    [ConfigSection("Config")]
    public class PluginConfiguration
    {
        public bool Dump = false;

        public bool EnableLogging = true;

        public bool EnableStringReload = false;

        public bool GlobalMode = false;

        public bool Load = true;
    }
}