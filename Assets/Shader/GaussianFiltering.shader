Shader "Pineapple/GaussianFiltering"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
       HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        float4  _MainTex_TexelSize;
        float3 _Params;
       //const float PI 3.14159265f;
        
        struct Attributes
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct Varyings_Gaussian
        {
            float4 positionCS : SV_POSITION;
            float2 uv[9] : TEXCOORD0;
        };


        float gauss(float x, float y, float sigma)
        {
            return  1.0f / (2.0f * PI * sigma * sigma) * exp(-(x * x + y * y) / (2.0f * sigma * sigma));
        }
        float Luminance(float4 color) {
            return  0.2125 * color.r + 0.7154 * color.g + 0.0721 * color.b;
        }

        Varyings_Gaussian vertexCompute_Gaussian(Attributes i)
        {
            Varyings_Gaussian o = (Varyings_Gaussian)0;
            o.positionCS = TransformObjectToHClip(i.positionOS);

            o.uv[0] = i.uv + _MainTex_TexelSize.xy * float2(-1, -1) * _Params.y;
            o.uv[1] = i.uv + _MainTex_TexelSize.xy * float2(0, -1) * _Params.y;
            o.uv[2] = i.uv + _MainTex_TexelSize.xy * float2(1, -1) * _Params.y;
            o.uv[3] = i.uv + _MainTex_TexelSize.xy * float2(-1, 0) * _Params.y;
            o.uv[4] = i.uv + _MainTex_TexelSize.xy * float2(0, 0) * _Params.y;
            o.uv[5] = i.uv + _MainTex_TexelSize.xy * float2(1, 0) * _Params.y;
            o.uv[6] = i.uv + _MainTex_TexelSize.xy * float2(-1, 1) * _Params.y;
            o.uv[7] = i.uv + _MainTex_TexelSize.xy * float2(0, 1) * _Params.y;
            o.uv[8] = i.uv + _MainTex_TexelSize.xy * float2(1, 1) * _Params.y;

            return o;
        }

        
        float4 Gaussian_Color(Varyings_Gaussian v)
        {

            float G[9];

            G[0] = gauss(-1.0f, -1.0f, _Params.x);
            G[1] = gauss(0.0f, -1.0f, _Params.x);
            G[2] = gauss(1.0f, -1.0f, _Params.x);
            G[3] = gauss(-1.0f, 0.0f, _Params.x);
            G[4] = gauss(0.0f, 0.0f, _Params.x);
            G[5] = gauss(1.0f, 0.0f, _Params.x);
            G[6] = gauss(-1.0f, 1.0f, _Params.x);
            G[7] = gauss(0.0f, 1.0f, _Params.x);
            G[8] = gauss(1.0f, 1.0f, _Params.x);

            float sum = 0.0f;
            float4 color = 0;
            for (int i = 0; i <= 8;i++)
            {
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, v.uv[i]) * G[i];
                sum += G[i];
            }

            color *= 1 / sum;

            return color;
        }

        half Sobel(Varyings_Gaussian v)
        {
            //SobelËã×Ó
            const half Gx[9] = { -1,-2,-1,
                                  0, 0, 0,
                                  1, 2, 1 };

            const half Gy[9] = { -1, 0, 1,
                                 -2, 0, 2,
                                 -1, 0, 1 };

            half luminance;
            half edge_x;
            half edge_y;

            for (int i = 0; i < 9; i++)
            {
                luminance = Luminance(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, v.uv[i]));

                edge_x += luminance * Gx[i];
                edge_y += luminance * Gy[i];

            }

            half edge = 1 - abs(edge_x) - abs(edge_y);

            return smoothstep(_Params.z - 0.1 , _Params.z +0.1, edge);

        }
        float4 fragShading_Gaussian(Varyings_Gaussian v) : SV_TARGET
        {
            return Gaussian_Color(v);
        }

        float4 fragShading_Bilateral(Varyings_Gaussian v) : SV_TARGET
        {
            float4 color = Gaussian_Color(v);
            float4 mainColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, v.uv[4]);
            half sobelFactor = Sobel(v);
            return lerp( color,mainColor, sobelFactor);
        }


        ENDHLSL


        Pass
        {
            HLSLPROGRAM

            #pragma vertex vertexCompute_Gaussian
            #pragma fragment fragShading_Gaussian

            ENDHLSL

        }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vertexCompute_Gaussian
            #pragma fragment fragShading_Bilateral

            ENDHLSL

        }

       
    }
}
