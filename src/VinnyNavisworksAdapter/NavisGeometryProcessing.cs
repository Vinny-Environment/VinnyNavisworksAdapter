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

using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Takeoff;

using NAV = Autodesk.Navisworks.Api;
using ComBridge = Autodesk.Navisworks.Api.ComApi.ComApiBridge;
using COMApi = Autodesk.Navisworks.Api.Interop.ComApi;
using Autodesk.Navisworks.Api.Interop.ComApi;
using System.Runtime.InteropServices;


namespace VinnyNavisworksAdapter
{
    internal class NavisGeometryProcessing : COMApi.InwSimplePrimitivesCB
    {
        internal IEnumerable<double>? LocalToWorldTransformation { get; set; }
        public List<int[]> Faces;
        public List<double[]> Points;

        internal NavisGeometryProcessing()
        {
            Faces = new List<int[]>();
            Points = new List<double[]>();

        }
        public void Line(COMApi.InwSimpleVertex v1,
                COMApi.InwSimpleVertex v2)
        {
            // do your work
        }

        public void Point(COMApi.InwSimpleVertex v1)
        {
            // do your work
        }

        public void SnapPoint(COMApi.InwSimpleVertex v1)
        {
            // do your work
        }

        public void Triangle(COMApi.InwSimpleVertex v1,
                COMApi.InwSimpleVertex v2,
                COMApi.InwSimpleVertex v3)
        {
            if (v1 == null || v2 == null || v3 == null)
            {
                return;
            }

            using var vD1 = TransformVectorToOrientation(
              ApplyTransformation(VectorFromVertex(v1), LocalToWorldTransformation),
              IsUpright
            );
            using var vD2 = TransformVectorToOrientation(
              ApplyTransformation(VectorFromVertex(v2), LocalToWorldTransformation),
              IsUpright
            );
            using var vD3 = TransformVectorToOrientation(
              ApplyTransformation(VectorFromVertex(v3), LocalToWorldTransformation),
              IsUpright
            );

            Points.Add(new double[] { vD1.X, vD1.Y, vD1.Z });
            Points.Add(new double[] { vD2.X, vD2.Y, vD2.Z });
            Points.Add(new double[] { vD3.X, vD3.Y, vD3.Z });

            Faces.Add(new int[] { Points.Count - 3, Points.Count - 2, Points.Count - 1 });
        }

        

        private static NAV.Vector3D TransformVectorToOrientation(NAV.Vector3D v, bool isUpright) =>
    isUpright ? v : new NAV.Vector3D(v.X, -v.Z, v.Y);

        private static NAV.Vector3D ApplyTransformation(NAV.Vector3D vector3, IEnumerable<double>? matrixStore)
        {
            var matrix = matrixStore!.ToList();
            var t1 = matrix[3] * vector3.X + matrix[7] * vector3.Y + matrix[11] * vector3.Z + matrix[15];
            var vectorDoubleX = (matrix[0] * vector3.X + matrix[4] * vector3.Y + matrix[8] * vector3.Z + matrix[12]) / t1;
            var vectorDoubleY = (matrix[1] * vector3.X + matrix[5] * vector3.Y + matrix[9] * vector3.Z + matrix[13]) / t1;
            var vectorDoubleZ = (matrix[2] * vector3.X + matrix[6] * vector3.Y + matrix[10] * vector3.Z + matrix[14]) / t1;

            return new NAV.Vector3D(vectorDoubleX, vectorDoubleY, vectorDoubleZ);
        }

        private static NAV.Vector3D VectorFromVertex(InwSimpleVertex v)
        {
            var arrayV = (Array)v.coord;
            return new NAV.Vector3D((double)arrayV.GetValue(1), (double)arrayV.GetValue(2), (double)arrayV.GetValue(3));
        }

        private bool IsUpright = true;


    }
}
