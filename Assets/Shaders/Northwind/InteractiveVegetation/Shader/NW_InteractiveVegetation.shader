Shader "Northwind/Interactive Vegetation/Vegetation" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Vegeation Texture", 2D) = "white" {}
		[Normal]
		_NormalMap("Normal Map", 2D) = "bump" {}
		_CutOff ("Cut Off", Range(0,1)) = 0.5

		//Deformer Interaction

		_DeformerMode("Deformer Mode (0 = vertex.y, 1 = vertex.color)", Int) = 0

		_DeformerStiffness("Deform Stiffness", Range(0,1)) = 0.5

		//Wind

		_WindMode("Wind Mode (0 = vertex.y, 1 = vertex.color)", Int) = 0
		_DEBUG_VertexColors("Debug: Vertex Colors Only", Int) = 0

		_WindStrength("Wind Strength", float) = 1
		_WindSpeed("Wind Speed", float) = 5
		_WindScale("Wind Scale", float) = 0.5

		//Burn Dissolve

		_BurnColor("Burn Edge Color", Color) = (0,0,0,1)
		_BurnColorStart("Burn Color Start", Range(0,1)) = 0.6
		_BurnCutStart("Burn Cut Start", Range(0,1)) = 0.9

		_BurnDuration("Burn Duration", Range(0.5, 50)) = 1
		_ObjectHeight("Object Height", Float) = 1

		_BurnMode("Burn Mode (0 = Procedural, 1 = UV)", Int) = 0

		//Burn Procedural
		_BurnNoiseScale("Burn Noise Scale", Float) = 5
		_BurnNoiseMovementX("Burn Noise MovementX", Float) = 0
		_BurnNoiseMovementY("Burn Noise MovementY", Float) = 0

		//Burn UV
		_BurnNoiseMap("Burn Noise Map", 2D) = "black" {}

		//EDITOR VALUES
		_E_ToggleMain("E_Main", Int) = 1
		_E_ToggleDeform("E_Deform", Int) = 1
		_E_ToggleWind("E_Wind", Int) = 1
		_E_ToggleBurn("E_Burn", Int) = 1
	}
	SubShader {
		Tags { "RenderType" = "Opaque" "ForceNoShadowCasting" = "True" "IgnoreProjector" = "True" "DisableBatching" = "True" }
		
		Cull Off

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		#include "../../Libs/noiseSimplex.cginc"

		struct Input {
			float2 uv_MainTex;
			float2 uv_NormalMap;
			float2 uv_BurnNoiseMap;
			float4 vertexData : TEXCOORD1; //x = vertex height; yzw = vertex color
			float3 worldPos;
			INTERNAL_DATA
		};

		//Editor values
		int _E_ToggleMain;
		int _E_ToggleDeform;
		int _E_ToggleWind;
		int _E_ToggleBurn;

		//Main values

		sampler2D _MainTex;
		sampler2D _NormalMap;
		float4 _Color;
		float _CutOff;

		//Interactive vegetation values

		int _DEBUG_VertexColors;

		int _DeformerMode;

		float _DeformerStiffness;

		int _WindMode;

		sampler2D _VegetationDeformTex;
		float4 _VegetationTextureArea;

		float _WindStrength;
		float _WindSpeed;
		float _WindScale;

		//Burn values

		float _BurnTime; //When the vegetation began to burn

		float4 _BurnColor;
		float _BurnColorStart;
		float _BurnDuration;

		float _ObjectHeight;
		float _BurnCutStart;

		int _BurnMode;

		//Burn Mode Procedural

		float _BurnNoiseScale;
		float _BurnNoiseMovementX;
		float _BurnNoiseMovementY;

		//Burn Mode UV

		sampler2D _BurnNoiseMap;

		float3 InteractionAndWind(float4 worldPos, float4 vertexPos, float4 vertexColor, float pi) {
			float3 lOffset = float3(0, 0, 0);

			//Interaction
			if (_E_ToggleDeform) {
				float4 lValue = (tex2Dlod(_VegetationDeformTex, float4((worldPos.xz - _VegetationTextureArea.xy + _VegetationTextureArea.zw / 2) / _VegetationTextureArea.zw, 0, 0)) - float4(0.5, 0.5, 0.5, 0.5)) * 2;
				float3 lDeformOffset = lValue.rgb * (1 - _DeformerStiffness);
				lDeformOffset.y *= 0;

				if (_DeformerMode == 0) {
					lDeformOffset *= vertexPos.y;
				}
				else if (_DeformerMode == 1) {
					lDeformOffset *= vertexColor.r;
				}

				lOffset += lDeformOffset;
			}

			//Wind
			if (_E_ToggleWind) {
				float2 lWind = float2(sin(snoise(float2(-_Time.x * _WindSpeed + worldPos.x * _WindScale, _Time.x * _WindSpeed + worldPos.z* _WindScale)) * pi), sin(snoise(float2(_Time.x * _WindSpeed + worldPos.z* _WindScale, -_Time.x * _WindSpeed + worldPos.x* _WindScale)) * pi)) * _WindStrength * vertexPos.y * vertexPos.y;
				float3 lWindOffset = float3(lWind.x, 0, lWind.y);

				//Vertex Paint Data -> Strength
				if (_WindMode == 0) {
					lWindOffset *= vertexPos.y;
				}
				else if (_WindMode == 1) {
					lWindOffset *= vertexColor.r;
				}

				lOffset += lWindOffset;
			}

			//Turn Into Rotate
			float lY = length(lOffset.xz);
			lOffset.y = -(lY*lY);
			
			return lOffset;
		}

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			//PI helper
			float pi = 3.14159265359;

			float4 lWorldPos = mul(unity_ObjectToWorld, v.vertex);
			float4 lObjectRoot = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));

			o.vertexData = float4((lWorldPos.y - lObjectRoot.y) / _ObjectHeight, v.color.rgb);

			//Aplly Interaction and Wind
			lWorldPos.xyz += InteractionAndWind(lWorldPos, v.vertex, v.color, 3.14159265359);

			//Back to Object Space
			lWorldPos = mul(unity_WorldToObject, lWorldPos);

			//o.vertexColor = v.color;

			v.vertex = lWorldPos;
		}

		float Remap(float value, float low1, float high1, float low2, float high2) {
			return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
		}

		float4 BurnProcedural(float4 baseColor, float3 worldPos, float vertexHeight) {
			float4 lColor = baseColor;

			//Time to a 0 - 1 Space
			float lHeightProcess = (vertexHeight - 1);
			float lProcess = clamp((_Time.y - _BurnTime) / _BurnDuration, 0, 1) * 1.75 - 0.75 - (1 - vertexHeight) * 0.1;

			float3 lMove = float3(sin(_BurnNoiseMovementX * _Time.y) * 0.1, _BurnNoiseMovementY * _Time.y, sin(_BurnNoiseMovementX * _Time.y) * 0.1);

			float lVal = saturate((snoise((worldPos.xyz + lMove) * _BurnNoiseScale) + 1) / 2);
			lVal *= saturate((snoise((worldPos.zyx - lMove) * _BurnNoiseScale * 3) + 1) / 2);
			lVal *= 1 - vertexHeight;

			float4 lNoise = float4(lVal, lVal, lVal, 1);
			if ((lNoise.r) < 0.5) {
				lNoise.rgb = 1 - lNoise.rgb;
			}

			lNoise.rgb += lProcess * 2;

			lColor.rgb = lerp(baseColor.rgb, _BurnColor.rgb, saturate(Remap(lNoise.r, _BurnColorStart, _BurnCutStart, 0, 1)));
			if (lNoise.r > _BurnCutStart) {
				lColor = float4(0, 0, 0, 0);
			}

			return  lColor;
		}

		float4 BurnUV(float4 baseColor, float2 burnUV) {
			float4 lColor = baseColor;

			//Time to a 0 - 1 Space
			float lProcess = clamp((_Time.y - _BurnTime) / _BurnDuration, 0, 1) * 1.75 - 0.75;

			float lVal = tex2D(_BurnNoiseMap, burnUV);

			float4 lNoise = float4(lVal, lVal, lVal, 1);
			if ((lNoise.r) < 0.5) {
				lNoise.rgb = 1 - lNoise.rgb;
			}

			lNoise.rgb += lProcess * 2;

			lColor.rgb = lerp(baseColor.rgb, _BurnColor.rgb, saturate(Remap(lNoise.r, _BurnColorStart, _BurnCutStart, 0, 1)));
			if (lNoise.r > _BurnCutStart) {
				lColor = float4(0, 0, 0, 0);
			}

			return  lColor;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) {
			float4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

			//Burn
			float4 col = float4(0, 0, 0, 0);
			if (_E_ToggleBurn) {
				if (_BurnMode == 0) {
					col = BurnProcedural(c, IN.worldPos, IN.vertexData.x);
				}
				if (_BurnMode == 1) {
					col = BurnUV(c, IN.uv_BurnNoiseMap);
				}
			}
			else {
				col = c;
			}

			//Clip
			if (col.a <= 0.1) {
				clip(-1);
			}
			if (_DEBUG_VertexColors) {
				o.Albedo = float4(IN.vertexData.yzw, 1);
			}
			else {
				o.Albedo = col.rgb;
			}
			o.Alpha = col.a;

			o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));
		}


		ENDCG
	}
	CustomEditor "Northwind.Shaders.InteractiveVegetation.Editors.NW_InteractiveVegetationEditor"
	FallBack "Diffuse"
}
