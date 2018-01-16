// Shader created with Shader Forge v1.37 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.37;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:2,rntp:3,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3022,x:33643,y:32745,varname:node_3022,prsc:2|emission-8245-OUT,clip-5249-OUT;n:type:ShaderForge.SFN_Tex2d,id:4296,x:32208,y:33005,ptovrint:False,ptlb:Spawn,ptin:_Spawn,varname:node_4296,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:cb89c18f7333d5a41877e7e0584433fd,ntxv:0,isnm:False|UVIN-3070-UVOUT;n:type:ShaderForge.SFN_Color,id:7904,x:31902,y:32400,ptovrint:False,ptlb:SpawnColor,ptin:_SpawnColor,varname:node_7904,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.7058823,c2:0.8782961,c3:1,c4:1;n:type:ShaderForge.SFN_TexCoord,id:8553,x:31797,y:33005,varname:node_8553,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Panner,id:3070,x:32020,y:33005,varname:node_3070,prsc:2,spu:0,spv:1|UVIN-8553-UVOUT,DIST-9318-OUT;n:type:ShaderForge.SFN_Slider,id:9318,x:31447,y:32897,ptovrint:False,ptlb:Glow_Position,ptin:_Glow_Position,varname:node_9318,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Tex2dAsset,id:8333,x:31789,y:33575,ptovrint:False,ptlb:Noise Texture,ptin:_NoiseTexture,varname:node_8333,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:28c7aad1372ff114b90d330f8a2dd938,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:5249,x:32854,y:33245,varname:node_5249,prsc:2|A-4296-R,B-6352-OUT;n:type:ShaderForge.SFN_Tex2d,id:8043,x:32222,y:33474,varname:node_8043,prsc:2,tex:28c7aad1372ff114b90d330f8a2dd938,ntxv:0,isnm:False|UVIN-2260-UVOUT,TEX-8333-TEX;n:type:ShaderForge.SFN_TexCoord,id:1164,x:31079,y:33316,varname:node_1164,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Multiply,id:9733,x:31588,y:33381,varname:node_9733,prsc:2|A-130-G,B-8199-OUT;n:type:ShaderForge.SFN_Vector1,id:8199,x:31266,y:33462,varname:node_8199,prsc:2,v1:0.3;n:type:ShaderForge.SFN_Power,id:392,x:32408,y:33474,varname:node_392,prsc:2|VAL-8043-R,EXP-6111-OUT;n:type:ShaderForge.SFN_Vector1,id:6111,x:32222,y:33608,varname:node_6111,prsc:2,v1:6;n:type:ShaderForge.SFN_Multiply,id:2213,x:32597,y:33474,varname:node_2213,prsc:2|A-392-OUT,B-6111-OUT;n:type:ShaderForge.SFN_Panner,id:2260,x:32222,y:33226,varname:node_2260,prsc:2,spu:0,spv:1|UVIN-7354-OUT,DIST-3293-OUT;n:type:ShaderForge.SFN_Multiply,id:3293,x:32052,y:33226,varname:node_3293,prsc:2|A-9318-OUT,B-6989-OUT;n:type:ShaderForge.SFN_Vector1,id:6989,x:31797,y:33244,varname:node_6989,prsc:2,v1:0.2;n:type:ShaderForge.SFN_Add,id:6352,x:32843,y:33430,varname:node_6352,prsc:2|A-2213-OUT,B-691-OUT;n:type:ShaderForge.SFN_Color,id:2550,x:31902,y:32575,ptovrint:False,ptlb:GlowColor,ptin:_GlowColor,varname:node_2550,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:1119,x:32429,y:32421,varname:node_1119,prsc:2|A-7904-RGB,B-5249-OUT;n:type:ShaderForge.SFN_Multiply,id:3230,x:32429,y:32577,varname:node_3230,prsc:2|A-2550-RGB,B-5249-OUT,C-7752-OUT;n:type:ShaderForge.SFN_Vector1,id:7752,x:31902,y:32727,varname:node_7752,prsc:2,v1:0.5;n:type:ShaderForge.SFN_Add,id:8245,x:33027,y:32566,varname:node_8245,prsc:2|A-1119-OUT,B-3230-OUT,C-9318-OUT;n:type:ShaderForge.SFN_Vector1,id:9036,x:32408,y:33416,varname:node_9036,prsc:2,v1:0.75;n:type:ShaderForge.SFN_Multiply,id:691,x:32597,y:33322,varname:node_691,prsc:2|A-9318-OUT,B-9036-OUT;n:type:ShaderForge.SFN_ComponentMask,id:130,x:31266,y:33316,varname:node_130,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-1164-UVOUT;n:type:ShaderForge.SFN_Append,id:7354,x:31809,y:33324,varname:node_7354,prsc:2|A-6659-OUT,B-9733-OUT;n:type:ShaderForge.SFN_Multiply,id:6659,x:31588,y:33244,varname:node_6659,prsc:2|A-303-OUT,B-130-R;n:type:ShaderForge.SFN_Vector1,id:303,x:31266,y:33265,varname:node_303,prsc:2,v1:2;proporder:4296-7904-9318-8333-2550;pass:END;sub:END;*/

Shader "Cognition/Spawn" {
    Properties {
        _Spawn ("Spawn", 2D) = "white" {}
        _SpawnColor ("SpawnColor", Color) = (0.7058823,0.8782961,1,1)
        _Glow_Position ("Glow_Position", Range(0, 1)) = 0
        _NoiseTexture ("Noise Texture", 2D) = "white" {}
        _GlowColor ("GlowColor", Color) = (1,1,1,1)
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "Queue"="AlphaTest"
            "RenderType"="TransparentCutout"
        }
        LOD 100
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Cull Off
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 
            #pragma target 3.0
            uniform sampler2D _Spawn; uniform float4 _Spawn_ST;
            uniform float4 _SpawnColor;
            uniform float _Glow_Position;
            uniform sampler2D _NoiseTexture; uniform float4 _NoiseTexture_ST;
            uniform float4 _GlowColor;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                float2 node_3070 = (i.uv0+_Glow_Position*float2(0,1));
                float4 _Spawn_var = tex2D(_Spawn,TRANSFORM_TEX(node_3070, _Spawn));
                float2 node_130 = i.uv0.rg;
                float2 node_2260 = (float2((2.0*node_130.r),(node_130.g*0.3))+(_Glow_Position*0.2)*float2(0,1));
                float4 node_8043 = tex2D(_NoiseTexture,TRANSFORM_TEX(node_2260, _NoiseTexture));
                float node_6111 = 6.0;
                float node_5249 = (_Spawn_var.r*((pow(node_8043.r,node_6111)*node_6111)+(_Glow_Position*0.75)));
                clip(node_5249 - 0.5);
////// Lighting:
////// Emissive:
                float3 emissive = ((_SpawnColor.rgb*node_5249)+(_GlowColor.rgb*node_5249*0.5)+_Glow_Position);
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 
            #pragma target 3.0
            uniform sampler2D _Spawn; uniform float4 _Spawn_ST;
            uniform float _Glow_Position;
            uniform sampler2D _NoiseTexture; uniform float4 _NoiseTexture_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                float2 node_3070 = (i.uv0+_Glow_Position*float2(0,1));
                float4 _Spawn_var = tex2D(_Spawn,TRANSFORM_TEX(node_3070, _Spawn));
                float2 node_130 = i.uv0.rg;
                float2 node_2260 = (float2((2.0*node_130.r),(node_130.g*0.3))+(_Glow_Position*0.2)*float2(0,1));
                float4 node_8043 = tex2D(_NoiseTexture,TRANSFORM_TEX(node_2260, _NoiseTexture));
                float node_6111 = 6.0;
                float node_5249 = (_Spawn_var.r*((pow(node_8043.r,node_6111)*node_6111)+(_Glow_Position*0.75)));
                clip(node_5249 - 0.5);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
