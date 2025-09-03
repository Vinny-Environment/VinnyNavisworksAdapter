using System;
using System.IO;
using System.Reflection;

using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Plugins;

namespace VinnyNavisworksLoader
{
    [AddInPlugin(AddInLocation.Export,
               CanToggle = true,
               LoadForCanExecute = true,
               CallCanExecute = CallCanExecute.Always,
               Icon = "Resources\\vinnyIcon_16x16.bmp",
               LargeIcon = "Resources\\vinnyIcon_32x32.bmp",
               ShortcutWindowTypes = "")]
    [PluginAttribute("VinnyNavisworksAdapter", 
                    "VINNY",
                    ToolTip = "Vinny data exporter for Navisworks",
                    DisplayName = "Vinny data exporter")
        ]

    public class VinnyLoader : AddInPlugin
    {
        public override int Execute(params string[] parameters)
        {
            //Load all need dlls
            if (!pIsInitialized) Init();

            Run();
            return 0;
        }

        private void Run()
        {
            VinnyNavisworksAdapter.VinnyAdapterImpl.CreateInstance().Start();
        }

        private static bool pIsInitialized = false;

        private void Init()
        {
            string vinnyPath = @"C:\VinnyConverter";
            if (File.Exists("VinnyPath.txt")) vinnyPath = File.ReadAllText("VinnyPath.txt");
#if DEBUG
            vinnyPath = @"C:\Users\Georg\Documents\GitHub\VinnyLibBin\Debug";
#endif
            string navisYear = "";
            string targetFramework = "net48";
#if D_N21 || R_N21
            navisYear = "2021";
#endif

            string VinnyLibConverterCommonPath = Path.Combine(vinnyPath, "VinnyLibConverterCommon.dll");
            string VinnyLibConverterKernelPath = Path.Combine(vinnyPath, "VinnyLibConverterKernel.dll");
            string VinnyLibConverterUIPath = Path.Combine(vinnyPath, "ui", targetFramework, "VinnyLibConverterUI.dll");

            string VinnyNavisworksAdapterPath = Path.Combine(vinnyPath, "plugins", "navisworks", navisYear, "VinnyNavisworksAdapter.dll");

            var ass1 = Assembly.LoadFrom(VinnyLibConverterCommonPath);
            var ass2 = Assembly.LoadFrom(VinnyLibConverterKernelPath);
            var ass3 = Assembly.LoadFrom(VinnyLibConverterUIPath);
            var ass4 = Assembly.LoadFrom(VinnyNavisworksAdapterPath);

            AddEnv(Path.GetDirectoryName(VinnyNavisworksAdapterPath));
            AddEnv(Path.GetDirectoryName(VinnyLibConverterUIPath));
            AddEnv(vinnyPath);

            foreach (string depsDir in Directory.GetDirectories(Path.Combine(vinnyPath, "dependencies"), "*.*", SearchOption.TopDirectoryOnly))
            {
                AddEnv(depsDir);
                foreach (string DepsAssPath in Directory.GetFiles(depsDir, "*.dll", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        Assembly.LoadFrom(DepsAssPath);
                    }
                    catch (Exception ex) {  }
                }
            }

            string env = Environment.GetEnvironmentVariable("PATH");

            pIsInitialized = true;
        }

        private void AddEnv(string path)
        {
            string newEnwPathValue = Environment.GetEnvironmentVariable("PATH");
            if (newEnwPathValue.EndsWith(";")) newEnwPathValue += path + ";";
            else newEnwPathValue += ";" + path + ";";

            Environment.SetEnvironmentVariable("PATH", newEnwPathValue);
        }

    }
}
