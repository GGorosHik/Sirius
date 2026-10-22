using dnlib.DotNet;

namespace Obfuscator.Obfuscator.Anti;

internal class AntiDe4dot
{
	public static void Execute(AssemblyDef mod)
	{
		foreach (ModuleDef module in mod.Modules)
		{
			InterfaceImplUser item = new InterfaceImplUser(module.GlobalType);
			for (int i = 0; i < 1; i++)
			{
				TypeDefUser typeDefUser = new TypeDefUser(string.Empty, $"Form{i}", module.CorLibTypes.GetTypeRef("System", "Attribute"));
				InterfaceImplUser item2 = new InterfaceImplUser(typeDefUser);
				module.Types.Add(typeDefUser);
				typeDefUser.Interfaces.Add(item2);
				typeDefUser.Interfaces.Add(item);
			}
		}
	}
}
