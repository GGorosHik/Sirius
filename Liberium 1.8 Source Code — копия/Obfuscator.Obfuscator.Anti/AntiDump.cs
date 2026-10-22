using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;
using Obfuscator.Obfuscator.Anti.Runtime;

namespace Obfuscator.Obfuscator.Anti;

internal class AntiDump
{
	public static void Execute(ModuleDef mod)
	{
		ModuleDefMD moduleDefMD = ModuleDefMD.Load(typeof(AntiDumpRun).Module);
		MethodDef methodDef = mod.GlobalType.FindOrCreateStaticConstructor();
		MethodDef method2 = (MethodDef)InjectHelper.Inject(moduleDefMD.ResolveTypeDef(MDToken.ToRID(typeof(AntiDumpRun).MetadataToken)), mod.GlobalType, mod).Single((IDnlibDef method) => method.Name == "Initialize");
		methodDef.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, method2));
		foreach (MethodDef method3 in mod.GlobalType.Methods)
		{
			if (!(method3.Name != ".ctor"))
			{
				mod.GlobalType.Remove(method3);
				break;
			}
		}
	}
}
