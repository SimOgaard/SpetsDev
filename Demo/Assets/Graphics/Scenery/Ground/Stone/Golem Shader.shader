Shader "Custom/Golem Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _ColorLight ("ColorLight", Color) = (0.7607843, 0.8666667, 0.5960785, 1)
        _ColorMedium ("ColorMedium", Color) = (0.4666667, 0.654902, 0.4196078, 1)
        _ColorDark ("ColorDark", Color) = (0.1215686, 0.3411765, 0.3058824, 1)

        _FirstThreshold ("FirstThreshold", Range(0,1)) = 0.3
        _SecondThreshold ("SecondThreshold", Range(0,1)) = 0.5

		_Distort("Distort", 2D) = "white" {}
		_Radius("Radius", Range(0, 1)) = 0.75
		_DistortionAmount("Distortion Amount", Float) = 0.1
    }

    SubShader
    {
        Pass
        {
			Tags {
				"RenderType"= "Opaque"
				"LightMode" = "ForwardBase"
				"PassFlags" = "OnlyDirectional"
			}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			#include "Lighting.cginc"

			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #include "AutoLight.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
				SHADOW_COORDS(1)
				fixed3 diff : COLOR0;
                fixed3 ambient : COLOR1;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

			fixed4 _Color;
	        fixed4 _ColorLight;
			fixed4 _ColorMedium;
			fixed4 _ColorDark;

			float _FirstThreshold;
			float _SecondThreshold;

			sampler2D _Distort;
			float _DistortionAmount;
			float _Radius;

			float circle(in float2 _st, in float _radius){
				float2 dist = _st-float2(0.5, 0.5);
				return 1.-smoothstep(_radius-(_radius*0.01), _radius+(_radius*0.01), dot(dist,dist)*4.0);
			}

            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0.rgb;
                o.ambient = ShadeSH9(half4(worldNormal,1));
                TRANSFER_SHADOW(o)
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float2 distortSample = (tex2D(_Distort, i.uv).xy * 2 - 1) * _DistortionAmount;
				float value = circle(i.uv + distortSample, _Radius);

				fixed shadow = SHADOW_ATTENUATION(i);
                fixed3 lighting = i.diff * shadow + i.ambient;

				if (value < 0.5){
					float4 light_color = _LightColor0.rgba * lighting.r;
					return _Color * light_color;
				}

				float4 col = _ColorLight;
				if (lighting.x < _FirstThreshold)
				{
					col = _ColorDark;
				}
				else if (lighting.x < _SecondThreshold)
				{
					col = _ColorMedium;				
				}

                return col;
            }
            ENDCG
        }
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
