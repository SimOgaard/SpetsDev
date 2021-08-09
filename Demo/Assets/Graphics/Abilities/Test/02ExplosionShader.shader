Shader "Custom/02ExplosionShader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

		[HDR] _ColorFire ("Color Fire", Color) = (0,0,0)
		_ColorSmoke ("Color Smoke", Color) = (0,0,0,0)
		_VortexSpeed ("Vortex Speed", Float) = 2
		_SmokeFireRatio ("Smoke Fire Ratio", Range(0, 1)) = 0

		_AlphaClip ("Alpha Clip", Range(0, 1)) = 0.5
		_AlphaClipPower ("Alpha Clip Power", Float) = 4

		_ColorFire1 ("Color Fire1", Color) = (0,0,0,0)
		_ColorFire2 ("Color Fire2", Color) = (0,0,0,0)
		_ColorFire3 ("Color Fire3", Color) = (0,0,0,0)

		_Threshold1 ("Threshold1", Range(0,1)) = 0.2
		_Threshold2 ("Threshold2", Range(0,1)) = 0.8
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

			float4 _ColorFire1;
			float4 _ColorFire2;
			float4 _ColorFire3;

			float _Threshold1;
			float _Threshold2;

			struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed2 uv = i.uv + float2(_Time.x * _VortexSpeed, 0);
				fixed4 texture_col = tex2D(_MainTex, uv);

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

				float4 final_col = lerp(fire, smoke, _SmokeFireRatio);

				float alpha = saturate(texture_col.b * _AlphaClipPower) - _AlphaClip;
				clip(alpha);

				return final_col;
            }
            ENDCG
        }
    }
}