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

using ComBridge = Autodesk.Navisworks.Api.ComApi.ComApiBridge;
using COMApi = Autodesk.Navisworks.Api.Interop.ComApi;
using Autodesk.Navisworks.Api.Interop.ComApi;
using System.Runtime.InteropServices;

namespace VinnyNavisworksAdapter
{
    public class VinnyAdapterImpl : ICadExportProcessing
    {
        public static VinnyAdapterImpl CreateInstance()
        {
            if (mInstance == null) mInstance = new VinnyAdapterImpl();
            if (mConverter == null) mConverter = VinnyLibConverterKernel.VinnyLibConverter.CreateInstance2();
            return mInstance;
        }

        public void Start()
        {
            var timer = new Stopwatch();
            timer.Start();
            VinnyLibConverterUI.VLC_UI_MainWindow vinnyWindow = new VinnyLibConverterUI.VLC_UI_MainWindow(false);
            VinnyLibConverterCommon.ImportExportParameters parameters = new ImportExportParameters();

#if DEBUG
            parameters = VinnyLibConverterCommon.ImportExportParameters.LoadFromFile(@"E:\Temp\Vinny\rengaTestParams2.XML");
#else
            if (vinnyWindow.ShowDialog() == true) parameters = vinnyWindow.VinnyParametets;
#endif
            ExportTo(CreateData(), parameters);

            timer.Stop();
            string time = $"Обработка завершена!\nЗатраченное время {timer.Elapsed.TotalSeconds} с.";

            System.Windows.MessageBox.Show(time);
        }

        private const string NavisParamCategory_Properties = "Navisworks Properties";

        private static void CollectFragments(InwOaPath path, Stack<InwOaFragment3> fragmentStack)
        {
            var fragments = path.Fragments();
            foreach (var fragment in fragments.OfType<InwOaFragment3>())
            {
                if (fragment.path?.ArrayData is not Array pathData1 || path.ArrayData is not Array pathData2)
                {
                    continue;
                }

                var pathArray1 = pathData1.Cast<int>().ToArray<int>();
                var pathArray2 = pathData2.Cast<int>().ToArray<int>();

                if (pathArray1.Length == pathArray2.Length && pathArray1.SequenceEqual(pathArray2))
                {
                    fragmentStack.Push(fragment);
                }
            }
        }

        private static bool IsSameFragmentPath(Array a1, Array a2) => a1.Length == a2.Length && a1.Cast<int>().SequenceEqual(a2.Cast<int>());

        private static bool ValidateFragmentPath(InwOaFragment3 fragment, InwOaPath path)
        {
            if (fragment.path?.ArrayData is not Array fragmentPathData || path.ArrayData is not Array pathData)
            {
                return false;
            }

            return IsSameFragmentPath(fragmentPathData, pathData);
        }

        private void ProcessingNavisObject(ModelItem navisObject, int parentObjectId)
        {
            VinnyLibDataStructureObject vinnyObjectDef = mVinnyModelDef.ObjectsManager.GetObjectById(mVinnyModelDef.ObjectsManager.CreateObject());
            vinnyObjectDef.Name = navisObject.DisplayName;
            vinnyObjectDef.UniqueId = navisObject.InstanceGuid.ToString("N");
            vinnyObjectDef.ParentId = parentObjectId;

            vinnyObjectDef.Parameters.Add(mVinnyModelDef.ParametersManager.CreateParameterValueWithDefs("NavisworksInstanceGuid", navisObject.InstanceGuid.ToString("N"), NavisParamCategory_Properties));
            vinnyObjectDef.Parameters.Add(mVinnyModelDef.ParametersManager.CreateParameterValueWithDefs("NavisworksInstanceHashCode", navisObject.InstanceHashCode, NavisParamCategory_Properties, VinnyLibDataStructureParameterDefinitionType.ParamInteger));

            //properties
            foreach (PropertyCategory navisPropCategory in navisObject.PropertyCategories)
            {
                foreach (DataProperty navisProp in navisPropCategory.Properties)
                {
                    object propValue;
                    VinnyLibDataStructureParameterDefinitionType propType;
                    VinnyNavisUtils.GetPropertyValueAndTupe(navisProp.Value, out propValue, out propType);

                    vinnyObjectDef.Parameters.Add(mVinnyModelDef.ParametersManager.CreateParameterValueWithDefs(navisProp.Name, navisProp.Value, NavisParamCategory_Properties, propType, navisProp.DisplayName));
                }
            }

            //geometry
            if (navisObject.HasGeometry)
            {
                ModelGeometry navisObjectGeometry = navisObject.Geometry;
                ModelItemCollection navisGeometryItems = new ModelItemCollection();
                navisGeometryItems.Add(navisObjectGeometry.Item);

                //convert to COM selection

                COMApi.InwOpState oState = ComBridge.State;
                COMApi.InwOpSelection oSel = ComBridge.ToInwOpSelection(navisGeometryItems);

                try
                {
                    var fragmentStack = new Stack<InwOaFragment3>();
                    var paths = oSel.Paths();
                    try
                    {
                        // Populate fragment stack with all fragments
                        foreach (InwOaPath path in paths)
                        {
                            CollectFragments(path, fragmentStack);
                        }

                        //var callbackListeners = new List<NavisGeometryProcessing>();
                        foreach (InwOaPath path in paths)
                        {
                            

                            foreach (var fragment in fragmentStack)
                            {
                                if (!ValidateFragmentPath(fragment, path))
                                {
                                    continue;
                                }

                                var matrix = fragment.GetLocalToWorldMatrix();
                                var transform = matrix as InwLTransform3f3;
                                if (transform?.Matrix is not Array matrixArray)
                                {
                                    continue;
                                }
                                var processor = new NavisGeometryProcessing();
                                processor.LocalToWorldTransformation = matrixArray.Cast<double>().ToArray();
                                fragment.GenerateSimplePrimitives(nwEVertexProperty.eNORMAL, processor);

                                if (processor.Faces.Count > 0)
                                {
                                    VinnyLibDataStructureGeometryMesh vinnyMesh = VinnyLibDataStructureGeometryMesh.asType(mVinnyModelDef.GeometrtyManager.GetMeshGeometryById(mVinnyModelDef.GeometrtyManager.CreateGeometry(VinnyLibDataStructureGeometryType.Mesh)));
                                    vinnyMesh.MaterialId = mVinnyModelDef.MaterialsManager.CreateMaterial(new int[] {
                                    Convert.ToInt32(navisObjectGeometry.ActiveColor.R * 255.0),
                                    Convert.ToInt32(navisObjectGeometry.ActiveColor.G * 255.0),
                                    Convert.ToInt32(navisObjectGeometry.ActiveColor.B * 255.0) });

                                    foreach (var face in processor.Faces)
                                    {
                                        vinnyMesh.AddFace(processor.Points[face[0]], processor.Points[face[1]], processor.Points[face[2]]);
                                    }

                                    mVinnyModelDef.GeometrtyManager.SetMeshGeometry(vinnyMesh.Id, vinnyMesh);

                                    int vinnyMeshPIid = mVinnyModelDef.GeometrtyManager.CreateGeometryPlacementInfo(vinnyMesh.Id);
                                    vinnyObjectDef.GeometryPlacementInfoIds.Add(vinnyMeshPIid);
                                }
                                //callbackListeners.Add(processor);
                            }


                        }
                    }
                    finally
                    {
                        if (paths != null)
                        {
                            Marshal.ReleaseComObject(paths);
                        }
                    }
                }
                finally
                {
                    if (oSel != null)
                    {
                        Marshal.ReleaseComObject(oSel);
                    }
                }
            }


            foreach (ModelItem navisObjectChild in navisObject.Children)
            {
                ProcessingNavisObject(navisObjectChild, vinnyObjectDef.Id);
            }
            mVinnyModelDef.ObjectsManager.SetObject(vinnyObjectDef.Id, vinnyObjectDef);

        }

        public VinnyLibDataStructureModel CreateData()
        {
            mVinnyModelDef = new VinnyLibDataStructureModel();

            //получение текущей сцены
            VinnyLibDataStructureObject vinnyNavisScene = mVinnyModelDef.ObjectsManager.GetObjectById(mVinnyModelDef.ObjectsManager.CreateObject());
            vinnyNavisScene.Name = "Navisworks scene";
            foreach (Model navisModel in Application.ActiveDocument.Models)
            {
                
                ModelItem navisRootObject = navisModel.RootItem;
                ProcessingNavisObject(navisRootObject, vinnyNavisScene.Id);
               
            }
            mVinnyModelDef.ObjectsManager.SetObject(vinnyNavisScene.Id, vinnyNavisScene);


            return mVinnyModelDef;
        }

        public void ExportTo(VinnyLibDataStructureModel vinnyData, ImportExportParameters outputParameters)
        {
            mConverter.ExportModel(vinnyData, outputParameters);
        }

        private VinnyLibDataStructureModel mVinnyModelDef;
        private static VinnyAdapterImpl mInstance;
        private static VinnyLibConverterKernel.VinnyLibConverter mConverter;
    }
}
