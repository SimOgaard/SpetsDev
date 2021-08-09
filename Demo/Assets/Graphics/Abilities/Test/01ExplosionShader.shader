Shader "Custom/01ExplosionShader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

		_ColorSmoke ("Color Smoke", Color) = (0,0,0,0)
		_VortexSpeed ("Vortex Speed", Float) = 2

		_AlphaClip ("Alpha Clip", Range(0, 1)) = 0.5
		_AlphaClipPower ("Alpha Clip Power", Float) = 4
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

			float4 _ColorSmoke;
			float _VortexSpeed;
			float _AlphaClip;
			float _AlphaClipPower;
			
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
				float2 uv = i.uv + float2(_Time.x * _VortexSpeed, 0);
				fixed4 texture_col = tex2D(_MainTex, uv);

				fixed4 smoke = _ColorSmoke * texture_col.r;

				float alpha = saturate(texture_col.b * _AlphaClipPower) - _AlphaClip;
				clip(alpha);

				return smoke;
            }
            ENDCG
        }
    }
}
