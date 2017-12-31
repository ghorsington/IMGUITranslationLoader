using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Inject;
using ReiPatcher;
using ReiPatcher.Patch;

namespace IMGUITranslationLoader.Patch
{
    public class Patcher : PatchBase
    {
        public const string TAG = "IMGUI_PATCHED";
        private const string HOOK_NAME = "IMGUITranslationLoader.Hook";

        public override string Name => "IMGUITranslationLoader Patcher";

        private static AssemblyDefinition HookAssembly { get; set; }

        public override bool CanPatch(PatcherArguments args)
        {
            return args.Assembly.Name.Name == "UnityEngine" && !HasAttribute(args.Assembly, TAG);
        }

        public override void Patch(PatcherArguments args)
        {
            TypeDefinition guiContent = args.Assembly.MainModule.GetType("UnityEngine.GUIContent");

            TypeDefinition hook = HookAssembly.MainModule.GetType($"{HOOK_NAME}.TranslationHooks");

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

            mText.IsPublic = true;
            mText.IsPrivate = false;
            guiContent.GetMethod("set_text").InjectWith(onTranslateText, flags: InjectFlags.PassParametersRef);

            mTooltip.IsPublic = true;
            mTooltip.IsPrivate = false;
            guiContent.GetMethod("set_tooltip").InjectWith(onTranslateText, flags: InjectFlags.PassParametersRef);

            SetPatchedAttribute(args.Assembly, TAG);
        }

        public override void PrePatch()
        {
            RPConfig.RequestAssembly("UnityEngine.dll");
            HookAssembly = AssemblyLoader.LoadAssembly(Path.Combine(AssembliesDir, $"{HOOK_NAME}.dll"));
        }

        private bool HasAttribute(AssemblyDefinition assembly, string tag)
        {
            return GetPatchedAttributes(assembly).Any(ass => ass.Info == tag);
        }
    }
}