// Upgrade NOTE: upgraded instancing buffer 'CognitionDamageEffect' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Cognition/DamageEffect"
{
	Properties
	{
		_DamageTexture("Damage Texture", 2D) = "white" {}
		_DamageAmount("Damage Amount", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma surface surf Standard alpha:fade keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _DamageTexture;
		uniform float4 _DamageTexture_ST;

		UNITY_INSTANCING_BUFFER_START(CognitionDamageEffect)
			UNITY_DEFINE_INSTANCED_PROP(float, _DamageAmount)
#define _DamageAmount_arr CognitionDamageEffect
		UNITY_INSTANCING_BUFFER_END(CognitionDamageEffect)

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_DamageTexture = i.uv_texcoord * _DamageTexture_ST.xy + _DamageTexture_ST.zw;
			o.Albedo = tex2D( _DamageTexture, uv_DamageTexture ).rgb;
			float _DamageAmount_Instance = UNITY_ACCESS_INSTANCED_PROP(_DamageAmount_arr, _DamageAmount);
			o.Alpha = _DamageAmount_Instance;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=15201
-1414;107;1287;714;871.9691;247.5305;1;True;True
Node;AmplifyShaderEditor.SamplerNode;8;-468.9691,-102.5305;Float;True;Property;_DamageTexture;Damage Texture;0;0;Create;True;0;0;False;0;None;e2c183f00b9d3644295f1a7cb99ba7e9;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;6;-390.9691,169.4695;Float;False;InstancedProperty;_DamageAmount;Damage Amount;1;0;Create;True;0;0;False;0;0;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;Cognition/DamageEffect;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;0;False;0;Transparent;0.5;True;False;0;True;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;5;False;-1;10;False;-1;-1;False;-1;-1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;0;0;False;0;0;0;False;-1;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;0;0;8;0
WireConnection;0;9;6;0
ASEEND*/
//CHKSM=9AFBA29B59F27EC5C07D99262660DAC9A8C65F3B