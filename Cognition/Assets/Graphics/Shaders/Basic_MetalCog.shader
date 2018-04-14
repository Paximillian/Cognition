// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Cognition/BasicMetalCog"
{
	Properties
	{
		_Light_X("Light_X", Range( -1 , 1)) = 1
		_Light_Y("Light_Y", Range( -1 , 1)) = 1
		_Light_Z("Light_Z", Range( -1 , 1)) = 1
		_ShadowBrightness("ShadowBrightness", Range( 0 , 1)) = 0
		_ShadowPosition("ShadowPosition", Range( 0 , 1)) = 0.5
		_HighlightIntensity("HighlightIntensity", Range( 0 , 1)) = 0
		_MainTex("MainTex", 2D) = "white" {}
		_ContrastFix("ContrastFix", Range( 0.1 , 2)) = 1
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
			float3 viewDir;
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
		uniform half _Light_X;
		uniform half _Light_Y;
		uniform half _Light_Z;
		uniform half _ShadowPosition;
		uniform half _ShadowBrightness;
		uniform half _HighlightIntensity;


		float4 CalculateContrast( float contrastValue, float4 colorTarget )
		{
			float t = 0.5 * ( 1.0 - contrastValue );
			return mul( float4x4( contrastValue,0,0,t, 0,contrastValue,0,t, 0,0,contrastValue,t, 0,0,0,1 ), colorTarget );
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
			float4 temp_output_27_0 = ( CalculateContrast(_ContrastFix,tex2D( _MainTex, uv_MainTex )) * ( 0.6 + temp_output_19_0 ) * UNITY_LIGHTMODEL_AMBIENT );
			float temp_output_21_0 = step( _ShadowPosition , temp_output_19_0 );
			float3 normalizeResult47 = normalize( ( normalizeResult48 + i.viewDir ) );
			float dotResult35 = dot( ase_worldNormal , normalizeResult47 );
			float temp_output_53_0 = saturate( pow( saturate( dotResult35 ) , 20 ) );
			c.rgb = saturate( ( ( temp_output_27_0 * ( temp_output_21_0 + _ShadowBrightness ) ) + ( temp_output_21_0 * ( ( step( 0.5 , temp_output_53_0 ) * 0.2 ) + ( temp_output_53_0 * 0.2 ) ) * _HighlightIntensity ) ) ).rgb;
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
			float4 temp_output_27_0 = ( CalculateContrast(_ContrastFix,tex2D( _MainTex, uv_MainTex )) * ( 0.6 + temp_output_19_0 ) * UNITY_LIGHTMODEL_AMBIENT );
			o.Emission = temp_output_27_0.rgb;
		}

		ENDCG
		CGPROGRAM
		#pragma only_renderers d3d9 d3d11 glcore gles gles3 d3d11_9x 
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows 

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
				surfIN.viewDir = worldViewDir;
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
110;123;1332;665;1542.667;-701.678;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;13;-3236.536,-354.438;Float;False;752.0415;340.3575;Comment;5;12;10;11;9;48;Light Direction;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-3186.535,-304.438;Half;False;Property;_Light_X;Light_X;0;0;Create;True;0;0;False;0;1;1;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-3186.536,-220.7703;Half;False;Property;_Light_Y;Light_Y;1;0;Create;True;0;0;False;0;1;1;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-3184.189,-138.5378;Half;False;Property;_Light_Z;Light_Z;2;0;Create;True;0;0;False;0;1;-1;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;12;-2834.343,-259.8316;Float;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;65;-1838.001,469.582;Float;False;1365.874;601.5964;Comment;14;71;70;67;56;68;54;53;37;59;58;52;35;47;46;Specularity;1,1,1,1;0;0
Node;AmplifyShaderEditor.NormalizeNode;48;-2685.48,-252.5004;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;45;-2613.968,510.1252;Float;False;World;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;46;-1553.136,534.9041;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;15;-2610.545,348.6263;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalizeNode;47;-1402.936,542.9961;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;35;-1217.671,519.5819;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;52;-1069.116,522.114;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;58;-959.3431,676.1598;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;59;-1788.001,713.0169;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;37;-1771.249,800.0483;Float;False;2;0;FLOAT;0;False;1;FLOAT;20;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;53;-1592.272,800.1658;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;57;-1413.836,-425.3054;Float;False;970.7277;362.0052;Comment;6;16;21;17;23;19;14;Basic Shading;1,1,1,1;0;0
Node;AmplifyShaderEditor.StepOpNode;54;-1422.869,778.548;Float;False;2;0;FLOAT;0.5;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;14;-1363.836,-297.4223;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;19;-1198.838,-287.3599;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;56;-1273.133,778.372;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-1204.22,-375.3054;Half;False;Property;_ShadowPosition;ShadowPosition;4;0;Create;True;0;0;False;0;0.5;0.097;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;64;-1445.692,-1417.616;Float;False;988.2718;594.7847;Comment;6;32;25;31;27;30;28;Basic Texture;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;-1421.261,878.5747;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;71;-1042.134,992.1796;Half;False;Property;_HighlightIntensity;HighlightIntensity;5;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;67;-1047.365,781.1819;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;21;-893.913,-310.4053;Float;False;2;0;FLOAT;0.5;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;25;-1395.692,-1367.616;Float;True;Property;_MainTex;MainTex;6;0;Create;True;0;0;False;0;e2c183f00b9d3644295f1a7cb99ba7e9;e2c183f00b9d3644295f1a7cb99ba7e9;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;32;-1382.804,-1141.257;Float;False;Property;_ContrastFix;ContrastFix;7;0;Create;True;0;0;False;0;1;1.394;0.1;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;70;-681.8578,776.9415;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FogAndAmbientColorsNode;30;-1042.339,-1035.73;Float;False;UNITY_LIGHTMODEL_AMBIENT;0;1;COLOR;0
Node;AmplifyShaderEditor.SimpleContrastOpNode;31;-1020.198,-1275.916;Float;True;2;1;COLOR;0,0,0,0;False;0;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;28;-906.8627,-955.8315;Float;False;2;2;0;FLOAT;0.6;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-1035.434,-214.4727;Half;False;Property;_ShadowBrightness;ShadowBrightness;3;0;Create;True;0;0;False;0;0;0.607;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;16;-684.6466,-303.2246;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;66;-211.3201,798.3367;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-626.4206,-1022.4;Float;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-273.2062,-536.8198;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;63;635.5546,-972.9372;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;60;-105.4069,734.7352;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;61;738.7946,-903.175;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;36;156.1423,-19.1784;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;69;379.5335,-14.5175;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;62;883.3161,-252.8285;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;6;1017,-278.6185;Float;False;True;2;Float;ASEMaterialInspector;0;0;CustomLighting;Cognition/BasicMetalCog;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;0;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;False;True;False;False;False;False;False;False;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;-1;False;-1;-1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;0;0;False;0;0;0;False;-1;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;12;0;9;0
WireConnection;12;1;10;0
WireConnection;12;2;11;0
WireConnection;48;0;12;0
WireConnection;46;0;48;0
WireConnection;46;1;45;0
WireConnection;47;0;46;0
WireConnection;35;0;15;0
WireConnection;35;1;47;0
WireConnection;52;0;35;0
WireConnection;58;0;52;0
WireConnection;59;0;58;0
WireConnection;37;0;59;0
WireConnection;53;0;37;0
WireConnection;54;1;53;0
WireConnection;14;0;48;0
WireConnection;14;1;15;0
WireConnection;19;0;14;0
WireConnection;56;0;54;0
WireConnection;68;0;53;0
WireConnection;67;0;56;0
WireConnection;67;1;68;0
WireConnection;21;0;23;0
WireConnection;21;1;19;0
WireConnection;70;0;21;0
WireConnection;70;1;67;0
WireConnection;70;2;71;0
WireConnection;31;1;25;0
WireConnection;31;0;32;0
WireConnection;28;1;19;0
WireConnection;16;0;21;0
WireConnection;16;1;17;0
WireConnection;66;0;70;0
WireConnection;27;0;31;0
WireConnection;27;1;28;0
WireConnection;27;2;30;0
WireConnection;26;0;27;0
WireConnection;26;1;16;0
WireConnection;63;0;27;0
WireConnection;60;0;66;0
WireConnection;61;0;63;0
WireConnection;36;0;26;0
WireConnection;36;1;60;0
WireConnection;69;0;36;0
WireConnection;62;0;61;0
WireConnection;6;2;62;0
WireConnection;6;13;69;0
ASEEND*/
//CHKSM=630AFE311168CCC93FCCBACA4A15B16F4A39E6FB