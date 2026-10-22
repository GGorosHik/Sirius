using System.IO;
using System.Linq;
using System.Security.Cryptography;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;
using Obfuscator.Obfuscator.Anti.Runtime;

namespace Obfuscator.Obfuscator.Anti;

public static class AntiTamper
{
	public static void Sha256(string filePath)
	{
		byte[] array = SHA256.Create().ComputeHash(File.ReadAllBytes(filePath));
		using FileStream fileStream = new FileStream(filePath, FileMode.Append);
		fileStream.Write(array, 0, array.Length);
	}

	public static void Execute(ModuleDef module)
	{
		ModuleDefMD moduleDefMD = ModuleDefMD.Load(typeof(EofAntiTamper).Module);
		MethodDef methodDef = module.GlobalType.FindOrCreateStaticConstructor();
		MethodDef method2 = (MethodDef)InjectHelper.Inject(moduleDefMD.ResolveTypeDef(MDToken.ToRID(typeof(EofAntiTamper).MetadataToken)), module.GlobalType, module).Single((IDnlibDef method) => method.Name == "Initializer");
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
