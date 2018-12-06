using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Inject;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace IMGUITranslationLoader.Sybaris.Patcher
{
    public static class Patcher
    {
        public static readonly string[] TargetAssemblyNames = {"UnityEngine.dll"};
        private const string HOOK_NAME = "IMGUITranslationLoader.Managed";

        public static void Patch(AssemblyDefinition assembly)
        {
            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string hookDir = $"{HOOK_NAME}.dll";
            AssemblyDefinition hookAssembly = AssemblyLoader.LoadAssembly(Path.Combine(assemblyDir, hookDir));

            TypeDefinition guiContent = assembly.MainModule.GetType("UnityEngine.GUIContent");

            TypeDefinition hook = hookAssembly.MainModule.GetType($"{HOOK_NAME}.Hooks.TranslationHooks");

            MethodDefinition onTranslateContentText = hook.GetMethod("OnTranslateGuiContent");

            PatchGui(assembly.MainModule.GetType("UnityEngine.GUI"), hook);
            PatchGui(assembly.MainModule.GetType("UnityEngine.GUILayout"), hook);
            PatchGUIContent(guiContent, hook);

            guiContent.GetMethod("set_text").InjectWith(onTranslateContentText, flags: InjectFlags.PassParametersRef);

            guiContent.GetMethod("set_tooltip")
                      .InjectWith(onTranslateContentText, flags: InjectFlags.PassParametersRef);
        }

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

        private static void PatchGUIContent(TypeDefinition guiContent, TypeDefinition hook)
        {
            MethodDefinition onTranslateTextTooltip = hook.GetMethod("OnTranslateTextTooltip");
            FieldDefinition mText = guiContent.GetField("m_Text");
            FieldDefinition mTooltip = guiContent.GetField("m_Tooltip");
            FieldDefinition insideCtorField = hook.GetField("InsideContentCtor");

            foreach (MethodDefinition ctor in guiContent.Methods.Where(m => m.IsConstructor
                                                                            && !m.IsStatic
                                                                            && m.Parameters.Count > 0))
            {
                if (!ctor.HasBody)
                    continue;
                MethodBody body = ctor.Body;

                if (ctor.Parameters.All(p => p.ParameterType.FullName != "System.String"
                                             && p.ParameterType.FullName != "UnityEngine.GUIContent"))
                    continue;

                if (body.Instructions.Any(ins => ins.OpCode == OpCodes.Call
                                                 && ((MethodReference) ins.Operand).Name == ".ctor"
                                                 && ((MethodReference) ins.Operand).DeclaringType.FullName
                                                 == guiContent.FullName))
                    continue;

                ILProcessor il = ctor.Body.GetILProcessor();
                Instruction firstIns = ctor.Body.Instructions.First();
                il.InsertBefore(firstIns, il.Create(OpCodes.Ldc_I4_1));
                il.InsertBefore(firstIns,
                                il.Create(OpCodes.Stsfld, guiContent.Module.ImportReference(insideCtorField)));
                ctor.InjectWith(onTranslateTextTooltip,
                                -1,
                                flags: InjectFlags.PassFields,
                                typeFields: new[] {mText, mTooltip});
            }
        }
    }
}