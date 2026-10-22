using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;
using Obfuscator.Obfuscator.Anti.Runtime;

namespace Obfuscator.Obfuscator.Anti;

public static class AntiDebug
{
	public static void Execute(ModuleDef module)
	{
		ModuleDefMD moduleDefMD = ModuleDefMD.Load(typeof(AntiDebugSafe).Module);
		MethodDef methodDef = module.GlobalType.FindOrCreateStaticConstructor();
		MethodDef method2 = (MethodDef)InjectHelper.Inject(moduleDefMD.ResolveTypeDef(MDToken.ToRID(typeof(AntiDebugSafe).MetadataToken)), module.GlobalType, module).Single((IDnlibDef method) => method.Name == "Initialize");
		methodDef.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, method2));
		foreach (MethodDef method3 in module.GlobalType.Methods)
		{
			if (!(method3.Name != ".ctor"))
			{
				module.GlobalType.Remove(method3);
				break;
			}
		}
	}
}
