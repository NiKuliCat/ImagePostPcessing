Shader "Pineapple/MedianFiltering"
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
        float GrayColor(float4 color)
        {
            return dot(color.rgb , float3(0.299, 0.587, 0.114));
        }
        float Luminance(float4 color) {
            return  0.2125 * color.r + 0.7154 * color.g + 0.0721 * color.b;
        }

        float4 MedianFiltering_Color(Varyings v)
        {
            float4 color = float4(0, 0, 0, 0);
            float2 uv = v.uv;
            float4 Color_Tex[5];

            Color_Tex[0] = _MainTex.SampleLevel(sampler_MainTex, uv, 0);
            Color_Tex[1] = _MainTex.SampleLevel(sampler_MainTex, uv + _MainTex_TexelSize.xy, 0);
            Color_Tex[2] = _MainTex.SampleLevel(sampler_MainTex, uv + float2(-1, 1) * _MainTex_TexelSize.xy, 0);
            Color_Tex[3] = _MainTex.SampleLevel(sampler_MainTex, uv - _MainTex_TexelSize.xy, 0);
            Color_Tex[4] = _MainTex.SampleLevel(sampler_MainTex, uv + float2(1, -1) * _MainTex_TexelSize.xy, 0);

            float4 tempColor = float4(0, 0, 0, 0);
            float tempGray = 0;
            float pos = 0;

            for (half i = 0; i < 5; i++)
            {
                tempColor = Color_Tex[i];
                tempGray = Luminance(Color_Tex[i]);
                pos = i;
                for (half j = i + 1; j < 5; j++)
                {
                    if (Luminance(Color_Tex[j] )< tempGray)
                    {
                        tempColor = Color_Tex[j];
                        pos = j;
                    }
                }
                Color_Tex[pos] = Color_Tex[i];
                Color_Tex[i] = tempColor;
            }

            return  Color_Tex[2];
        }

        float4 MinFiltering_Color(Varyings v)
        {
            float4 color = float4(0, 0, 0, 0);
            float2 uv = v.uv;
            float4 Color_Tex[5];

            Color_Tex[0] = _MainTex.SampleLevel(sampler_MainTex, uv, 0);
            Color_Tex[1] = _MainTex.SampleLevel(sampler_MainTex, uv + _MainTex_TexelSize.xy, 0);
            Color_Tex[2] = _MainTex.SampleLevel(sampler_MainTex, uv + float2(-1, 1) * _MainTex_TexelSize.xy, 0);
            Color_Tex[3] = _MainTex.SampleLevel(sampler_MainTex, uv - _MainTex_TexelSize.xy, 0);
            Color_Tex[4] = _MainTex.SampleLevel(sampler_MainTex, uv + float2(1, -1) * _MainTex_TexelSize.xy, 0);

            float4 tempColor = float4(0, 0, 0, 0);
            float tempGray = 0;
            float pos = 0;

            for (half i = 0; i < 5; i++)
            {
                tempColor = Color_Tex[i];
                tempGray = GrayColor(Color_Tex[i]);
                pos = i;
                for (half j = i + 1; j < 5; j++)
                {
                    if (GrayColor(Color_Tex[j]) < tempGray)
                    {
                        tempColor = Color_Tex[j];
                        pos = j;
                    }
                }
                Color_Tex[pos] = Color_Tex[i];
                Color_Tex[i] = tempColor;
            }

            return  Color_Tex[0];
        }

        float4 MaxFiltering_Color(Varyings v)
        {
            float4 color = float4(0, 0, 0, 0);
            float2 uv = v.uv;
            float4 Color_Tex[5];

            Color_Tex[0] = _MainTex.SampleLevel(sampler_MainTex, uv, 0);
            Color_Tex[1] = _MainTex.SampleLevel(sampler_MainTex, uv + _MainTex_TexelSize.xy, 0);
            Color_Tex[2] = _MainTex.SampleLevel(sampler_MainTex, uv + float2(-1, 1) * _MainTex_TexelSize.xy, 0);
            Color_Tex[3] = _MainTex.SampleLevel(sampler_MainTex, uv - _MainTex_TexelSize.xy, 0);
            Color_Tex[4] = _MainTex.SampleLevel(sampler_MainTex, uv + float2(1, -1) * _MainTex_TexelSize.xy, 0);

            float4 tempColor = float4(0, 0, 0, 0);
            float tempGray = 0;
            float pos = 0;

            for (half i = 0; i < 5; i++)
            {
                tempColor = Color_Tex[i];
                tempGray = GrayColor(Color_Tex[i]);
                pos = i;
                for (half j = i + 1; j < 5; j++)
                {
                    if (GrayColor(Color_Tex[j]) < tempGray)
                    {
                        tempColor = Color_Tex[j];
                        pos = j;
                    }
                }
                Color_Tex[pos] = Color_Tex[i];
                Color_Tex[i] = tempColor;
            }

            return  Color_Tex[4];
        }


        float4 fragShading_MedianFiltering(Varyings v) : SV_TARGET
        {

            return MedianFiltering_Color(v);
        }
        float4 fragShading_MinFiltering(Varyings v) : SV_TARGET
        {

            return MedianFiltering_Color(v);
        }
        float4 fragShading_MaxFiltering(Varyings v) : SV_TARGET
        {

            return MedianFiltering_Color(v);
        }

        ENDHLSL

        Pass
        {
            HLSLPROGRAM


            #pragma vertex vertexCompute
            #pragma fragment fragShading_MinFiltering;

            ENDHLSL

        }

        Pass
        {
            HLSLPROGRAM


            #pragma vertex vertexCompute
            #pragma fragment fragShading_MedianFiltering;

            ENDHLSL

        }

        Pass
        {
            HLSLPROGRAM


            #pragma vertex vertexCompute
            #pragma fragment fragShading_MaxFiltering;

            ENDHLSL

        }

    }
}
