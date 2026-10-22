using System;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Obfuscator.Obfuscator.Invalid;

internal class InvalidOpcodes
{
	public static void Execute(ModuleDef module)
	{
		foreach (TypeDef type in module.GetTypes())
		{
			foreach (MethodDef method in type.Methods)
			{
				if (method.HasBody || method.Body.HasInstructions)
				{
					method.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Box, method.Module.Import(typeof(Math))));
				}
			}
		}
	}
}
