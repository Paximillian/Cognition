// Shader created with Shader Forge v1.37 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.37;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:9361,x:35554,y:32636,varname:node_9361,prsc:2|emission-2734-OUT,custl-5085-OUT,olwid-4707-OUT,olcol-2607-OUT;n:type:ShaderForge.SFN_NormalVector,id:9684,x:31607,y:33351,prsc:2,pt:True;n:type:ShaderForge.SFN_Tex2d,id:851,x:31225,y:32109,ptovrint:False,ptlb:Base Diffuse,ptin:_BaseDiffuse,varname:node_851,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:1941,x:32661,y:33177,cmnt:Diffuse Contribution,varname:node_1941,prsc:2|A-3201-OUT,B-5491-OUT;n:type:ShaderForge.SFN_Color,id:5927,x:31214,y:32746,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_5927,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Multiply,id:5085,x:33308,y:33162,cmnt:Attenuate and Color,varname:node_5085,prsc:2|A-1941-OUT,B-5722-RGB,C-7991-OUT;n:type:ShaderForge.SFN_AmbientLight,id:7528,x:32989,y:32555,varname:node_7528,prsc:2;n:type:ShaderForge.SFN_Multiply,id:2460,x:33220,y:32513,cmnt:Ambient Light,varname:node_2460,prsc:2|A-3201-OUT,B-7528-RGB;n:type:ShaderForge.SFN_Slider,id:1008,x:30347,y:33118,ptovrint:False,ptlb:Light Position X,ptin:_LightPositionX,varname:node_1008,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-1,cur:1,max:1;n:type:ShaderForge.SFN_Slider,id:6157,x:30347,y:33209,ptovrint:False,ptlb:Light Position Y,ptin:_LightPositionY,varname:_LightPosition_Y,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-1,cur:0,max:1;n:type:ShaderForge.SFN_Slider,id:5192,x:30347,y:33300,ptovrint:False,ptlb:Light Position Z,ptin:_LightPositionZ,varname:_LightPosition_Z,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-1,cur:1,max:1;n:type:ShaderForge.SFN_Append,id:7638,x:30739,y:33186,varname:node_7638,prsc:2|A-1008-OUT,B-6157-OUT,C-5192-OUT;n:type:ShaderForge.SFN_Normalize,id:6403,x:30918,y:33186,varname:node_6403,prsc:2|IN-7638-OUT;n:type:ShaderForge.SFN_Slider,id:7991,x:32649,y:33622,ptovrint:False,ptlb:Light Distance,ptin:_LightDistance,varname:node_7991,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Color,id:5722,x:32738,y:33379,ptovrint:False,ptlb:Light Color,ptin:_LightColor,varname:node_5722,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Set,id:902,x:31126,y:33186,varname:LightDirection,prsc:2|IN-6403-OUT;n:type:ShaderForge.SFN_Get,id:6721,x:31586,y:33200,varname:node_6721,prsc:2|IN-902-OUT;n:type:ShaderForge.SFN_Vector1,id:9923,x:31956,y:33353,varname:node_9923,prsc:2,v1:8;n:type:ShaderForge.SFN_Power,id:5491,x:32355,y:33202,varname:node_5491,prsc:2|VAL-9418-OUT,EXP-8756-OUT;n:type:ShaderForge.SFN_Exp,id:8756,x:32355,y:33340,varname:node_8756,prsc:2,et:0|IN-2804-OUT;n:type:ShaderForge.SFN_Slider,id:9307,x:32005,y:33523,ptovrint:False,ptlb:Light Contrast,ptin:_LightContrast,varname:node_9307,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.1,max:1;n:type:ShaderForge.SFN_RemapRange,id:2804,x:32162,y:33340,varname:node_2804,prsc:2,frmn:0,frmx:1,tomn:0.1,tomx:7|IN-9307-OUT;n:type:ShaderForge.SFN_Slider,id:9916,x:34066,y:33409,ptovrint:False,ptlb:Outline Size,ptin:_OutlineSize,varname:node_9916,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Multiply,id:4707,x:34400,y:33409,varname:node_4707,prsc:2|A-9916-OUT,B-2986-OUT;n:type:ShaderForge.SFN_Vector1,id:2986,x:34201,y:33596,varname:node_2986,prsc:2,v1:0.05;n:type:ShaderForge.SFN_Dot,id:5197,x:31796,y:33200,varname:node_5197,prsc:2,dt:1|A-6721-OUT,B-9684-OUT;n:type:ShaderForge.SFN_Posterize,id:9418,x:32162,y:33202,varname:node_9418,prsc:2|IN-9853-OUT,STPS-9923-OUT;n:type:ShaderForge.SFN_Color,id:8246,x:34066,y:33160,ptovrint:False,ptlb:Outline Color,ptin:_OutlineColor,varname:node_8246,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0,c3:0,c4:1;n:type:ShaderForge.SFN_Tex2d,id:9510,x:31225,y:31923,ptovrint:False,ptlb:Damaged Diffuse,ptin:_DamagedDiffuse,varname:node_9510,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Lerp,id:2345,x:31752,y:32090,varname:node_2345,prsc:2|A-9510-RGB,B-851-RGB,T-2519-OUT;n:type:ShaderForge.SFN_Tex2d,id:3023,x:30459,y:32303,ptovrint:False,ptlb:Damage Pattern,ptin:_DamagePattern,varname:node_3023,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:28c7aad1372ff114b90d330f8a2dd938,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Power,id:9987,x:30863,y:32303,varname:node_9987,prsc:2|VAL-4459-OUT,EXP-1300-OUT;n:type:ShaderForge.SFN_Vector1,id:1300,x:30680,y:32423,varname:node_1300,prsc:2,v1:10;n:type:ShaderForge.SFN_Add,id:4459,x:30680,y:32303,varname:node_4459,prsc:2|A-3023-RGB,B-9287-OUT;n:type:ShaderForge.SFN_OneMinus,id:2519,x:31407,y:32303,varname:node_2519,prsc:2|IN-7847-OUT;n:type:ShaderForge.SFN_Slider,id:9287,x:30459,y:32545,ptovrint:False,ptlb:Damage,ptin:_Damage,varname:node_9287,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Multiply,id:2063,x:31039,y:32303,varname:node_2063,prsc:2|A-9987-OUT,B-1300-OUT,C-9287-OUT;n:type:ShaderForge.SFN_Clamp01,id:7847,x:31225,y:32303,varname:node_7847,prsc:2|IN-2063-OUT;n:type:ShaderForge.SFN_Add,id:1154,x:34349,y:33108,varname:node_1154,prsc:2|A-8246-RGB,B-1063-OUT;n:type:ShaderForge.SFN_Add,id:97,x:34626,y:32598,varname:node_97,prsc:2|A-2460-OUT,B-1063-OUT;n:type:ShaderForge.SFN_Clamp01,id:2734,x:34825,y:32598,varname:node_2734,prsc:2|IN-97-OUT;n:type:ShaderForge.SFN_Clamp01,id:2607,x:34552,y:33108,varname:node_2607,prsc:2|IN-1154-OUT;n:type:ShaderForge.SFN_Slider,id:4475,x:33755,y:32885,ptovrint:False,ptlb:GlowPower,ptin:_GlowPower,varname:node_4475,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Multiply,id:1063,x:34103,y:32841,varname:node_1063,prsc:2|A-256-RGB,B-4475-OUT;n:type:ShaderForge.SFN_Color,id:256,x:33912,y:32716,ptovrint:False,ptlb:GlowColor,ptin:_GlowColor,varname:node_256,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Add,id:9853,x:31974,y:33087,varname:node_9853,prsc:2|A-8325-OUT,B-5197-OUT;n:type:ShaderForge.SFN_Vector1,id:8325,x:31773,y:33082,varname:node_8325,prsc:2,v1:0.2;n:type:ShaderForge.SFN_Tex2d,id:9617,x:31214,y:32556,ptovrint:False,ptlb:ColorMask,ptin:_ColorMask,varname:node_9617,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:2,isnm:False;n:type:ShaderForge.SFN_ChannelBlend,id:3201,x:32129,y:32494,varname:node_3201,prsc:2,chbt:1|M-9617-R,R-5927-RGB,BTM-2345-OUT;proporder:1008-6157-5192-7991-9307-5722-851-9510-5927-9916-8246-3023-9287-4475-256-9617;pass:END;sub:END;*/

Shader "Cognition/BasicMetalCog" {
    Properties {
        _LightPositionX ("Light Position X", Range(-1, 1)) = 1
        _LightPositionY ("Light Position Y", Range(-1, 1)) = 0
        _LightPositionZ ("Light Position Z", Range(-1, 1)) = 1
        _LightDistance ("Light Distance", Range(0, 1)) = 1
        _LightContrast ("Light Contrast", Range(0, 1)) = 0.1
        _LightColor ("Light Color", Color) = (1,1,1,1)
        _BaseDiffuse ("Base Diffuse", 2D) = "white" {}
        _DamagedDiffuse ("Damaged Diffuse", 2D) = "white" {}
        _Color ("Color", Color) = (0.5,0.5,0.5,1)
        _OutlineSize ("Outline Size", Range(0, 1)) = 0
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _DamagePattern ("Damage Pattern", 2D) = "white" {}
        _Damage ("Damage", Range(0, 1)) = 0
        _GlowPower ("GlowPower", Range(0, 1)) = 0
        _GlowColor ("GlowColor", Color) = (0.5,0.5,0.5,1)
        _ColorMask ("ColorMask", 2D) = "black" {}
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "Outline"
            Tags {
            }
            Cull Front
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 
            #pragma target 3.0
            uniform float _OutlineSize;
            uniform float4 _OutlineColor;
            uniform float _GlowPower;
            uniform float4 _GlowColor;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                UNITY_FOG_COORDS(0)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.pos = UnityObjectToClipPos( float4(v.vertex.xyz + v.normal*(_OutlineSize*0.05),1) );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float3 node_1063 = (_GlowColor.rgb*_GlowPower);
                return fixed4(saturate((_OutlineColor.rgb+node_1063)),0);
            }
            ENDCG
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 
            #pragma target 3.0
            uniform sampler2D _BaseDiffuse; uniform float4 _BaseDiffuse_ST;
            uniform float4 _Color;
            uniform float _LightPositionX;
            uniform float _LightPositionY;
            uniform float _LightPositionZ;
            uniform float _LightDistance;
            uniform float4 _LightColor;
            uniform float _LightContrast;
            uniform sampler2D _DamagedDiffuse; uniform float4 _DamagedDiffuse_ST;
            uniform sampler2D _DamagePattern; uniform float4 _DamagePattern_ST;
            uniform float _Damage;
            uniform float _GlowPower;
            uniform float4 _GlowColor;
            uniform sampler2D _ColorMask; uniform float4 _ColorMask_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 normalDirection = i.normalDir;
////// Lighting:
////// Emissive:
                float4 _ColorMask_var = tex2D(_ColorMask,TRANSFORM_TEX(i.uv0, _ColorMask));
                float4 _DamagedDiffuse_var = tex2D(_DamagedDiffuse,TRANSFORM_TEX(i.uv0, _DamagedDiffuse));
                float4 _BaseDiffuse_var = tex2D(_BaseDiffuse,TRANSFORM_TEX(i.uv0, _BaseDiffuse));
                float4 _DamagePattern_var = tex2D(_DamagePattern,TRANSFORM_TEX(i.uv0, _DamagePattern));
                float node_1300 = 10.0;
                float3 node_2345 = lerp(_DamagedDiffuse_var.rgb,_BaseDiffuse_var.rgb,(1.0 - saturate((pow((_DamagePattern_var.rgb+_Damage),node_1300)*node_1300*_Damage))));
                float3 node_3201 = (lerp( node_2345, _Color.rgb, _ColorMask_var.r.r ));
                float3 node_1063 = (_GlowColor.rgb*_GlowPower);
                float3 emissive = saturate(((node_3201*UNITY_LIGHTMODEL_AMBIENT.rgb)+node_1063));
                float3 LightDirection = normalize(float3(_LightPositionX,_LightPositionY,_LightPositionZ));
                float node_9923 = 8.0;
                float3 finalColor = emissive + ((node_3201*pow(floor((0.2+max(0,dot(LightDirection,normalDirection))) * node_9923) / (node_9923 - 1),exp((_LightContrast*6.9+0.1))))*_LightColor.rgb*_LightDistance);
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
