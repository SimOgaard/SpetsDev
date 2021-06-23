Shader "Custom/Pixel Shader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float2 _PixelDivision;

			float2 getPos(float2 position, float2 resolution)
			{
				position *= resolution;
				position = floor(position);
				position /= resolution;

				return position;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float2 pos = getPos(i.uv, _PixelDivision);

				return tex2D(_MainTex, pos);
			}
			ENDCG
		}
	}
}