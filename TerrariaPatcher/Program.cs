using System;
using System.IO;
using Mono.Cecil;
using Microsoft.Win32;

namespace TerrariaPatcher
{
	class Program
	{
		private static readonly string OutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");

		static void Main(string[] args)
		{
			Directory.CreateDirectory(OutputDirectory);

			string terrariaPath = null;
			if (File.Exists("Terraria.exe"))
			{
				terrariaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Terraria.exe");
			}
			else
			{
				RegistryKey key = Environment.Is64BitOperatingSystem
					? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
					: RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);

				var path = key.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 105600")
					.GetValue("InstallLocation").ToString();
				if (File.Exists(Path.Combine(path, "Terraria.exe")))
				{
					terrariaPath = Path.Combine(path, "Terraria.exe");
				}
			}

			if (string.IsNullOrWhiteSpace(terrariaPath))
			{
				Console.WriteLine(
					"Could not locate Terraria.exe. Place the exe into the patcher's directory and restart the patcher.");
			}
			else
			{
				var assembly = AssemblyDefinition.ReadAssembly(terrariaPath);

				var socialApi = assembly.GetTypeDefinition("SocialAPI");
				socialApi.GetMethodDefinition("Initialize").Ignore();
				socialApi.GetMethodDefinition("Shutdown").Ignore();

				assembly.Write(Path.Combine(OutputDirectory, "Terraria.exe"));
				Console.WriteLine("Patching complete...");
			}

			Console.ReadKey();
		}
	}
}
