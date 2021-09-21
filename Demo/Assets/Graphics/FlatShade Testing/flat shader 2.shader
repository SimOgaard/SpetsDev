Shader "Unlit/per vertex shader 2"
 {
     Properties
     {
         _MainTex ("Texture", 2D) = "white" {}
     }
 
     SubShader
     {
         Pass
         {
             Tags 
             {
                 "RenderType" = "Opaque"
                 "LightMode" = "ForwardBase"
                 "PassFlags" = "OnlyDirectional"
             }
 
             CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
 
             #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
 
             #include "UnityCG.cginc"
             #include "Lighting.cginc"
             #include "AutoLight.cginc"
 
             struct v2f
             {
                 float4 pos : SV_POSITION;
                 half3 diff : TEXCOORD0;
                 half3 ambient : TEXCOORD1;
                 float3 worldPos : TEXCOORD2;
             };
 
             sampler2D _MainTex;
             float4 _MainTex_ST;
 
             // Declare the shadowmap
             UNITY_DECLARE_SHADOWMAP(_DirectionalShadowmap);
 
             sampler2D _LightTexture0;
             float4x4 unity_WorldToLight;
 
             v2f vert (appdata_base v)
             {
                 v2f o;
                 o.pos = UnityObjectToClipPos (v.vertex);
                 o.worldPos = mul (unity_ObjectToWorld, v.vertex);
                 half3 worldNormal = UnityObjectToWorldNormal (v.normal);
                 o.ambient = ShadeSH9 (half4 (worldNormal, 1));
 
                 // Support cascaded shadows
                 float4 shadowCoords0 = mul (unity_WorldToShadow[0], float4 (o.worldPos, 1));
                 float4 shadowCoords1 = mul (unity_WorldToShadow[1], float4 (o.worldPos, 1));
                 float4 shadowCoords2 = mul (unity_WorldToShadow[2], float4 (o.worldPos, 1));
                 float4 shadowCoords3 = mul (unity_WorldToShadow[3], float4 (o.worldPos, 1));
 
                 // Find which cascaded shadow coords to use based on our distance to the camera
                 float dist = distance (o.worldPos, _WorldSpaceCameraPos.xyz);
                 float4 zNear = dist >= _LightSplitsNear;
                 float4 zFar = dist < _LightSplitsFar;
                 float4 weights = zNear * zFar;
                 float4 shadowCoords = shadowCoords0 * weights.x + shadowCoords1 * weights.y + shadowCoords2 * weights.z + shadowCoords3 * weights.w;
 
                 // Sample the shadowmap
                 half shadow = UNITY_SAMPLE_SHADOW (_DirectionalShadowmap, shadowCoords);
 
                 half nl = max (0, dot (worldNormal, _WorldSpaceLightPos0.xyz));
                 o.diff = (nl * shadow) * _LightColor0.rgb;
                 return o;
             }
 
             half4 frag (v2f i) : SV_Target
             {
                 float2 uvCookie = mul (unity_WorldToLight, float4 (i.worldPos, 1)).xy;
                 float attenuation = tex2D (_LightTexture0, uvCookie).w;
                 half3 lighting = i.diff * attenuation + i.ambient;
 
                 return float4 (lighting, 1);
             }
             ENDCG
         }
         UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
     }
 }
