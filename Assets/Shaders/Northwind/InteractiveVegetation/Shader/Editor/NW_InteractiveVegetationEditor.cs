using UnityEngine;
using UnityEditor;
using System;

namespace Northwind.Shaders.InteractiveVegetation.Editors
{
    public class NW_InteractiveVegetationEditor : ShaderGUI
    {
        enum BurnModes { Procedural, UV };
        enum WindModes { VertexHeight, VertexPainting };

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Material targetMat = materialEditor.target as Material;

            /////////////////////////////////
            //Main
            EditorGUILayout.BeginVertical(EditorStyles.miniButton);
            targetMat.SetInt("_E_ToggleMain", EditorGUILayout.BeginToggleGroup(new GUIContent("Main", "The main part, disabling this has no effect"), targetMat.GetInt("_E_ToggleMain") == 1) ? 1 : 0);
            if (targetMat.GetInt("_E_ToggleMain") == 1)
            {
                EditorGUILayout.Space();

                Texture2D mainTexture = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Albedo", "The main vegetation texture"), targetMat.GetTexture("_MainTex"), typeof(Texture2D), false, GUILayout.Height(16f));
                targetMat.SetTexture("_MainTex", mainTexture);

                Texture2D normalTexture = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Normal", "The normal map for the vegetation"), targetMat.GetTexture("_NormalMap"), typeof(Texture), false, GUILayout.Height(16f));
                if (normalTexture != null)
                {
                    TextureImporter lImporter = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(normalTexture.GetInstanceID()));
                    if (lImporter.textureType == TextureImporterType.NormalMap)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.LabelField("Texture is no normal map!");
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Fix now"))
                        {
                            lImporter.textureType = TextureImporterType.NormalMap;
                            lImporter.convertToNormalmap = true;
                        }
                        if (GUILayout.Button("To Settings"))
                        {
                            Selection.activeObject = lImporter;
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                    }
                }

                targetMat.SetTexture("_NormalMap", normalTexture);

                targetMat.SetColor("_Color", EditorGUILayout.ColorField(new GUIContent("Tint Color", "The color tint for the texture"), targetMat.GetColor("_Color")));
                targetMat.SetFloat("_CutOff", EditorGUILayout.Slider(new GUIContent("Cut Off", "The alpha strength which will be invisible"), targetMat.GetFloat("_CutOff"), 0, 1));

                EditorGUILayout.Space();
            }
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndVertical();

            /////////////////////////////////
            //Interactive Deformation
            EditorGUILayout.BeginVertical(EditorStyles.miniButton);
            targetMat.SetInt("_E_ToggleDeform", EditorGUILayout.BeginToggleGroup(new GUIContent("Interactive Deformation", "The deformation vegetation effect, can be disabled"), targetMat.GetInt("_E_ToggleDeform") == 1) ? 1 : 0);
            if (targetMat.GetInt("_E_ToggleDeform") == 1)
            {
                EditorGUILayout.Space();

                targetMat.SetFloat("_DeformerStiffness", EditorGUILayout.Slider(new GUIContent("Deform Stiffness", "The stiffness of the vegetation"), targetMat.GetFloat("_DeformerStiffness"), 0, 1));

                targetMat.SetInt("_DeformerMode", (int)(WindModes)EditorGUILayout.EnumPopup(new GUIContent("Deformer Deform Mode", "The mode for the vertex strength"), (WindModes)targetMat.GetInt("_DeformerMode")));
                WindModes result = (WindModes)targetMat.GetInt("_DeformerMode");

                if (result == WindModes.VertexPainting)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    targetMat.SetInt("_DEBUG_VertexColors", EditorGUILayout.Toggle(new GUIContent("Show Vertex Colors", "DEBUG: draws the vertex Colors"), targetMat.GetInt("_DEBUG_VertexColors") == 1) ? 1 : 0);
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.Space();
            }
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndVertical();

            /////////////////////////////////
            //Wind
            EditorGUILayout.BeginVertical(EditorStyles.miniButton);
            targetMat.SetInt("_E_ToggleWind", EditorGUILayout.BeginToggleGroup(new GUIContent("Interactive Wind", "The wind vegetation effect, can be disabled"), targetMat.GetInt("_E_ToggleWind") == 1) ? 1 : 0);
            if (targetMat.GetInt("_E_ToggleWind") == 1)
            {
                EditorGUILayout.Space();

                targetMat.SetFloat("_WindStrength", EditorGUILayout.FloatField(new GUIContent("Wind Strength", "The strength of the deformation"), targetMat.GetFloat("_WindStrength")));
                targetMat.SetFloat("_WindSpeed", EditorGUILayout.FloatField(new GUIContent("Wind Speed", "The speed for the wind"), targetMat.GetFloat("_WindSpeed")));
                targetMat.SetFloat("_WindScale", EditorGUILayout.FloatField(new GUIContent("Wind Scale", "The scale of the wind noise"), targetMat.GetFloat("_WindScale")));

                targetMat.SetInt("_WindMode", (int)(WindModes)EditorGUILayout.EnumPopup(new GUIContent("Wind Deform Mode", "The mode for the vertex strength"), (WindModes)targetMat.GetInt("_WindMode")));
                WindModes result = (WindModes)targetMat.GetInt("_WindMode");

                if (result == WindModes.VertexPainting)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    targetMat.SetInt("_DEBUG_VertexColors", EditorGUILayout.Toggle(new GUIContent("Show Vertex Colors", "DEBUG: draws the vertex Colors"), targetMat.GetInt("_DEBUG_VertexColors") == 1) ? 1 : 0);
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.Space();
            }
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndVertical();

            /////////////////////////////////
            //Burn
            EditorGUILayout.BeginVertical(EditorStyles.miniButton);
            targetMat.SetInt("_E_ToggleBurn", EditorGUILayout.BeginToggleGroup(new GUIContent("Interactive Burn", "The burning vegetation effect, can be disabled"), targetMat.GetInt("_E_ToggleBurn") == 1) ? 1 : 0);
            if (targetMat.GetInt("_E_ToggleBurn") == 1)
            {
                EditorGUILayout.Space();

                targetMat.SetColor("_BurnColor", EditorGUILayout.ColorField(new GUIContent("Burn Color", "The Color which will be faded at the edge of burning vegetation"), targetMat.GetColor("_BurnColor")));

                //Burn Space
                float minValue = targetMat.GetFloat("_BurnColorStart");
                float maxValue = targetMat.GetFloat("_BurnCutStart");

                EditorGUILayout.MinMaxSlider(new GUIContent("Color to Cut", "Defines the Range where the Color Fading starts and when it fades into the cut off"), ref minValue, ref maxValue, 0f, 1f);

                EditorGUILayout.BeginHorizontal();

                minValue = EditorGUILayout.FloatField(minValue);
                maxValue = EditorGUILayout.FloatField(maxValue);

                minValue = Mathf.Clamp(Mathf.Min(minValue, maxValue), 0f, 1f);
                maxValue = Mathf.Clamp(Mathf.Max(maxValue, minValue), 0f, 1f);

                EditorGUILayout.EndHorizontal();

                targetMat.SetFloat("_BurnColorStart", minValue);
                targetMat.SetFloat("_BurnCutStart", maxValue);

                EditorGUILayout.Space();

                targetMat.SetFloat("_BurnDuration", EditorGUILayout.Slider(new GUIContent("Burn Duration", "The length the burning takes"), targetMat.GetFloat("_BurnDuration"), 0, 50));

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginVertical(EditorStyles.miniButtonMid);
                targetMat.SetInt("_BurnMode", (int)(BurnModes)EditorGUILayout.EnumPopup("Burn Mode", (BurnModes)targetMat.GetInt("_BurnMode")));
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();

                BurnModes result = (BurnModes)targetMat.GetInt("_BurnMode");

                switch (result)
                {
                    case BurnModes.Procedural:
                        {
                            targetMat.SetFloat("_ObjectHeight", EditorGUILayout.FloatField(new GUIContent("Object Height", "The height of the object in world units from the pivot point"), targetMat.GetFloat("_ObjectHeight")));

                            targetMat.SetFloat("_BurnNoiseScale", EditorGUILayout.FloatField(new GUIContent("Noise Scale", "The Scale of the procedural noise"), targetMat.GetFloat("_BurnNoiseScale")));

                            Vector2 movement = new Vector2(targetMat.GetFloat("_BurnNoiseMovementX"), targetMat.GetFloat("_BurnNoiseMovementY"));

                            movement = EditorGUILayout.Vector2Field(new GUIContent("Noise Movement", "Movement of the noise | X = Horizontal and Y = Vertical"), movement);

                            targetMat.SetFloat("_BurnNoiseMovementX", movement.x);
                            targetMat.SetFloat("_BurnNoiseMovementY", movement.y);
                        }
                        break;
                    case BurnModes.UV:
                        {
                            //_BurnNoiseMap("Burn Noise Map", 2D) = "black" {};
                            Texture2D burnTexture = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Burn Noise Map", "The Burn Map layouted via UV | White = first burned and Black = last burned"), targetMat.GetTexture("_BurnNoiseMap"), typeof(Texture2D), false);
                            targetMat.SetTexture("_BurnNoiseMap", burnTexture);
                        }
                        break;
                }
                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndVertical();

            if (targetMat.GetInt("_DEBUG_VertexColors") == 1)
                EditorGUILayout.HelpBox("Vertex Colors are turned on", MessageType.Warning);
        }
    }
}