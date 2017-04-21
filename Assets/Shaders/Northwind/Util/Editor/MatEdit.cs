using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Northwind.Editors.Shaders
{
    public static class MatEdit
    {
        #region MatEdit_Customizations

        public enum PackagePart {x, y, z, w};

        public enum GroupStyles {Main = 0, Sub = 1};
        private static GUIStyle[] groupStyles = new GUIStyle[] {EditorStyles.miniButton, EditorStyles.helpBox};

        public enum TextureFieldType {Small = 16, Medium = 32, Large = 64};

        #endregion MatEdit_Customizations

        #region MatEdit_Stats

        private static Material scopeMaterial;

        #endregion MatEdit_Stats

        #region MatEdit_HelperClasses

        [System.Serializable]
        private class AnimationCurveContainer
        {
            public AnimationCurve localCurve;

            public AnimationCurveContainer(AnimationCurve curve)
            {
                localCurve = curve;
            }
        }

        #endregion MatEdit_HelperClasses

        #region MatEdit_HelperFunctions

        private static Texture2D AnimationCurveToTexture(AnimationCurve curve, int steps, bool debug = false)
        {
            System.Diagnostics.Stopwatch lWatch = new System.Diagnostics.Stopwatch();
            if (debug)
            {
                lWatch.Start();
            }

            Texture2D lResult = new Texture2D(steps, 1);

            Color[] lPixels = new Color[steps];
            float length = steps;
            for (int p = 0; p < steps; p++)
            {
                float point = p;
                float lVal = curve.Evaluate(point / length);
                lPixels[p] = new Color(lVal, (lVal - 1f), (lVal - 2f), 1f);
            }

            lResult.SetPixels(lPixels);
            lResult.Apply();

            if (debug)
            {
                lWatch.Stop();
                Debug.Log("<color=green>Success:</color> Converted AnimationCurve to Texture2D in " + lWatch.ElapsedMilliseconds + "ms");
            }

            return lResult;
        }

        private static Texture2D GradientToTexture(Gradient gradiant, int steps, bool debug = false)
        {
            System.Diagnostics.Stopwatch lWatch = new System.Diagnostics.Stopwatch();
            if (debug)
            {
                lWatch.Start();
            }

            Texture2D lResult = new Texture2D(steps, 1);

            Color[] lPixels = new Color[steps];
            float length = steps;
            for (int p = 0; p < steps; p++)
            {
                float point = p;
                lPixels[p] = gradiant.Evaluate(point / length);
            }

            lResult.SetPixels(lPixels);
            lResult.Apply();

            if (debug)
            {
                lWatch.Stop();
                Debug.Log("<color=green>Success:</color> Converted Gradient to Texture2D in " + lWatch.ElapsedMilliseconds + "ms");
            }

            return lResult;
        }

        #endregion MatEdit_HelperFunctions

        #region SettingsFunctions

        public static void SetScope(Material material)
        {
            scopeMaterial = material;
        }

        #endregion SettingsFunctions

        //Editor Part

        #region GroupFields

        //Fold Group
        public static bool BeginFoldGroup(GUIContent content, string toggleID, GroupStyles style = GroupStyles.Main, bool spacing = false, bool writeToShader = false)
        {
            return BeginFoldGroup(content, toggleID, scopeMaterial, style, spacing, writeToShader);
        }

        public static bool BeginFoldGroup(GUIContent content, string toggleID, Material material, GroupStyles style = GroupStyles.Main, bool spacing = false, bool writeToShader = false)
        {
            string lKey = "MatEdit:" + material.GetInstanceID() + "-> ToggleID:" + toggleID;

            EditorGUILayout.BeginVertical(groupStyles[(int)style]);

            if (GUILayout.Button(content, EditorStyles.boldLabel))
            {
                if (writeToShader)
                {
                    material.SetInt(toggleID, scopeMaterial.GetInt(toggleID) == 1 ? 0 : 1);
                } else
                {
                    EditorPrefs.SetBool(lKey, !EditorPrefs.GetBool(lKey));
                }
            }

            if (spacing)
            {
                EditorGUILayout.Space();
            }

            if (writeToShader)
            {
                return material.GetInt(toggleID) == 1;
            }
            else
            {
                return EditorPrefs.GetBool(lKey);
            }
        }

        //Toggle Group
        public static bool BeginToggleGroup(GUIContent content, string toggleID, GroupStyles style = GroupStyles.Main, bool spacing = false, bool writeToShader = false)
        {
            return BeginToggleGroup(content, toggleID, scopeMaterial, style, spacing, writeToShader);
        }

        public static bool BeginToggleGroup(GUIContent content, string toggleID, Material material, GroupStyles style = GroupStyles.Main, bool spacing = false, bool writeToShader = false)
        {
            string lKey = "MatEdit:" + material.GetInstanceID() + "-> ToggleID:" + toggleID;

            EditorGUILayout.BeginVertical(groupStyles[(int)style]);

            bool toggle = false;
            if (writeToShader)
            {
                toggle = material.GetInt(toggleID) == 1;
            }
            else
            {
                toggle = EditorPrefs.GetBool(lKey);
            }
            toggle = EditorGUILayout.BeginToggleGroup(content, toggle);
            EditorGUILayout.EndToggleGroup();

            if (writeToShader)
            {
                material.SetInt(toggleID, toggle ? 1 : 0);
            }
            else
            {
                EditorPrefs.SetBool(lKey, toggle);
            }

            if (spacing)
            {
                EditorGUILayout.Space();
            }

            return toggle;
        }

        //Static Group
        public static void BeginGroup(GUIContent content, GroupStyles style = GroupStyles.Main, bool spacing = false)
        {
            BeginGroup(content, scopeMaterial, style, spacing);
        }

        public static void BeginGroup(GUIContent content, Material material, GroupStyles style = GroupStyles.Main, bool spacing = false)
        {
            EditorGUILayout.BeginVertical(groupStyles[(int)style]);
            EditorGUILayout.LabelField(content, EditorStyles.boldLabel);

            if (spacing)
            {
                EditorGUILayout.Space();
            }
        }

        //End Current Group
        public static void EndGroup(bool spacing = false)
        {
            if (spacing)
            {
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndVertical();
        }

        #endregion GroupFields

        #region TextureFields

        public static void TextureField(GUIContent content, string property, TextureFieldType size = TextureFieldType.Small)
        {
            TextureField(content, property, scopeMaterial, size);
        }

        public static void TextureField(GUIContent content, string property, Material material, TextureFieldType size = TextureFieldType.Small)
        {
            Texture2D mainTexture = (Texture2D)EditorGUILayout.ObjectField(content, material.GetTexture(property), typeof(Texture2D), false, GUILayout.Height((float)size));
            material.SetTexture(property, mainTexture);
        }

        public static void NormalTextureField(GUIContent content, string property, TextureFieldType size = TextureFieldType.Small)
        {
            NormalTextureField(content, property, scopeMaterial, size);
        }

        public static void NormalTextureField(GUIContent content, string property, Material material, TextureFieldType size = TextureFieldType.Small)
        {
            Texture2D normalTexture = (Texture2D)EditorGUILayout.ObjectField(content, material.GetTexture(property), typeof(Texture), false, GUILayout.Height((float)size));
            if (normalTexture != null)
            {
                TextureImporter lImporter = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(normalTexture.GetInstanceID()));
                if (lImporter.textureType != TextureImporterType.NormalMap)
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
            material.SetTexture(property, normalTexture);
        }

        public static void TextureDataField(GUIContent content, string property)
        {
            TextureDataField(content, property, scopeMaterial);
        }

        public static void TextureDataField(GUIContent content, string property, Material material)
        {
            if (content.text != "")
            {
                EditorGUILayout.LabelField(content);
            }

            MatEdit.VectorField(new GUIContent("Tiling", ""), property, PackagePart.x, PackagePart.y);
            MatEdit.VectorField(new GUIContent("Offset", ""), property, PackagePart.z, PackagePart.w);
        }

        #endregion TextureFields

        #region SimpleFields

        //Color Field
        public static void ColorField(GUIContent content, string property)
        {
            ColorField(content, property, scopeMaterial);
        }

        public static void ColorField(GUIContent content, string property, Material material)
        {
            material.SetColor(property, EditorGUILayout.ColorField(content, material.GetColor(property)));
        }

        //Toggle Field
        public static void ToggleField(GUIContent content, string property)
        {
            ToggleField(content, property, scopeMaterial);
        }

        public static void ToggleField(GUIContent content, string property, Material material)
        {
            material.SetInt(property, EditorGUILayout.Toggle(content, material.GetInt(property) == 1 ? true : false) ? 1 : 0);
        }

        //Int Field
        public static void IntField(GUIContent content, string property)
        {
            IntField(content, property, scopeMaterial);
        }

        public static void IntField(GUIContent content, string property, Material material)
        {
            material.SetInt(property, EditorGUILayout.IntField(content, material.GetInt(property)));
        }

        //Float Field
        public static void FloatField(GUIContent content, string property)
        {
            FloatField(content, property, scopeMaterial);
        }

        public static void FloatField(GUIContent content, string property, Material material)
        {
            material.SetFloat(property, EditorGUILayout.FloatField(content, material.GetFloat(property)));
        }

        //Slider Field
        public static void SliderField(GUIContent content, string property, float min, float max, bool round = false)
        {
            SliderField(content, property, min, max, scopeMaterial, round);
        }

        public static void SliderField(GUIContent content, string property, float min, float max, Material material, bool round = false)
        {
            float lValue = EditorGUILayout.Slider(content, material.GetFloat(property), min, max);
            if (round)
            {
                lValue = Mathf.Round(lValue);
            }
            material.SetFloat(property, lValue);
        }

        //Packed Float Field
        public static void FloatPackedField(GUIContent content, string property, PackagePart part)
        {
            FloatPackedField(content, property, scopeMaterial, part);
        }

        public static void FloatPackedField(GUIContent content, string property, Material material, PackagePart part)
        {
            Vector4 lOriginal = material.GetVector(property);

            lOriginal[(int)part] = EditorGUILayout.FloatField(content, lOriginal[(int)part]);
            material.SetVector(property, lOriginal);
        }

        //Packed Slider Field
        public static void SliderPackedField(GUIContent content, string property, float min, float max, PackagePart part, bool round = false)
        {
            SliderPackedField(content, property, min, max, scopeMaterial, part, round);
        }

        public static void SliderPackedField(GUIContent content, string property, float min, float max, Material material, PackagePart part, bool round = false)
        {
            Vector4 lOriginal = material.GetVector(property);

            lOriginal[(int)part] = EditorGUILayout.Slider(content, lOriginal[(int)part], min, max);
            if (round)
            {
                lOriginal[(int)part] = Mathf.Round(lOriginal[(int)part]);
            }
            material.SetVector(property, lOriginal);
        }

        //Vector Field
        public static void VectorField(GUIContent content, string property, params PackagePart[] part)
        {
            VectorField(content, property, scopeMaterial, part);
        }

        public static void VectorField(GUIContent content, string property, Material material, params PackagePart[] part)
        {
            Vector4 lOriginal = material.GetVector(property);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(content);
            for (int p = 0; p < part.Length; p++)
            {
                lOriginal[(int)part[p]] = EditorGUILayout.FloatField(lOriginal[(int)part[p]]);
            }
            EditorGUILayout.EndHorizontal();
            material.SetVector(property, lOriginal);
        }

        #endregion SimpleFields

        #region SpecialFields

        //AnimationCurve Field
        public static void AnimationCurveField(GUIContent content, string property, int quality, bool debug = false)
        {
            AnimationCurveField(content, property, quality, scopeMaterial, debug);
        }

        public static void AnimationCurveField(GUIContent content, string property, int quality, Material material, bool debug = false)
        {
            string getJSON = EditorPrefs.GetString(material.GetInstanceID() + ":Animation Curve:" + property);
            AnimationCurve curve;
            if (getJSON != "")
            {
                curve = JsonUtility.FromJson<AnimationCurveContainer>(getJSON).localCurve;
            }
            else
            {
                curve = null;
            }

            if (curve == null)
            {
                curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
            }

            curve = EditorGUILayout.CurveField(content, curve);
            string setJSON = JsonUtility.ToJson(new AnimationCurveContainer(curve));
            EditorPrefs.SetString(material.GetInstanceID() + ":Animation Curve:" + property, setJSON);

            Texture2D mainTexture = AnimationCurveToTexture(curve, quality, debug);
            material.SetTexture(property, mainTexture);
        }

        #endregion SpecialFields
    }
}