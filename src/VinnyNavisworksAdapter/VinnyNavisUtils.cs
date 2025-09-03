using Autodesk.Navisworks.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VinnyLibConverterCommon.VinnyLibDataStructure;

namespace VinnyNavisworksAdapter
{
    internal class VinnyNavisUtils
    {
        public static void GetPropertyValueAndTupe(VariantData _property, out object propValue, out VinnyLibDataStructureParameterDefinitionType propType)
        {
            propValue = null;
            propType = VinnyLibDataStructureParameterDefinitionType.ParamString;

            switch (_property.DataType)
            {
                case VariantDataType.Double:
                    propValue = _property.ToDouble();
                    propType = VinnyLibDataStructureParameterDefinitionType.ParamReal;
                    break;
                case VariantDataType.Int32:
                    propValue = _property.ToInt32();
                    propType = VinnyLibDataStructureParameterDefinitionType.ParamInteger;
                    break;
                case VariantDataType.Boolean:
                    propValue = _property.ToBoolean();
                    propType = VinnyLibDataStructureParameterDefinitionType.ParamBool;
                    break;
                case VariantDataType.DisplayString:
                    propValue = _property.ToDisplayString();
                    propType = VinnyLibDataStructureParameterDefinitionType.ParamString;
                    break;
                case VariantDataType.DateTime:
                    propValue = _property.ToDateTime();
                    propType = VinnyLibDataStructureParameterDefinitionType.ParamDate;
                    break;
                case VariantDataType.DoubleLength:
                    propValue = _property.ToDoubleLength();
                    propType = VinnyLibDataStructureParameterDefinitionType.ParamReal;
                    break;
                case VariantDataType.DoubleAngle:
                    propValue = _property.ToDoubleAngle();
                    propType = VinnyLibDataStructureParameterDefinitionType.ParamReal;
                    break;
                case VariantDataType.NamedConstant:
                    propValue = _property.ToNamedConstant();
                    propType = VinnyLibDataStructureParameterDefinitionType.ParamString;
                    break;
                case VariantDataType.IdentifierString:
                    propValue = _property.ToIdentifierString();
                    propType = VinnyLibDataStructureParameterDefinitionType.ParamString;
                    break;
                case VariantDataType.DoubleArea:
                    propValue = _property.ToDoubleArea();
                    propType = VinnyLibDataStructureParameterDefinitionType.ParamReal;
                    break;
                case VariantDataType.DoubleVolume:
                    propValue = _property.ToDoubleVolume();
                    propType = VinnyLibDataStructureParameterDefinitionType.ParamReal;
                    break;
                case VariantDataType.Point3D:
                    var p3d = _property.ToPoint3D();
                    propValue = $"{p3d.X};{p3d.Y};{p3d.Z}";
                    propType = VinnyLibDataStructureParameterDefinitionType.ParamString;
                    break;
                case VariantDataType.Point2D:
                    var p2d = _property.ToPoint2D();
                    propValue = $"{p2d.X};{p2d.Y}";
                    propType = VinnyLibDataStructureParameterDefinitionType.ParamString;
                    break;

            }

        }
    }
}
