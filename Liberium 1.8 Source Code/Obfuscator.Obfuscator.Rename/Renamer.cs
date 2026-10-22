using System.Collections.Generic;
using dnlib.DotNet;
using Obfuscator.Helper;

namespace Obfuscator.Obfuscator.Rename;

internal class Renamer
{
	private static readonly Dictionary<TypeDef, bool> TypeRename = new Dictionary<TypeDef, bool>();

	private static readonly List<string> TypeNewName = new List<string>();

	private static readonly Dictionary<MethodDef, bool> MethodRename = new Dictionary<MethodDef, bool>();

	private static readonly List<string> MethodNewName = new List<string>();

	private static readonly Dictionary<FieldDef, bool> FieldRename = new Dictionary<FieldDef, bool>();

	private static readonly List<string> FieldNewName = new List<string>();

	private static int RenameTypes;

	private static int RenameFields;

	private static int RenameMethods;

	private static int RenameNameSpaces;

	public static void Execute(ModuleDefMD module)
	{
		RenameTypes = 0;
		RenameFields = 0;
		RenameMethods = 0;
		RenameNameSpaces = 1;
		string text = Methods.GenerateString();
		foreach (TypeDef type in module.Types)
		{
			if (Methods.GenerateBool(4))
			{
				text = Methods.GenerateString();
				RenameNameSpaces++;
			}
			if (TypeRename.TryGetValue(type, out var value))
			{
				if (value)
				{
					InternalRename(type);
				}
			}
			else
			{
				InternalRename(type);
			}
			type.Namespace = text;
			foreach (TypeDef nestedType in type.NestedTypes)
			{
				if (TypeRename.TryGetValue(nestedType, out var value2))
				{
					if (value2)
					{
						InternalRename(nestedType);
					}
				}
				else
				{
					InternalRename(nestedType);
				}
				foreach (MethodDef method in nestedType.Methods)
				{
					foreach (ParamDef paramDef in method.ParamDefs)
					{
						paramDef.Name = Methods.GenerateString(16);
					}
				}
			}
			foreach (MethodDef method2 in type.Methods)
			{
				if (MethodRename.TryGetValue(method2, out var value3))
				{
					if (value3 && !method2.IsConstructor && !method2.IsSpecialName)
					{
						InternalRename(method2);
					}
				}
				else if (!method2.IsConstructor && !method2.IsSpecialName)
				{
					InternalRename(method2);
				}
			}
			MethodNewName.Clear();
			foreach (FieldDef field in type.Fields)
			{
				if (FieldRename.TryGetValue(field, out var value4))
				{
					if (value4)
					{
						InternalRename(field);
					}
				}
				else
				{
					InternalRename(field);
				}
			}
			FieldNewName.Clear();
		}
		TypeRename.Clear();
		MethodRename.Clear();
		FieldRename.Clear();
	}

	public static string Info()
	{
		return $"Rename Obfuscator [Types: [{RenameTypes}]  Namespaces: [{RenameNameSpaces}]  Methods: [{RenameMethods}]  Fields: [{RenameFields}]]";
	}

	private static void InternalRename(TypeDef type)
	{
		string text = Methods.GenerateString();
		while (TypeNewName.Contains(text))
		{
			text = Methods.GenerateString();
		}
		TypeNewName.Add(text);
		type.Name = text;
		RenameTypes++;
	}

	private static void InternalRename(MethodDef method)
	{
		string text = Methods.GenerateString();
		while (MethodNewName.Contains(text))
		{
			text = Methods.GenerateString();
		}
		MethodNewName.Add(text);
		method.Name = text;
		RenameMethods++;
	}

	private static void InternalRename(FieldDef field)
	{
		string text = Methods.GenerateString();
		while (FieldNewName.Contains(text))
		{
			text = Methods.GenerateString();
		}
		FieldNewName.Add(text);
		field.Name = text;
		RenameFields++;
	}
}
