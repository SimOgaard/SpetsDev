Shader "Custom/per vertex shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
 
    CGINCLUDE
 
    #include "UnityCG.cginc"
    #include "Lighting.cginc"
 
    #include "HLSLSupport.cginc"
    #include "UnityShadowLibrary.cginc"

	#pragma target 3.0
 
    #if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
        #define UNITY_SAMPLE_SCREEN_SHADOW(tex, uv) UNITY_SAMPLE_TEX2DARRAY_LOD( tex, float3((uv).x/(uv).w, (uv).y/(uv).w, (float)unity_StereoEyeIndex), 0 ).r
    #else
        #define UNITY_SAMPLE_SCREEN_SHADOW(tex, uv) tex2Dlod( tex, float4 (uv.xy / uv.w, 0, 0) ).r
    #endif
 
    #include "AutoLight.cginc"
     
    struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
        float3 normal : NORMAL;
    };
 
    struct v2f
    {
        float4 pos : SV_POSITION;
		float3 worldPos : TEXCOORD2;
		float attenuation : TEXCOORD3;
        nointerpolation float3 lighting : TEXCOORD0;
        SHADOW_COORDS(1)
    };
 
  	sampler2D _LightTexture0;
	float4x4 unity_WorldToLight;

    v2f vert (appdata v)
    {
        v2f o;
        o.pos = UnityObjectToClipPos (v.vertex);
 
        o.worldPos = mul (unity_ObjectToWorld, float4 (v.vertex.xyz, 1)).xyz;
		
		float2 uvCookie = mul(unity_WorldToLight, float4(o.worldPos, 1)).xy;
		o.attenuation = tex2Dlod(_LightTexture0, float4(uvCookie,0,0)).w;
		
		float3 worldNorm = UnityObjectToWorldNormal (v.normal);
 
        TRANSFER_SHADOW(o);
        fixed shadow = SHADOW_ATTENUATION (o);
 
        half diff = smoothstep (0, 0.5, dot (worldNorm, _WorldSpaceLightPos0.xyz));
        o.lighting = _LightColor0.rgb * diff * shadow + ShadeSH9 (half4 (worldNorm, 1));
 
        return o;
    }
 
    half4 frag (v2f i) : SV_Target
    {
        return half4 (i.lighting * i.attenuation , 1);
    }
 
    ENDCG
 
    SubShader
    {
        LOD 100
 
        Pass
        {
            Tags {
				"RenderType" = "Opaque"
				"LightMode" = "ForwardAdd"
				"PassFlags" = "OnlyDirectional"
			}
 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            ENDCG
        }
 
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}