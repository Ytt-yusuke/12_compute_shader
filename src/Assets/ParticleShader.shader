Shader "Unlit/ParticleShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"

            // 構造体定義
            struct Particle
            {
                float3 pos;
                float3 vel;
                float3 color;
                float time;
            };

            struct v2g
            {
                float4 pos : SV_POSITION;
                fixed3 color : COLOR;
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed3 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            uniform StructuredBuffer<Particle>    particles;
            uniform StructuredBuffer<uint>    visibles;

            v2g vert (uint id : SV_VertexID)
            {
                Particle p = particles[visibles[id]];
                v2g o;
                o.pos = float4(UnityObjectToViewPos(p.pos),1);
                o.color = p.color;

                return o;
            }

	       	// ジオメトリシェーダ
            [maxvertexcount(4)]
			void geom (point v2g input[1], inout TriangleStream<g2f> outStream)
			{
				g2f output;
                output.color = input[0].color;
				float4 pos = input[0].pos;
                
                const float size = 0.3;

                output.pos = mul (UNITY_MATRIX_P, pos + size * float4(float2(-0.5, -0.5), 0, 0));
                output.uv = float2(0.0, 0.0);
                outStream.Append (output);

                output.pos = mul (UNITY_MATRIX_P, pos + size * float4(float2(-0.5, +0.5), 0, 0));
                output.uv = float2(0.0, 1.0);
                outStream.Append (output);

                output.pos = mul (UNITY_MATRIX_P, pos + size * float4(float2(+0.5, -0.5), 0, 0));
                output.uv = float2(1.0, 0.0);
                outStream.Append (output);

                output.pos = mul (UNITY_MATRIX_P, pos + size * float4(float2(+0.5, +0.5), 0, 0));
                output.uv = float2(1.0, 1.0);
                outStream.Append (output);

                outStream.RestartStrip();
            }

            fixed4 frag (g2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * fixed4(i.color, 1.0);
                clip(col.a - 0.3);// 半透明でなくてカットオフ
                return col;
            }
            ENDCG
        }
    }
}
