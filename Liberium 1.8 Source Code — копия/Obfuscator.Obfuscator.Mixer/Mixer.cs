using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Obfuscator.Helper;
using Obfuscator.Obfuscator.Junk;

namespace Obfuscator.Obfuscator.Mixer;

internal class Mixer
{
	private static Random random = new Random();

	private static string getRandomCharacters(int count)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 1; i <= count; i++)
		{
			int index = Methods.Random.Next(0, "asdfghjklqwertyuiopmnbvcxz".Length);
			stringBuilder.Append("asdfghjklqwertyuiopmnbvcxz"[index]);
		}
		return stringBuilder.ToString();
	}

	private static TypeDef CreateNewClass(TypeDef typem)
	{
		return new TypeDefUser(string.Concat(typem.Name, "_", getRandomCharacters(10)))
		{
			Attributes = (TypeAttributes.Public | TypeAttributes.BeforeFieldInit),
			Namespace = typem.Namespace,
			IsClass = true
		};
	}

	private static MethodDef ctor(ModuleDef module)
	{
		MethodDefUser methodDefUser = new MethodDefUser(".ctor", MethodSig.CreateInstance(module.CorLibTypes.Void), MethodImplAttributes.IL, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
		MemberRefUser mr = new MemberRefUser(module, ".ctor", MethodSig.CreateInstance(module.CorLibTypes.Void), module.CorLibTypes.Object.TypeDefOrRef);
		methodDefUser.Body = new CilBody();
		methodDefUser.Body.Instructions.Add(OpCodes.Ldarg_0.ToInstruction());
		methodDefUser.Body.Instructions.Add(OpCodes.Call.ToInstruction(mr));
		methodDefUser.Body.Instructions.Add(OpCodes.Nop.ToInstruction());
		methodDefUser.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
		return methodDefUser;
	}

	private static bool Analys(ModuleDef module, MethodDef method)
	{
		if (method.Name == "GetFiltes" || method.FullName.ToLower().Contains("get_") || method.FullName.ToLower().Contains("set_") || method.FullName.ToLower().Contains("<>") || !method.Attributes.ToString().Contains("Static") || method.FullName.ToLower().Contains("<start>") || method.FullName.ToLower().Contains("block") || method.Attributes.ToString().ToLower().Contains("pinvokeimpl") || method.IsConstructor || method.IsSpecialName)
		{
			return false;
		}
		if (method.Attributes.ToString().Replace("PrivateScope", "").Contains("Private"))
		{
			method.Attributes = MethodAttributes.Public | MethodAttributes.Static;
		}
		for (int i = 0; i < method.Body.Instructions.Count(); i++)
		{
			if (method.Body.Instructions[i].OpCode == OpCodes.Call && method.Body.Instructions[i].Operand is MethodDef)
			{
				MethodAttributes attributes = ((MethodDef)method.Body.Instructions[i].Operand).Attributes;
				if (attributes.ToString().Contains("Private") || !attributes.ToString().Contains("Static"))
				{
					return false;
				}
			}
			else if (method.Body.Instructions[i].OpCode == OpCodes.Ldsfld && method.Body.Instructions[i].Operand is FieldDef)
			{
				FieldAttributes attributes2 = ((FieldDef)method.Body.Instructions[i].Operand).Attributes;
				if (attributes2.ToString().Contains("Private") || !attributes2.ToString().Contains("Static"))
				{
					return false;
				}
			}
		}
		return true;
	}

	private static void ReplaceFunction(ModuleDef module, MethodDef replaced, MethodDef main)
	{
		foreach (TypeDef item in (IEnumerable<TypeDef>)module.Types.ToArray())
		{
			foreach (MethodDef method in item.Methods)
			{
				if (method.Body == null)
				{
					continue;
				}
				for (int i = 0; i < method.Body.Instructions.Count(); i++)
				{
					if ((method.Body.Instructions[i].OpCode == OpCodes.Call || method.Body.Instructions[i].OpCode == OpCodes.Ldftn) && method.Body.Instructions[i].Operand == replaced)
					{
						method.Body.Instructions[i].Operand = main;
					}
				}
			}
		}
	}

	public static MethodAttributes RandomMethodAttributes()
	{
		MethodAttributes methodAttributes = MethodAttributes.Private;
		switch (Methods.Random.Next(0, 2))
		{
		case 0:
			methodAttributes = MethodAttributes.Public;
			break;
		case 1:
			methodAttributes = MethodAttributes.Private;
			break;
		}
		return methodAttributes | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.PrivateScope;
	}

	public static FieldAttributes RandomFieldAttributes()
	{
		FieldAttributes fieldAttributes = FieldAttributes.Private;
		switch (Methods.Random.Next(0, 3))
		{
		case 0:
			fieldAttributes = FieldAttributes.Public;
			break;
		case 1:
			fieldAttributes = FieldAttributes.Private;
			break;
		case 2:
			fieldAttributes = FieldAttributes.Literal;
			break;
		}
		return fieldAttributes | FieldAttributes.Static;
	}

	public static void Execute(ModuleDef module)
	{
		List<MethodDef> list = new List<MethodDef>();
		foreach (TypeDef item2 in (IEnumerable<TypeDef>)module.Types.ToArray())
		{
			foreach (MethodDef method2 in item2.Methods)
			{
				if (method2.Body == null || !(method2.Name != "Main") || !(method2.Name != ".ctor") || !(method2.Name != ".cctor") || !Analys(module, method2))
				{
					continue;
				}
				TypeDef typeDef = CreateNewClass(item2);
				typeDef.BaseType = item2.BaseType;
				MethodDefUser methodDefUser = new MethodDefUser(method2.Name, method2.MethodSig, method2.ImplAttributes, MethodAttributes.Public | MethodAttributes.Static);
				methodDefUser.Body = method2.Body;
				bool flag = false;
				List<MethodDef> list2 = new List<MethodDef>();
				for (int i = 0; i < random.Next(1, 4); i++)
				{
					for (int j = 0; j < Methods.Random.Next(0, 2); j++)
					{
						typeDef.Methods.Add(Junks.GetIntFunction(module, Junks.GeneratorBody(module, "Int32"), RandomMethodAttributes()));
					}
					for (int k = 0; k < Methods.Random.Next(0, 2); k++)
					{
						typeDef.Fields.Add(Junks.CreateString(module, RandomFieldAttributes()));
					}
					for (int l = 0; l < Methods.Random.Next(0, 2); l++)
					{
						typeDef.Fields.Add(Junks.CreateBool(module, RandomFieldAttributes()));
					}
					for (int m = 0; m < Methods.Random.Next(0, 2); m++)
					{
						typeDef.Fields.Add(Junks.CreateInt64(module, RandomFieldAttributes()));
					}
					for (int n = 0; n < Methods.Random.Next(0, 2); n++)
					{
						typeDef.Methods.Add(Junks.GetStringFunction(module, Junks.GeneratorBody(module, "String"), RandomMethodAttributes()));
					}
					for (int num = 0; num < Methods.Random.Next(0, 2); num++)
					{
						MethodDefUser item = Junks.MethodGenerator(module, Junks.GeneratorBody(module, "Void"), RandomMethodAttributes());
						list2.Add(item);
						typeDef.Methods.Add(item);
					}
					if (!flag && random.Next(3) == 0)
					{
						typeDef.Methods.Add(methodDefUser);
						typeDef.Methods.Add(ctor(module));
						module.Types.Add(typeDef);
						ReplaceFunction(module, method2, methodDefUser);
						list.Add(method2);
						flag = true;
					}
				}
				if (!flag)
				{
					typeDef.Methods.Add(methodDefUser);
					typeDef.Methods.Add(ctor(module));
					module.Types.Add(typeDef);
					ReplaceFunction(module, method2, methodDefUser);
					list.Add(method2);
				}
				for (int num2 = 0; num2 < list2.Count; num2++)
				{
					MethodDef methodDef = list2[num2];
					MethodDef method = list2[random.Next(list2.Count)];
					methodDef.Body.Instructions.Add(Instruction.Create(OpCodes.Call, method));
					methodDef.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
				}
				list2.Clear();
			}
		}
		foreach (MethodDef item3 in list)
		{
			foreach (TypeDef type in module.Types)
			{
				if (type.Methods.Contains(item3))
				{
					type.Methods.Remove(item3);
					break;
				}
			}
		}
		list.Clear();
	}
}
