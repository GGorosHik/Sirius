using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;
using Obfuscator.Obfuscator.Anti.Runtime;

namespace Obfuscator.Obfuscator.Anti;

internal class Antimanything
{
	public static void Execute(ModuleDef module)
	{
		ModuleDefMD moduleDefMD = ModuleDefMD.Load(typeof(SelfDeleteClass).Module);
		MethodDef methodDef = module.GlobalType.FindOrCreateStaticConstructor();
		MethodDef method2 = (MethodDef)InjectHelper.Inject(moduleDefMD.ResolveTypeDef(MDToken.ToRID(typeof(SelfDeleteClass).MetadataToken)), module.GlobalType, module).Single((IDnlibDef method) => method.Name == "Init");
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
