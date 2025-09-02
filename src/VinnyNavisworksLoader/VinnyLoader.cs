using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Plugins;
using System.IO;
using System.Reflection;
using System.Windows;


namespace VinnyNavisworksLoader
{
    [PluginAttribute("VinnyNavisworksAdapter", 
                    "379f8e07a9104710828dbcd05f3916ae",
                    ToolTip = "Vinny data exporter for Navisworks",
                    DisplayName = "Vinny data exporter")]

    public class VinnyLoader : AddInPlugin
    {
        public override int Execute(params string[] parameters)
        {
            //Load all need dlls
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

            string VinnyNavisworksAdapterPath = Path.Combine(vinnyPath, "plugins", "navisworks", navisYear, targetFramework, "VinnyNavisworksAdapter.dll");

            Assembly.LoadFrom(VinnyLibConverterCommonPath);
            Assembly.LoadFrom(VinnyLibConverterKernelPath);
            Assembly.LoadFrom(VinnyLibConverterUIPath);
            Assembly.LoadFrom(VinnyNavisworksAdapterPath);

            VinnyNavisworksAdapter.VinnyAdapterImpl.Start();
            return 1;
        }

    }
}
