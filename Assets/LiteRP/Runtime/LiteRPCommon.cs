using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP
{
    public static class ShaderKeywordStrings
    {
        public const string _ALPHATEST_ON = "_ALPHATEST_ON";                            //AlphaTest开启
        public const string _ALPHAPREMULTIPLY_ON = "_ALPHAPREMULTIPLY_ON";              //Alpha预乘开启
        public const string _ALPHAMODULATE_ON = "_ALPHAMODULATE_ON";                    //Alpha调制开启
        public const string _SURFACE_TYPE_TRANSPARENT = "_SURFACE_TYPE_TRANSPARENT";    //透明表面类型
        
                        
        public const string _NORMALMAP = "_NORMALMAP";                                  //使用Normal
        public const string _EMISSION = "_EMISSION";                                    //使用自发光
        public const string _RECEIVE_SHADOWS_OFF = "_RECEIVE_SHADOWS_OFF";              //接收阴影
    }

    internal static class ShaderPropertyId
    {
        public static readonly int alphaToMaskAvailable = Shader.PropertyToID("_AlphaToMaskAvailable");
    }
}