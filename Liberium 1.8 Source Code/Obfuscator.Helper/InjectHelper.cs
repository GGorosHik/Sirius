using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Obfuscator.Helper;

public static class InjectHelper
{
	private class InjectContext : ImportMapper
	{
		public readonly Dictionary<IDnlibDef, IDnlibDef> Mep = new Dictionary<IDnlibDef, IDnlibDef>();

		public readonly ModuleDef TargetModule;

		public Importer Importer { get; }

		public InjectContext(ModuleDef target)
		{
			TargetModule = target;
			Importer = new Importer(target, ImporterOptions.TryToUseTypeDefs, default(GenericParamContext), this);
		}

		public override ITypeDefOrRef Map(ITypeDefOrRef typeDefOrRef)
		{
			if (!(typeDefOrRef is TypeDef key) || !Mep.ContainsKey(key))
			{
				return null;
			}
			return (TypeDef)Mep[key];
		}

		public override IMethod Map(MethodDef methodDef)
		{
			if (!Mep.ContainsKey(methodDef))
			{
				return null;
			}
			return (MethodDef)Mep[methodDef];
		}

		public override IField Map(FieldDef fieldDef)
		{
			if (!Mep.ContainsKey(fieldDef))
			{
				return null;
			}
			return (FieldDef)Mep[fieldDef];
		}
	}

	private static TypeDefUser Clone(TypeDef origin)
	{
		TypeDefUser typeDefUser = new TypeDefUser(origin.Namespace, origin.Name)
		{
			Attributes = origin.Attributes
		};
		if (origin.ClassLayout != null)
		{
			typeDefUser.ClassLayout = new ClassLayoutUser(origin.ClassLayout.PackingSize, origin.ClassSize);
		}
		foreach (GenericParam genericParameter in origin.GenericParameters)
		{
			typeDefUser.GenericParameters.Add(new GenericParamUser(genericParameter.Number, genericParameter.Flags, "-"));
		}
		return typeDefUser;
	}

	private static MethodDefUser Clone(MethodDef origin)
	{
		MethodDefUser methodDefUser = new MethodDefUser(origin.Name, null, origin.ImplAttributes, origin.Attributes);
		foreach (GenericParam genericParameter in origin.GenericParameters)
		{
			methodDefUser.GenericParameters.Add(new GenericParamUser(genericParameter.Number, genericParameter.Flags, "-"));
		}
		return methodDefUser;
	}

	private static FieldDefUser Clone(FieldDef origin)
	{
		return new FieldDefUser(origin.Name, null, origin.Attributes);
	}

	private static TypeDef PopulateContext(TypeDef typeDef, InjectContext ctx)
	{
		TypeDef typeDef2;
		if (!ctx.Mep.TryGetValue(typeDef, out var value))
		{
			typeDef2 = Clone(typeDef);
			ctx.Mep[typeDef] = typeDef2;
		}
		else
		{
			typeDef2 = (TypeDef)value;
		}
		foreach (TypeDef nestedType in typeDef.NestedTypes)
		{
			typeDef2.NestedTypes.Add(PopulateContext(nestedType, ctx));
		}
		foreach (MethodDef method in typeDef.Methods)
		{
			IList<MethodDef> methods = typeDef2.Methods;
			IDnlibDef dnlibDef2 = (ctx.Mep[method] = Clone(method));
			methods.Add((MethodDef)dnlibDef2);
		}
		foreach (FieldDef field in typeDef.Fields)
		{
			IList<FieldDef> fields = typeDef2.Fields;
			IDnlibDef dnlibDef2 = (ctx.Mep[field] = Clone(field));
			fields.Add((FieldDef)dnlibDef2);
		}
		return typeDef2;
	}

	private static void CopyTypeDef(TypeDef typeDef, InjectContext ctx)
	{
		TypeDef typeDef2 = (TypeDef)ctx.Mep[typeDef];
		typeDef2.BaseType = ctx.Importer.Import(typeDef.BaseType);
		foreach (InterfaceImpl @interface in typeDef.Interfaces)
		{
			typeDef2.Interfaces.Add(new InterfaceImplUser(ctx.Importer.Import(@interface.Interface)));
		}
	}

	private static void CopyMethodDef(MethodDef methodDef, InjectContext ctx)
	{
		MethodDef methodDef2 = (MethodDef)ctx.Mep[methodDef];
		methodDef2.Signature = ctx.Importer.Import(methodDef.Signature);
		methodDef2.Parameters.UpdateParameterTypes();
		if (methodDef.ImplMap != null)
		{
			methodDef2.ImplMap = new ImplMapUser(new ModuleRefUser(ctx.TargetModule, methodDef.ImplMap.Module.Name), methodDef.ImplMap.Name, methodDef.ImplMap.Attributes);
		}
		foreach (CustomAttribute customAttribute in methodDef.CustomAttributes)
		{
			methodDef2.CustomAttributes.Add(new CustomAttribute((ICustomAttributeType)ctx.Importer.Import(customAttribute.Constructor)));
		}
		if (!methodDef.HasBody)
		{
			return;
		}
		methodDef2.Body = new CilBody(methodDef.Body.InitLocals, new List<Instruction>(), new List<ExceptionHandler>(), new List<Local>())
		{
			MaxStack = methodDef.Body.MaxStack
		};
		Dictionary<object, object> bodyMap = new Dictionary<object, object>();
		foreach (Local variable in methodDef.Body.Variables)
		{
			Local local = new Local(ctx.Importer.Import(variable.Type));
			methodDef2.Body.Variables.Add(local);
			local.Name = variable.Name;
			local.Attributes = variable.Attributes;
			bodyMap[variable] = local;
		}
		foreach (Instruction instruction2 in methodDef.Body.Instructions)
		{
			Instruction instruction = new Instruction(instruction2.OpCode, instruction2.Operand)
			{
				SequencePoint = instruction2.SequencePoint
			};
			object operand = instruction.Operand;
			if (!(operand is IType type))
			{
				if (!(operand is IMethod method))
				{
					if (operand is IField field)
					{
						instruction.Operand = ctx.Importer.Import(field);
					}
				}
				else
				{
					instruction.Operand = ctx.Importer.Import(method);
				}
			}
			else
			{
				instruction.Operand = ctx.Importer.Import(type);
			}
			methodDef2.Body.Instructions.Add(instruction);
			bodyMap[instruction2] = instruction;
		}
		foreach (Instruction instruction3 in methodDef2.Body.Instructions)
		{
			if (instruction3.Operand != null && bodyMap.ContainsKey(instruction3.Operand))
			{
				instruction3.Operand = bodyMap[instruction3.Operand];
			}
			else if (instruction3.Operand is Instruction[] source)
			{
				instruction3.Operand = source.Select((Instruction target) => (Instruction)bodyMap[target]).ToArray();
			}
		}
		foreach (ExceptionHandler exceptionHandler in methodDef.Body.ExceptionHandlers)
		{
			methodDef2.Body.ExceptionHandlers.Add(new ExceptionHandler(exceptionHandler.HandlerType)
			{
				CatchType = ((exceptionHandler.CatchType == null) ? null : ctx.Importer.Import(exceptionHandler.CatchType)),
				TryStart = (Instruction)bodyMap[exceptionHandler.TryStart],
				TryEnd = (Instruction)bodyMap[exceptionHandler.TryEnd],
				HandlerStart = (Instruction)bodyMap[exceptionHandler.HandlerStart],
				HandlerEnd = (Instruction)bodyMap[exceptionHandler.HandlerEnd],
				FilterStart = ((exceptionHandler.FilterStart == null) ? null : ((Instruction)bodyMap[exceptionHandler.FilterStart]))
			});
		}
		methodDef2.Body.SimplifyMacros(methodDef2.Parameters);
	}

	private static void CopyFieldDef(FieldDef fieldDef, InjectContext ctx)
	{
		((FieldDef)ctx.Mep[fieldDef]).Signature = ctx.Importer.Import(fieldDef.Signature);
	}

	private static void Copy(TypeDef typeDef, InjectContext ctx, bool copySelf)
	{
		if (copySelf)
		{
			CopyTypeDef(typeDef, ctx);
		}
		foreach (TypeDef nestedType in typeDef.NestedTypes)
		{
			Copy(nestedType, ctx, copySelf: true);
		}
		foreach (MethodDef method in typeDef.Methods)
		{
			CopyMethodDef(method, ctx);
		}
		foreach (FieldDef field in typeDef.Fields)
		{
			CopyFieldDef(field, ctx);
		}
	}

	public static TypeDef Inject(TypeDef typeDef, ModuleDef target)
	{
		InjectContext injectContext = new InjectContext(target);
		PopulateContext(typeDef, injectContext);
		Copy(typeDef, injectContext, copySelf: true);
		return (TypeDef)injectContext.Mep[typeDef];
	}

	public static MethodDef Inject(MethodDef methodDef, ModuleDef target)
	{
		InjectContext injectContext = new InjectContext(target);
		injectContext.Mep[methodDef] = Clone(methodDef);
		InjectContext injectContext2 = injectContext;
		CopyMethodDef(methodDef, injectContext2);
		return (MethodDef)injectContext2.Mep[methodDef];
	}

	public static IEnumerable<IDnlibDef> Inject(TypeDef typeDef, TypeDef newType, ModuleDef target)
	{
		InjectContext injectContext = new InjectContext(target);
		injectContext.Mep[typeDef] = newType;
		InjectContext injectContext2 = injectContext;
		PopulateContext(typeDef, injectContext2);
		Copy(typeDef, injectContext2, copySelf: false);
		return injectContext2.Mep.Values.Except(new TypeDef[1] { newType });
	}
}
