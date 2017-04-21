using UnityEngine;
using UnityEditor;
using System;
using Northwind.Editors.Shaders;

namespace Northwind.Shaders.InteractiveVegetation.Editors
{
    public class NW_InteractiveVegetationEditor : ShaderGUI
    {
        enum BurnModes { Procedural, UV };
        enum WindModes { VertexHeight, VertexPainting };

        GUIContent[] burnModes = new GUIContent[] { new GUIContent("Procedural"), new GUIContent("UV") };
        GUIContent[] affectModes = new GUIContent[] { new GUIContent("Vertex Height"), new GUIContent("Vertex Painting")};

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Material targetMat = materialEditor.target as Material;
            MatEdit.SetScope(targetMat);

            /////////////////////////////////
            //Main
            if (MatEdit.BeginFoldGroup(new GUIContent("Main"), "_E_ToggleMain"))
            {
                MatEdit.TextureField(new GUIContent("Albedo", "The main vegetation texture"), "_MainTex");

                MatEdit.NormalTextureField(new GUIContent("Normal", "The normal map for the vegetation"), "_NormalMap");

                MatEdit.ColorField(new GUIContent("Tint Color", "The color tint for the texture"), "_Color");
                MatEdit.SliderField(new GUIContent("Cut Off", "The alpha strength which will be invisible"), "_CutOff", 0f, 1f);
            }
            MatEdit.EndGroup();

            /////////////////////////////////
            //Interactive Deformation
            if (MatEdit.BeginToggleGroup(new GUIContent("Interactive Deformation"), "_E_ToggleDeform", MatEdit.GroupStyles.Main, false, true))
            {
                MatEdit.SliderField(new GUIContent("Deform Stiffness", "The stiffness of the vegetation"), "_DeformerStiffness", 0f, 1f);

                int lIndex = MatEdit.EnumField(new GUIContent("Deformer Deform Mode", "The mode for the vertex strength"), "_DeformerMode", affectModes);

                if (affectModes[lIndex].text == "Vertex Painting")
                {
                    MatEdit.BeginGroup(new GUIContent(), MatEdit.GroupStyles.Sub);
                    MatEdit.ToggleField(new GUIContent("Show Vertex Colors", "DEBUG: draws the vertex Colors"), "_DEBUG_VertexColors");
                    MatEdit.EndGroup();
                }
            }
            MatEdit.EndGroup();

            /////////////////////////////////
            //Wind
            if (MatEdit.BeginToggleGroup(new GUIContent("Interactive Wind", "The wind vegetation effect, can be disabled"), "_E_ToggleWind", MatEdit.GroupStyles.Main, false, true))
            {
                MatEdit.FloatField(new GUIContent("Wind Strength", "The strength of the deformation"), "_WindStrength");
                MatEdit.FloatField(new GUIContent("Wind Speed", "The speed for the wind"), "_WindSpeed");
                MatEdit.FloatField(new GUIContent("Wind Scale", "The scale of the wind noise"), "_WindScale");

                int lIndex = MatEdit.EnumField(new GUIContent("Wind Deform Mode", "The mode for the vertex strength"), "_WindMode", affectModes);

                if (affectModes[lIndex].text == "Vertex Painting")
                {
                    MatEdit.BeginGroup(new GUIContent(), MatEdit.GroupStyles.Sub);
                    MatEdit.ToggleField(new GUIContent("Show Vertex Colors", "DEBUG: draws the vertex Colors"), "_DEBUG_VertexColors");
                    MatEdit.EndGroup();
                }
            }
            MatEdit.EndGroup();

            /////////////////////////////////
            //Burn
            if (MatEdit.BeginToggleGroup(new GUIContent("Interactive Burn", "The burning vegetation effect, can be disabled"), "_E_ToggleBurn", MatEdit.GroupStyles.Main, false, true))
            {
                MatEdit.ColorField(new GUIContent("Burn Color", "The Color which will be faded at the edge of burning vegetation"), "_BurnColor");
                
                MatEdit.MinMaxSliderField(new GUIContent("Color to Cut", "Defines the Range where the Color Fading starts and when it fades into the cut off"), "_BurnColorStart", "_BurnCutStart", 0f, 1f, true);

                MatEdit.SliderField(new GUIContent("Burn Duration", "The length the burning takes"), "_BurnDuration", 0f, 50f);

                MatEdit.BeginGroup(MatEdit.GroupStyles.Sub);

                switch (burnModes[MatEdit.EnumField(new GUIContent("Burn Mode"), "_BurnMode", burnModes)].text)
                {
                    case "Procedural":
                        {
                            MatEdit.FloatField(new GUIContent("Object Height", "The height of the object in world units from the pivot point"), "_ObjectHeight");

                            MatEdit.FloatField(new GUIContent("Noise Scale", "The Scale of the procedural noise"), "_BurnNoiseScale");

                            MatEdit.FloatAsVectorField(new GUIContent("Noise Movement", "Movement of the noise | X = Horizontal and Y = Vertical"), "_BurnNoiseMovementX", "_BurnNoiseMovementY");
                        }
                        break;
                    case "UV":
                        {
                            MatEdit.TextureField(new GUIContent("Burn Noise Map", "The Burn Map layouted via UV | White = first burned and Black = last burned"), "_BurnNoiseMap");
                        }
                        break;
                }
                MatEdit.EndGroup();
            }
            MatEdit.EndGroup();

            if (targetMat.GetInt("_DEBUG_VertexColors") == 1)
                EditorGUILayout.HelpBox("Vertex Colors are turned on", MessageType.Warning);
        }
    }
}