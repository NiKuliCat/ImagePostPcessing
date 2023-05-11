Shader "Pineapple/HistogramEqualization"
{
    Properties
    {
        _Image("Image",2D) = "white" {}
        _Modify_Image("Modify Image",2D) = "white" {}
    }
    SubShader
    {

        HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"

        TEXTURE2D(_Image);
        SAMPLER(sampler_Image);

        TEXTURE2D(_Modify_Image);
        SAMPLER(sampler_Modify_Image);

        half _Intensity;

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


        float3 GrayColor(float3 color)
        {
            return dot(color, float3(0.299, 0.587, 0.114));
        }



        Varyings vertexCompute(Attributes i)
        {
            Varyings o = (Varyings)0;
            o.positionCS = TransformObjectToHClip(i.positionOS);
            o.uv = i.uv;

            return o;
        }

        float4 fragShading_HistogramEqualization(Varyings v) : SV_TARGET
        {
            //float3 color = GrayColor(SAMPLE_TEXTURE2D(_Image,sampler_Image,v.uv).rgb);
            float3 color = SAMPLE_TEXTURE2D(_Image,sampler_Image,v.uv).rgb;
            float3 color_modify = SAMPLE_TEXTURE2D(_Modify_Image, sampler_Modify_Image, v.uv).rgb;
            return float4(lerp(color, color_modify, _Intensity), 1.0);
        }



        ENDHLSL


        Pass
        {
            HLSLPROGRAM

            #pragma vertex vertexCompute
            #pragma fragment fragShading_HistogramEqualization

            ENDHLSL

        }
    }
}
