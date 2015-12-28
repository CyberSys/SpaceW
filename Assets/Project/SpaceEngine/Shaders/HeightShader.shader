﻿Shader "Custom/HeightShader" 
{
	Properties
	{
		_Displacement("Displacement", float) = 1
		_Color("Color", color) = (1,1,1,0)
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 300

		CGPROGRAM
		#pragma surface surf Lambert addshadow fullforwardshadows vertex:disp nolightmap
		#pragma glsl
		#pragma target 3.0

		#include "TCCommon.cginc"

		float _Displacement;
		float4 _Color;

		struct appdata
		{
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float3 color : COLOR;
			float2 uv0 : TEXCOORD0;
			float2 uv1 : TEXCOORD1;
		};

		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		float CurrentNoiseF(float3 p)
		{
			return Cell2Noise(p) * _Displacement;
		}

		void disp(inout appdata v)
		{
			float noise = CurrentNoiseF(v.vertex.xyz);

			v.vertex.xyz += v.normal * noise;
		}

		void surf(Input IN, inout SurfaceOutput o)
		{
			float3 objectPos = mul(_World2Object, float4(IN.worldPos, 1)).xyz;

			float noise = CurrentNoiseF(objectPos);

			o.Albedo = noise * _Color.rgb;
		}
		ENDCG
	}
	FallBack "Diffuse"
}