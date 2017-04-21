using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Northwind.Shaders.InteractiveVegetation.Editors
{
    [CustomEditor(typeof(BurnStarter))]
    public class BurnStarterEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            BurnStarter burnStarter = (BurnStarter)target;

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Burn"))
            {
                burnStarter.StartGlobalBurn();
            }

            if (GUILayout.Button("Reset"))
            {
                burnStarter.ResetBurn();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}