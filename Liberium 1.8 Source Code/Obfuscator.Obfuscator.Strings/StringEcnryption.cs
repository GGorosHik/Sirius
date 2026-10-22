using System;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;
using Obfuscator.Obfuscator.Strings.Runtime;

namespace Obfuscator.Obfuscator.Strings;

internal class StringEcnryption
{
	private static MethodDef InjectMethod(ModuleDef module, string methodName)
	{
		MethodDef result = (MethodDef)InjectHelper.Inject(ModuleDefMD.Load(typeof(DecryptionHelper).Module).ResolveTypeDef(MDToken.ToRID(typeof(DecryptionHelper).MetadataToken)), module.GlobalType, module).Single((IDnlibDef method) => method.Name == methodName);
		foreach (MethodDef method in module.GlobalType.Methods)
		{
			if (method.Name == ".ctor")
			{
				module.GlobalType.Remove(method);
				break;
			}
		}
		return result;
	}

	private static string Encrypt(string dataPlain)
	{
		try
		{
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(dataPlain));
		}
		catch (Exception)
		{
			return null;
		}
	}

	public static void Execute(ModuleDef module)
	{
		MethodDef methodDef = InjectMethod(module, "Decrypt_Base64");
		foreach (TypeDef type in module.Types)
		{
			if (type.IsGlobalModuleType || type.Name == "Resources" || type.Name == "Settings")
			{
				continue;
			}
			foreach (MethodDef method in type.Methods)
			{
				if (!method.HasBody || method == methodDef)
				{
					continue;
				}
				method.Body.KeepOldMaxStack = true;
				for (int i = 0; i < method.Body.Instructions.Count; i++)
				{
					if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr)
					{
						string dataPlain = method.Body.Instructions[i].Operand.ToString();
						method.Body.Instructions[i].Operand = Encrypt(dataPlain);
						method.Body.Instructions.Insert(i + 1, new Instruction(OpCodes.Call, methodDef));
					}
				}
				method.Body.SimplifyBranches();
				method.Body.OptimizeBranches();
			}
		}
	}
}
