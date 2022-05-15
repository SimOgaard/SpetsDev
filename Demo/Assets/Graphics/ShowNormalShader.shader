Shader "Custom/ShowNormalShader"
{
    Properties
    {
        _ColorMultiplier ("Color Multiplier", float) = 1
    }
    SubShader{
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _ColorMultiplier;

            struct v2f {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color.xyz = (v.normal * 0.5 + 0.5) * _ColorMultiplier;
                o.color.w = 1.0;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                return i.color;
            }
            ENDCG
        }
    }
}
