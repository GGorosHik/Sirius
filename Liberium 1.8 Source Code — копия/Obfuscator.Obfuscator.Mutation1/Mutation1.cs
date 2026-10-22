using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Obfuscator.Obfuscator.Mutation1;

internal class Mutation1
{
	private static ModuleDefMD _moduleDefMd;

	public static void Execute(ModuleDefMD moduleDefMd)
	{
		_moduleDefMd = moduleDefMd;
		MutationHelper.CryptoRandom cryptoRandom = new MutationHelper.CryptoRandom();
		foreach (TypeDef type in moduleDefMd.GetTypes())
		{
			List<MethodDef> list = new List<MethodDef>();
			foreach (MethodDef item in type.Methods.Where((MethodDef x) => x.HasBody))
			{
				IList<Instruction> instructions = item.Body.Instructions;
				for (int i = 0; i < instructions.Count; i++)
				{
					if (instructions[i].IsLdcI4() && IsSafe(instructions.ToList(), i))
					{
						MethodDef methodDef = null;
						int ldcI4Value = instructions[i].GetLdcI4Value();
						instructions[i].OpCode = OpCodes.Ldc_R8;
						switch (cryptoRandom.Next(0, 3))
						{
						case 0:
							methodDef = GenerateRefMethod("Floor");
							instructions[i].Operand = Convert.ToDouble((double)ldcI4Value + cryptoRandom.NextDouble());
							break;
						case 1:
							methodDef = GenerateRefMethod("Sqrt");
							instructions[i].Operand = Math.Pow(Convert.ToDouble(ldcI4Value), 2.0);
							break;
						case 2:
							methodDef = GenerateRefMethod("Round");
							instructions[i].Operand = Convert.ToDouble(ldcI4Value);
							break;
						}
						instructions.Insert(i + 1, OpCodes.Call.ToInstruction(methodDef));
						instructions.Insert(i + 2, OpCodes.Conv_I4.ToInstruction());
						i += 2;
						list.Add(methodDef);
					}
				}
				item.Body.SimplifyMacros(item.Parameters);
			}
			foreach (MethodDef item2 in list)
			{
				type.Methods.Add(item2);
			}
		}
	}

	private static MethodDef GenerateRefMethod(string methodName)
	{
		MethodDefUser methodDefUser = new MethodDefUser("_" + Guid.NewGuid().ToString("D").ToUpper()
			.Substring(2, 5), MethodSig.CreateStatic(_moduleDefMd.ImportAsTypeSig(typeof(double))), MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig)
		{
			Signature = new MethodSig
			{
				Params = { _moduleDefMd.ImportAsTypeSig(typeof(double)) },
				RetType = _moduleDefMd.ImportAsTypeSig(typeof(double))
			}
		};
		CilBody cilBody = new CilBody();
		cilBody.Instructions.Add(OpCodes.Ldarg_0.ToInstruction());
		cilBody.Instructions.Add(OpCodes.Call.ToInstruction(GetMethod(typeof(Math), methodName, new Type[1] { typeof(double) })));
		cilBody.Instructions.Add(OpCodes.Stloc_0.ToInstruction());
		cilBody.Instructions.Add(OpCodes.Ldloc_0.ToInstruction());
		cilBody.Instructions.Add(OpCodes.Ret.ToInstruction());
		CilBody body = cilBody;
		methodDefUser.Body = body;
		methodDefUser.Body.Variables.Add(new Local(_moduleDefMd.ImportAsTypeSig(typeof(double))));
		return methodDefUser.ResolveMethodDef();
	}

	private static bool IsSafe(List<Instruction> instructions, int i)
	{
		if (new int[5] { -2, -1, 0, 1, 2 }.Contains(instructions[i].GetLdcI4Value()))
		{
			return false;
		}
		return true;
	}

	private static IMethod GetMethod(Type type, string methodName, Type[] types)
	{
		return _moduleDefMd.Import(type.GetMethod(methodName, types));
	}
}
