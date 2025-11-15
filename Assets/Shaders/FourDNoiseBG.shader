Shader "Unlit/FourDNoiseBG"
{
    Properties
    {
        _MainTex("Noise Texture", 2D) = "white" {}
        _Intensity("Intensity", Range(0,1)) = 0.4
        _ScrollW("4D Scroll", Float) = 0
        _ColorA("Color A", Color) = (0.2, 0.1, 0.8, 1)
        _ColorB("Color B", Color) = (0.0, 1.0, 0.8, 1)
    }

        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 100

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                sampler2D _MainTex;
                float _Intensity;
                float _ScrollW;

                // You FORGOT these — required for color blending
                float4 _ColorA;
                float4 _ColorB;

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
                    o.uv = v.uv;
                    return o;
                }

                float snoise4(float4 v);

                float snoise4(float4 v)
                {
                    const float F4 = 0.309016994374947451;
                    const float G4 = 0.138196601125010504;

                    float4 i = floor(v + dot(v, float4(F4,F4,F4,F4)));
                    float4 x0 = v - i + dot(i, float4(G4,G4,G4,G4));

                    float4 rank = step(x0.yzwx, x0.xyzw);
                    float4 s1 = rank * (1 - rank.zwxy);
                    float4 s2 = (1 - rank) * rank.zwxy;

                    float4 i1 = min(s1, float4(1,1,1,1));
                    float4 i2 = max(s2, float4(0,0,0,0));

                    float4 i3 = float4(1,1,1,1) - i1 - i2;

                    float4 x1 = x0 - i1 + G4;
                    float4 x2 = x0 - i2 + 2.0 * G4;
                    float4 x3 = x0 - i3 + 3.0 * G4;
                    float4 x4 = x0 - 1.0 + 4.0 * G4;

                    float4 w;
                    w.x = dot(x0, x0);
                    w.y = dot(x1, x1);
                    w.z = dot(x2, x2);
                    w.w = dot(x3, x3);

                    float4 m = max(0.6 - w, 0.0);
                    m = m * m;

                    float4 px = float4(
                        dot(x0, float4(1,1,1,1)),
                        dot(x1, float4(1,1,1,1)),
                        dot(x2, float4(1,1,1,1)),
                        dot(x3, float4(1,1,1,1))
                    );

                    return dot(m * m, px) * 10.0;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    float2 uv = i.uv * 4.0;

                    float n = snoise4(float4(uv.x, uv.y, _ScrollW, _Time.y * 0.1));

                    // normalize noise 0 → 1
                    n = (n + 1.0) * 0.5;

                    // smooth out the noise to avoid "river" look
                    n = smoothstep(0.2, 0.8, n);

                    // blend between your two colors
                    float3 col = lerp(_ColorA.rgb, _ColorB.rgb, n);

                    return float4(col * _Intensity, 1);
                }

                ENDCG
            }
        }
}
