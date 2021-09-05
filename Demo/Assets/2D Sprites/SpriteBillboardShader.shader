Shader "Unlit/SpriteBillboardShader"
{
	Properties
	{
	   _MainTex("Texture Image", 2D) = "white" {}
	}
	SubShader
	{
		Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "DisableBatching" = "True" }
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM

			#pragma vertex vert  
			#pragma fragment frag

			uniform sampler2D _MainTex;

			struct vertexInput
			{
				float4 vertex : POSITION;
				float4 tex : TEXCOORD0;
			};
			struct vertexOutput
			{
				float4 pos : SV_POSITION;
				float4 tex : TEXCOORD0;
			};

			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;

				//copy them so we can change them (demonstration purpos only)
				float4x4 m = UNITY_MATRIX_M;
				float4x4 v = UNITY_MATRIX_V;
				float4x4 p = UNITY_MATRIX_P;
    
				//break out the axis
				float3 right = normalize(v._m00_m01_m02);
				float3 up = float3(0,1,0);
				float3 forward = normalize(v._m20_m21_m22);
				//get the rotation parts of the matrix
				float4x4 rotationMatrix = float4x4(right, 0,
    				up, 0,
    				forward, 0,
    				0, 0, 0, 1);
    
				//the inverse of a rotation matrix happens to always be the transpose
				float4x4 rotationMatrixInverse = transpose(rotationMatrix);
    
				//apply the rotationMatrixInverse, model, view and projection matrix
				float4 pos = input.vertex;
				pos = mul(rotationMatrixInverse, pos);
				pos = mul(m, pos);
				pos = mul(v, pos);
				pos = mul(p, pos);

				output.pos = pos;
				output.tex = input.tex;

				return output;
			}

			float4 frag(vertexOutput input) : COLOR
			{
				return tex2D(_MainTex, input.tex.xy);
			}

			ENDCG
		}
	}
}
