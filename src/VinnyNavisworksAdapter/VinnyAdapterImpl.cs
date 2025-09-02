using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VinnyLibConverterCommon.Interfaces;
using VinnyLibConverterCommon.VinnyLibDataStructure;
using VinnyLibConverterCommon;

using System.Diagnostics;

namespace VinnyNavisworksAdapter
{
    public class VinnyAdapterImpl : ICadExportProcessing
    {
        public static void Start()
        {

        }

        public VinnyLibDataStructureModel CreateData()
        {
            throw new NotImplementedException();
        }

        public void ExportTo(VinnyLibDataStructureModel vinnyData, ImportExportParameters outputParameters)
        {
            throw new NotImplementedException();
        }
    }
}
