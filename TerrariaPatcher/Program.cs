using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Mono.Cecil;

namespace TerrariaPatcher
{
	class Program
	{
		private static readonly string InputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "input");
		private static readonly string OutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");

		static void Main(string[] args)
		{
			Directory.CreateDirectory(InputDirectory);
			Directory.CreateDirectory(OutputDirectory);

			var terrariaPath = Path.Combine(InputDirectory, "Terraria.exe");
			if (!File.Exists(terrariaPath))
			{
				Console.WriteLine(
					"Could not locate Terraria.exe. Place the exe into the 'input' directory and restart the patcher.");
			}
			else
			{
				var assembly = AssemblyDefinition.ReadAssembly(terrariaPath);

				var socialApi = assembly.GetTypeDefinition("SocialAPI");
				socialApi.GetMethodDefinition("Initialize").Ignore();
				socialApi.GetMethodDefinition("Shutdown").Ignore();

				using (var stream = new MemoryStream())
				{
					assembly.Write(stream);
					assembly.Write(Path.Combine(OutputDirectory, "Terraria.exe"));
				}

				Console.WriteLine("Patching complete...");
			}

			Console.ReadKey();
		}
	}
}
