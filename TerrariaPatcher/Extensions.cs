using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace TerrariaPatcher
{
	public static class Extensions
	{
		public static TypeDefinition GetTypeDefinition(this AssemblyDefinition assembly, string typeName)
		{
			return assembly.Modules.SelectMany(m => m.Types).First(t => t.Name == typeName);
		}

		public static MethodDefinition GetMethodDefinition(this TypeDefinition type, string methodName)
		{
			return type.Methods.First(m => m.Name == methodName);
		}

		public static void Ignore(this MethodDefinition method)
		{
			var body = method.Body;
			var processor = body.GetILProcessor();
			var target = body.Instructions[0];

			processor.InsertBefore(target, Instruction.Create(OpCodes.Ret));
		}
	}
}
