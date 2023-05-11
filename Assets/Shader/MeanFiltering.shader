Shader "Pineapple/MeanFiltering"
{
    Properties
    {
        _MainTex("Texture",2D) = "white"{}
    }
    SubShader
    {
        HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"


        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);


        half _Intensity;
        float4  _MainTex_TexelSize;

        struct Attributes
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        Varyings vertexCompute(Attributes i)
        {
            Varyings o = (Varyings)0;
            o.positionCS = TransformObjectToHClip(i.positionOS);
            o.uv = i.uv;

            return o;
        }


        float4 SmoothFiltering(float2 uv,float2 offset)
        {
            float4 color = float4(0,0,0,0);
            color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
            color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offset);
            color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - offset);
            color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + 2 * offset);
            color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - 2 * offset);
            color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + 3 * offset);
            color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - 3 * offset);

            color *= 0.14286f;

            return color;
        }

        //水平方向
        float4 fragShading_HorizontalSmooth(Varyings v) : SV_TARGET
        {
            half2 uv = v.uv;

            float2 offset = float2(_MainTex_TexelSize.x, 0);
         
            return SmoothFiltering(uv, offset);
        }

        //垂直方向
        float4 fragShading_VerticalSmooth(Varyings v) : SV_TARGET
        {
             half2 uv = v.uv;

             float2 offset = float2(0, _MainTex_TexelSize.x);
             return SmoothFiltering(uv, offset);
        }


        ENDHLSL

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vertexCompute
            #pragma fragment fragShading_HorizontalSmooth

            ENDHLSL

        }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vertexCompute
            #pragma fragment fragShading_VerticalSmooth

            ENDHLSL

        }
    }
}
