using IMGUITranslationLoader.Plugin.Utils;

namespace IMGUITranslationLoader.Plugin
{
    [ConfigSection("Config")]
    public class PluginConfiguration
    {
        public bool Dump = false;
        public bool EnableStringReload = false;

        public bool Load = true;

        public bool Verbose = false;
    }
}