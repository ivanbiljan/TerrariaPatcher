using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Microsoft.Win32;
using Mono.Cecil.Cil;

namespace TerrariaPatcher
{
	internal class TerrariaResolver : BaseAssemblyResolver {
		private readonly DefaultAssemblyResolver _defaultResolver;

		public TerrariaResolver() {
			_defaultResolver = new DefaultAssemblyResolver();
		}

		public override AssemblyDefinition Resolve(AssemblyNameReference name) {
			AssemblyDefinition assembly = null;
			try {
				assembly = _defaultResolver.Resolve(name);
			}
			catch (AssemblyResolutionException ex) {
			}

			return assembly;
		}
	}

	class Program
	{
		private static readonly string OutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");
		private static AssemblyDefinition _terrariaAssembly;

		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
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
			else {
				_terrariaAssembly =
					AssemblyDefinition.ReadAssembly(terrariaPath, new ReaderParameters {AssemblyResolver = new TerrariaResolver()});

				var socialApi = _terrariaAssembly.GetTypeDefinition("SocialAPI");
				socialApi.GetMethodDefinition("Initialize").Ignore();
				socialApi.GetMethodDefinition("Shutdown").Ignore();

				var notificationsTracker =
					_terrariaAssembly.MainModule.Types.FirstOrDefault(t => t.FullName == "Terraria.UI.InGameNotificationsTracker");
				var initialize = notificationsTracker.Methods.FirstOrDefault(m => m.Name == "Initialize");
				var processor = initialize.Body.GetILProcessor();
				processor.InsertBefore(initialize.Body.Instructions[0], Instruction.Create(OpCodes.Ret));

				_terrariaAssembly.Write(Path.Combine(OutputDirectory, "patched.exe"));
				Console.WriteLine("Patching complete...");
			}

			Console.ReadKey();
		}

		private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args) {
			// var assemblyName = new AssemblyName(args.Name);
			// var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName.Name);
			// if (assembly != null) {
			// 	return assembly;
			// }
			//
			// var resource = _terrariaAssembly.GetManifestResourceNames().FirstOrDefault(r => r.EndsWith(assemblyName.Name + ".dll"));
			// if (resource == null) {
			// 	return null;
			// }
			//
			// using (var stream = _terrariaAssembly.GetManifestResourceStream(resource)) {
			// 	var buffer = new byte[stream.Length];
			// 	stream.Read(buffer, 0, buffer.Length);
			// 	return Assembly.Load(buffer);
			// }

			return null;
		}
	}
}
