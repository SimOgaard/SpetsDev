Shader "Custom/04ExplosionShader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

		_ColorSmoke ("Color Smoke", Color) = (0,0,0,0)
		_VortexSpeed ("Vortex Speed", Float) = 2
		_SmokeFireRatio ("Smoke Fire Ratio", Range(0, 1)) = 0
		_SmokeFireBlend ("Smoke Fire Blend", Range(0, 1)) = 0

		_AlphaClip ("Alpha Clip", Range(0, 1)) = 0.5
		_AlphaClipPower ("Alpha Clip Power", Float) = 4

		_ColorFire1 ("Color Fire1", Color) = (0,0,0,0)
		_ColorFire2 ("Color Fire2", Color) = (0,0,0,0)
		_ColorFire3 ("Color Fire3", Color) = (0,0,0,0)

		_Threshold1 ("Threshold1", Range(0,1)) = 0.2
		_Threshold2 ("Threshold2", Range(0,1)) = 0.8

        [NoScaleOffset] _DistortionNormalMap ("Distortion Normal Map", 2D) = "white" {}
		_JiggleStrength ("Jiggle Strength", Float) = 0.05
		_JiggleSpeed ("Jiggle Speed", Float) = 0.1
    }
    SubShader
    {
		Tags{ "RenderType" = "TransparentCutout" "Queue" = "AlphaTest"}
		Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float4 _ColorFire;
			float4 _ColorSmoke;
			float _VortexSpeed;
			float _AlphaClip;
			float _AlphaClipPower;
			float _SmokeFireRatio;
			float _SmokeFireBlend;

			float4 _ColorFire1;
			float4 _ColorFire2;
			float4 _ColorFire3;

			float _Threshold1;
			float _Threshold2;

			sampler2D _DistortionNormalMap;
			float _JiggleStrength;
			float _JiggleSpeed;

			struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float2 test : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float2 test : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.test = v.test;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed2 distort = tex2D(_DistortionNormalMap, i.uv + float2(_Time.x * _JiggleSpeed, _Time.x * _JiggleSpeed)).rg * _JiggleStrength;
				fixed2 uv = i.uv + distort + float2(_Time.x * _VortexSpeed, 0);
				fixed4 texture_col = tex2D(_MainTex, uv);
				
				float alpha = saturate(texture_col.b * _AlphaClipPower) - i.test.y;
				clip(alpha);

				fixed4 smoke = _ColorSmoke * texture_col.r;				

				float4 fire = _ColorFire3;
				if (texture_col.g < _Threshold1)
				{
					fire = _ColorFire1;
				}
				else if (texture_col.g < _Threshold2)
				{
					fire = _ColorFire2;
				}

				//float4 ratio_col = texture_col.g < i.test.x ? smoke : fire;
				//float4 blended_col = lerp(ratio_col, smoke, i.test.x);
				float4 blended_col = lerp(fire, smoke, i.test.x);

				return blended_col;
            }
            ENDCG
        }
    }
}