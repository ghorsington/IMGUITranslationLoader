using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Inject;

namespace IMGUITranslationLoader.Sybaris.Patcher
{
    public static class Patcher
    {
        public static readonly string[] TargetAssemblyNames = {"UnityEngine.dll"};
        private const string HOOK_NAME = "IMGUITranslationLoader.Hook";

        private static void PatchGui(TypeDefinition gui, TypeDefinition hook)
        {
            MethodDefinition onTranslateText = hook.GetMethod("OnTranslateText");
            MethodDefinition onTranslateTextMany = hook.GetMethod("OnTranslateTextMany");

            foreach (MethodDefinition guiMethod in gui.Methods)
            {
                if (!guiMethod.IsPublic || !guiMethod.HasBody)
                    continue;
                if (guiMethod.Name.ToLowerInvariant().Contains("field")
                    || guiMethod.Name.ToLowerInvariant().Contains("area"))
                    continue;

                ParameterDefinition stringParam =
                        guiMethod.Parameters.FirstOrDefault(p => p.ParameterType.FullName == "System.String"
                                                                 || p.ParameterType.FullName == "System.String[]");

                if (stringParam == null)
                    continue;

                MethodDefinition target = stringParam.ParameterType.IsArray ? onTranslateTextMany : onTranslateText;

                ILProcessor il = guiMethod.Body.GetILProcessor();
                Instruction first = guiMethod.Body.Instructions.First();
                il.InsertBefore(first, il.Create(OpCodes.Ldarga, stringParam));
                il.InsertBefore(first, il.Create(OpCodes.Call, gui.Module.ImportReference(target)));
            }
        }

        public static void Patch(AssemblyDefinition assembly)
        {
            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string hookDir = $"{HOOK_NAME}.dll";
            AssemblyDefinition hookAssembly = AssemblyLoader.LoadAssembly(Path.Combine(assemblyDir, hookDir));

            TypeDefinition guiContent = assembly.MainModule.GetType("UnityEngine.GUIContent");

            TypeDefinition hook = hookAssembly.MainModule.GetType($"{HOOK_NAME}.TranslationHooks");

            FieldDefinition insideCtorField = hook.GetField("InsideContentCtor");
            MethodDefinition onTranslateTextTooltip = hook.GetMethod("OnTranslateTextTooltip");
            MethodDefinition onTranslateContentText = hook.GetMethod("OnTranslateGuiContent");

            PatchGui(assembly.MainModule.GetType("UnityEngine.GUI"), hook);
            PatchGui(assembly.MainModule.GetType("UnityEngine.GUILayout"), hook);

            FieldDefinition mText = guiContent.GetField("m_Text");
            FieldDefinition mTooltip = guiContent.GetField("m_Tooltip");

            MethodDefinition ctorString =
                    guiContent.GetMethod(".ctor", "System.String", "UnityEngine.Texture", "System.String");
            ILProcessor il = ctorString.Body.GetILProcessor();
            Instruction firstIns = ctorString.Body.Instructions.First();
            il.InsertBefore(firstIns, il.Create(OpCodes.Ldc_I4_1));
            il.InsertBefore(firstIns, il.Create(OpCodes.Stsfld, assembly.MainModule.ImportReference(insideCtorField)));
            ctorString.InjectWith(onTranslateTextTooltip,
                                  -1,
                                  flags: InjectFlags.PassFields,
                                  typeFields: new[] {mText, mTooltip});

            MethodDefinition ctorObj = guiContent.GetMethod(".ctor", "UnityEngine.GUIContent");
            il = ctorObj.Body.GetILProcessor();
            firstIns = ctorObj.Body.Instructions.First();
            il.InsertBefore(firstIns, il.Create(OpCodes.Ldc_I4_1));
            il.InsertBefore(firstIns, il.Create(OpCodes.Stsfld, assembly.MainModule.ImportReference(insideCtorField)));
            ctorObj.InjectWith(onTranslateTextTooltip,
                               -1,
                               flags: InjectFlags.PassFields,
                               typeFields: new[] {mText, mTooltip});

            guiContent.GetMethod("set_text").InjectWith(onTranslateContentText, flags: InjectFlags.PassParametersRef);

            guiContent.GetMethod("set_tooltip")
                      .InjectWith(onTranslateContentText, flags: InjectFlags.PassParametersRef);
        }
    }
}