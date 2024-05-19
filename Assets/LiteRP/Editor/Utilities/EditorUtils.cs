using UnityEditor;
using UnityEngine;

namespace LiteRP.Editor
{
    internal static class EditorUtils
    {
        internal enum Unit
        {
            Metric,
            Percent
        }

        internal class Styles
        {
            //Measurements
            public static float defaultLineSpace =
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

    }
}