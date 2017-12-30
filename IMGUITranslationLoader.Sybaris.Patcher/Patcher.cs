using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Inject;

namespace IMGUITranslationLoader.Sybaris.Patcher
{
    public static class Patcher
    {
        public static readonly string[] TargetAssemblyNames = {"UnityEngine.dll"};
        private const string HOOK_NAME = "IMGUITranslationLoader.Hook";

        private const string SYBARIS_MANAGED_DIR = @"..\Plugins\Managed";

        public static void Patch(AssemblyDefinition assembly)
        {
            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string hookDir = $"{SYBARIS_MANAGED_DIR}\\{HOOK_NAME}.dll";
            AssemblyDefinition hookAssembly = AssemblyLoader.LoadAssembly(Path.Combine(assemblyDir, hookDir));

            TypeDefinition guiContent = assembly.MainModule.GetType("UnityEngine.GUIContent");

            TypeDefinition hook = hookAssembly.MainModule.GetType($"{HOOK_NAME}.TranslationHooks");

            MethodDefinition onTranslateTextTooltip = hook.GetMethod("OnTranslateTextTooltip");
            MethodDefinition onTranslateText = hook.GetMethod("OnTranslateText");
            MethodDefinition onTranslateTempText = hook.GetMethod("OnTranslateTempText");

            FieldDefinition mText = guiContent.GetField("m_Text");
            FieldDefinition mTooltip = guiContent.GetField("m_Tooltip");
            FieldDefinition sText = guiContent.GetField("s_Text");

            guiContent.GetMethods(".ctor").ForEach(m => m.InjectWith(onTranslateTextTooltip,
                                                                     -1,
                                                                     flags: InjectFlags.PassFields,
                                                                     typeFields: new[] {mText, mTooltip}));

            sText.IsPublic = true;
            sText.IsPrivate = false;
            guiContent.GetMethods("Temp").Where(m => m.Parameters.Any(p => p.ParameterType.FullName == "System.String"))
                      .ForEach(m => m.InjectWith(onTranslateTempText, -1));

            guiContent.GetMethod("set_text").InjectWith(onTranslateText, flags: InjectFlags.PassParametersRef);
            guiContent.GetMethod("set_tooltip").InjectWith(onTranslateText, flags: InjectFlags.PassParametersRef);
        }
    }
}