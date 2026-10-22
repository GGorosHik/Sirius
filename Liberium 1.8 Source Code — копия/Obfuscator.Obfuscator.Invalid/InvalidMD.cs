using System;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;

namespace Obfuscator.Obfuscator.Invalid;

internal class InvalidMD
{
	public static void Execute(ModuleDef module)
	{
		module.Mvid = null;
		module.Name = Methods.GenerateString();
		module.Import(new FieldDefUser(Methods.GenerateString()));
		foreach (TypeDef type in module.Types)
		{
			TypeDef typeDef = new TypeDefUser(Methods.GenerateString());
			typeDef.Methods.Add(new MethodDefUser());
			typeDef.NestedTypes.Add(new TypeDefUser(Methods.GenerateString()));
			MethodDef item = new MethodDefUser();
			typeDef.Methods.Add(item);
			type.NestedTypes.Add(typeDef);
			type.Events.Add(new EventDefUser());
			foreach (MethodDef method in type.Methods)
			{
				if (method.Body == null)
				{
					continue;
				}
				method.Body.SimplifyBranches();
				if (string.Compare(method.ReturnType.FullName, "System.Void", StringComparison.Ordinal) == 0 && method.HasBody && method.Body.Instructions.Count != 0)
				{
					Local local = new Local(module.Import(typeof(int)).ToTypeSig());
					Local local2 = new Local(module.Import(typeof(bool)).ToTypeSig());
					method.Body.Variables.Add(local);
					method.Body.Variables.Add(local2);
					Instruction operand = method.Body.Instructions[method.Body.Instructions.Count - 1];
					Instruction item2 = new Instruction(OpCodes.Ret);
					Instruction instruction = new Instruction(OpCodes.Ldc_I4_1);
					method.Body.Instructions.Insert(0, new Instruction(OpCodes.Ldc_I4_0));
					method.Body.Instructions.Insert(1, new Instruction(OpCodes.Stloc, local));
					method.Body.Instructions.Insert(2, new Instruction(OpCodes.Br, instruction));
					Instruction instruction2 = new Instruction(OpCodes.Ldloc, local);
					method.Body.Instructions.Insert(3, instruction2);
					method.Body.Instructions.Insert(4, new Instruction(OpCodes.Ldc_I4_0));
					method.Body.Instructions.Insert(5, new Instruction(OpCodes.Ceq));
					method.Body.Instructions.Insert(6, new Instruction(OpCodes.Ldc_I4_1));
					method.Body.Instructions.Insert(7, new Instruction(OpCodes.Ceq));
					method.Body.Instructions.Insert(8, new Instruction(OpCodes.Stloc, local2));
					method.Body.Instructions.Insert(9, new Instruction(OpCodes.Ldloc, local2));
					method.Body.Instructions.Insert(10, new Instruction(OpCodes.Brtrue, method.Body.Instructions[10]));
					method.Body.Instructions.Insert(11, new Instruction(OpCodes.Ret));
					method.Body.Instructions.Insert(12, new Instruction(OpCodes.Calli));
					method.Body.Instructions.Insert(13, new Instruction(OpCodes.Sizeof, operand));
					method.Body.Instructions.Insert(method.Body.Instructions.Count, instruction);
					method.Body.Instructions.Insert(method.Body.Instructions.Count, new Instruction(OpCodes.Stloc, local2));
					method.Body.Instructions.Insert(method.Body.Instructions.Count, new Instruction(OpCodes.Br, instruction2));
					method.Body.Instructions.Insert(method.Body.Instructions.Count, item2);
					ExceptionHandler item3 = new ExceptionHandler(ExceptionHandlerType.Finally)
					{
						HandlerStart = method.Body.Instructions[10],
						HandlerEnd = method.Body.Instructions[11],
						TryEnd = method.Body.Instructions[14],
						TryStart = method.Body.Instructions[12]
					};
					if (!method.Body.HasExceptionHandlers)
					{
						method.Body.ExceptionHandlers.Add(item3);
					}
					method.Body.OptimizeBranches();
					method.Body.OptimizeMacros();
				}
			}
		}
		TypeDef typeDef2 = new TypeDefUser(Methods.GenerateString());
		FieldDef item4 = new FieldDefUser(Methods.GenerateString(), new FieldSig(module.Import(typeof(void)).ToTypeSig()));
		typeDef2.Fields.Add(item4);
		typeDef2.BaseType = module.Import(typeof(void));
		module.Types.Add(typeDef2);
		TypeDef item5 = new TypeDefUser(Methods.GenerateString())
		{
			IsInterface = true,
			IsSealed = true
		};
		module.Types.Add(item5);
		module.TablesHeaderVersion = (ushort)257;
	}
}
