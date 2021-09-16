Shader "Custom/CooldownPixelated"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_PixelAmount ("Pixel Amount", float) = 17
		_Angle ("_Angle", Range(0.0, 1.0)) = 0.25
		_Arc1 ("_Arc Point 1", Range(0.0, 1.0)) = 0
		_Arc2 ("_Arc Point 2", Range(0.0, 1.0)) = 1
    }

    SubShader
	{
		Pass
		{
			Tags {
				"Queue"="AlphaTest"
				"IgnoreProjector"="True"
				"RenderType"="TransparentCutout"
			}

			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			float _PixelAmount;
			float _Angle;
			float _Arc1;
			float _Arc2;

			float2 getPos(float2 position, float2 resolution)
			{
				position *= resolution;
				position = ceil(position);
				position /= resolution;

				return position;
			}

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
				if (_Arc1 == 0) discard;

				float2 resolution = _PixelAmount.xx;
				float2 position = getPos(i.texcoord, resolution);
                float4 col = tex2D(_MainTex, position) * _Color;

				float startAngle = (_Angle - (1-_Arc1)) * 360;
                float endAngle = (_Angle + (1-_Arc2)) * 360;

                // check offsets
                float offset0 = clamp(0, 360, startAngle + 360);
                float offset360 = clamp(0, 360, endAngle - 360);

                // convert uv to atan coordinates
                float2 atan2Coord = float2(lerp(-1, 1, position.x), lerp(-1, 1, position.y));
                float atanAngle = atan2(atan2Coord.y, atan2Coord.x) * 57.3; // angle in degrees

                // convert angle to 360 system
                if(atanAngle < 0) atanAngle = 360 + atanAngle;

                if(atanAngle >= startAngle && atanAngle <= endAngle) discard;
                if(atanAngle <= offset360) discard;
                if(atanAngle >= offset0) discard;

				return col;
            }
	        ENDCG
		}
    }
}