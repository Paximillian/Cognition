// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Cognition/BasicTile"
{
	Properties
	{
		_Light_X("Light_X", Range( -1 , 1)) = 1
		_Light_Y("Light_Y", Range( -1 , 1)) = 1
		_Light_Z("Light_Z", Range( -1 , 1)) = 1
		_ShadowBrightness("ShadowBrightness", Range( 0 , 1)) = 0
		_ShadowPosition("ShadowPosition", Range( 0 , 1)) = 0.5
		_MainTex("MainTex", 2D) = "white" {}
		_ContrastFix("ContrastFix", Range( 0.1 , 2)) = 1
		_Brightness("Brightness", Range( 0 , 3)) = 1
		_VertexOffset("VertexOffset", Range( -1 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Off
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
			float3 worldNormal;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			fixed3 Albedo;
			fixed3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			fixed Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform float _ContrastFix;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform half _Brightness;
		uniform half _Light_X;
		uniform half _Light_Y;
		uniform half _Light_Z;
		uniform half _ShadowPosition;
		uniform half _ShadowBrightness;
		uniform float _VertexOffset;


		float4 CalculateContrast( float contrastValue, float4 colorTarget )
		{
			float t = 0.5 * ( 1.0 - contrastValue );
			return mul( float4x4( contrastValue,0,0,t, 0,contrastValue,0,t, 0,0,contrastValue,t, 0,0,0,1 ), colorTarget );
		}

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 appendResult72 = (float3(0 , _VertexOffset , 0));
			v.vertex.xyz += appendResult72;
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float3 appendResult12 = (float3(_Light_X , _Light_Y , _Light_Z));
			float3 normalizeResult48 = normalize( appendResult12 );
			float3 ase_worldNormal = i.worldNormal;
			float dotResult14 = dot( normalizeResult48 , ase_worldNormal );
			float temp_output_19_0 = saturate( dotResult14 );
			float4 temp_output_27_0 = ( saturate( ( CalculateContrast(_ContrastFix,tex2D( _MainTex, uv_MainTex )) * _Brightness ) ) * ( 0.6 + temp_output_19_0 ) * UNITY_LIGHTMODEL_AMBIENT );
			c.rgb = saturate( ( temp_output_27_0 * ( step( _ShadowPosition , temp_output_19_0 ) + _ShadowBrightness ) ) ).rgb;
			c.a = 1;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float3 appendResult12 = (float3(_Light_X , _Light_Y , _Light_Z));
			float3 normalizeResult48 = normalize( appendResult12 );
			float3 ase_worldNormal = i.worldNormal;
			float dotResult14 = dot( normalizeResult48 , ase_worldNormal );
			float temp_output_19_0 = saturate( dotResult14 );
			float4 temp_output_27_0 = ( saturate( ( CalculateContrast(_ContrastFix,tex2D( _MainTex, uv_MainTex )) * _Brightness ) ) * ( 0.6 + temp_output_19_0 ) * UNITY_LIGHTMODEL_AMBIENT );
			o.Emission = temp_output_27_0.rgb;
		}

		ENDCG
		CGPROGRAM
		#pragma only_renderers d3d9 d3d11 glcore gles gles3 d3d11_9x 
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			fixed4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldNormal = IN.worldNormal;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=15201
115;94;1332;665;1585.903;1410.045;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;13;-2901.249,-632.7272;Float;False;752.0415;340.3575;Comment;5;12;10;11;9;48;Light Direction;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-2851.248,-582.7272;Half;False;Property;_Light_X;Light_X;0;0;Create;True;0;0;False;0;1;1;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-2851.249,-499.0595;Half;False;Property;_Light_Y;Light_Y;1;0;Create;True;0;0;False;0;1;1;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-2848.902,-416.827;Half;False;Property;_Light_Z;Light_Z;2;0;Create;True;0;0;False;0;1;1;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;64;-1598.732,-1417.616;Float;False;1141.312;594.7847;Comment;9;25;31;27;30;28;32;76;77;78;Basic Texture;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;12;-2499.056,-538.1208;Float;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;25;-1580.063,-1371.231;Float;True;Property;_MainTex;MainTex;5;0;Create;True;0;0;False;0;e2c183f00b9d3644295f1a7cb99ba7e9;81c8c44b9a143f246925f7f30f6cfa0b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;32;-1576.816,-1182.229;Float;False;Property;_ContrastFix;ContrastFix;6;0;Create;True;0;0;False;0;1;1;0.1;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;48;-2350.193,-530.7896;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;57;-1413.836,-425.3054;Float;False;970.7277;362.0052;Comment;6;16;21;17;23;19;14;Basic Shading;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldNormalVector;15;-1840.444,-156.1288;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;77;-1251.162,-1146.307;Half;False;Property;_Brightness;Brightness;7;0;Create;True;0;0;False;0;1;1;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleContrastOpNode;31;-1233.719,-1365.065;Float;True;2;1;COLOR;0,0,0,0;False;0;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;14;-1363.836,-297.4223;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;76;-967.9326,-1364.622;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;19;-1198.838,-287.3599;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;78;-815.9026,-1363.045;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FogAndAmbientColorsNode;30;-1042.339,-1035.73;Float;False;UNITY_LIGHTMODEL_AMBIENT;0;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-1204.22,-375.3054;Half;False;Property;_ShadowPosition;ShadowPosition;4;0;Create;True;0;0;False;0;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;28;-906.8627,-955.8315;Float;False;2;2;0;FLOAT;0.6;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-626.4206,-1022.4;Float;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-1035.434,-214.4727;Half;False;Property;_ShadowBrightness;ShadowBrightness;3;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;21;-893.913,-310.4053;Float;False;2;0;FLOAT;0.5;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;16;-684.6466,-303.2246;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;63;204.6176,-950.9915;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;61;307.8575,-881.2293;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-217.3439,-474.9723;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;74;185.8557,-191.8851;Float;False;Property;_VertexOffset;VertexOffset;8;0;Create;True;0;0;False;0;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;69;164.065,-371.6368;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;62;398.8143,-570.3328;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;72;518.8557,-210.8851;Float;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;6;891.2418,-595.756;Float;False;True;2;Float;ASEMaterialInspector;0;0;CustomLighting;Cognition/BasicTile;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;0;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;False;True;False;False;False;False;False;False;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;-1;False;-1;-1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;0;0;False;0;0;0;False;-1;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;12;0;9;0
WireConnection;12;1;10;0
WireConnection;12;2;11;0
WireConnection;48;0;12;0
WireConnection;31;1;25;0
WireConnection;31;0;32;0
WireConnection;14;0;48;0
WireConnection;14;1;15;0
WireConnection;76;0;31;0
WireConnection;76;1;77;0
WireConnection;19;0;14;0
WireConnection;78;0;76;0
WireConnection;28;1;19;0
WireConnection;27;0;78;0
WireConnection;27;1;28;0
WireConnection;27;2;30;0
WireConnection;21;0;23;0
WireConnection;21;1;19;0
WireConnection;16;0;21;0
WireConnection;16;1;17;0
WireConnection;63;0;27;0
WireConnection;61;0;63;0
WireConnection;26;0;27;0
WireConnection;26;1;16;0
WireConnection;69;0;26;0
WireConnection;62;0;61;0
WireConnection;72;1;74;0
WireConnection;6;2;62;0
WireConnection;6;13;69;0
WireConnection;6;11;72;0
ASEEND*/
//CHKSM=0DE281F0C136E8923CC1D07C54F96F0F10EB6F2C