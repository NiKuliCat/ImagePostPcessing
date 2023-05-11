Shader "Pineapple/ImageSharpening"
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


        float4 _Params;
        float4 _LineColor;
        float4 _BackgroundColor;

        float4  _MainTex_TexelSize;

        struct Attributes
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct Varyings_Roberts
        {
            float4 positionCS : SV_POSITION;
            float2 uv[4] : TEXCOORD0;
        };

        struct Varyings_Sobel
        {
            float4 positionCS : SV_POSITION;
            float2 uv[9] : TEXCOORD0;
        };

        //返回颜色明度
        float Luminance(float4 color) {
            return  0.2125 * color.r + 0.7154 * color.g + 0.0721 * color.b;
        }

        Varyings_Roberts vertexCompute_Roberts(Attributes i)
        {
            Varyings_Roberts o = (Varyings_Roberts)0;
            o.positionCS = TransformObjectToHClip(i.positionOS);

            o.uv[0] = i.uv;
            o.uv[1] = i.uv + _MainTex_TexelSize.xy * float2(1, 0) * _Params.y;
            o.uv[2] = i.uv + _MainTex_TexelSize.xy * float2(0, -1) * _Params.y;
            o.uv[3] = i.uv + _MainTex_TexelSize.xy * float2(-1, -1) * _Params.y;

            return o;
        }

        Varyings_Sobel vertexCompute_Prewitt(Attributes i)
        {
            Varyings_Sobel o = (Varyings_Sobel)0;
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

        half Roberts(Varyings_Roberts r)
        {
            //robers算子
            const half Gx[4] = { 1,0,0,-1 };
            const half Gy[4] = { 0,1,-1,0 };

            half luminance;
            half edge_x;
            half edge_y;

            for (int i = 0; i < 4; i++)
            {
                luminance = Luminance(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, r.uv[i]));

                edge_x += luminance * Gx[i];
                edge_y += luminance * Gy[i];

            }

            half edge = 1 - abs(edge_x) - abs(edge_y);

            return smoothstep(_Params.x - _Params.z * 0.05, _Params.x + _Params.z * 0.05, edge);
        }

        half Prewitt(Varyings_Sobel p)
        {
            //prewitt算子
            const half Gx[9] = { -1,-1,-1,
                                  0, 0, 0,
                                  1, 1, 1};

            const half Gy[9] = { -1, 0, 1, 
                                 -1, 0, 1,
                                 -1, 0, 1};

            half luminance;
            half edge_x;
            half edge_y;

            for (int i = 0; i < 9; i++)
            {
                luminance = Luminance(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, p.uv[i]));

                edge_x += luminance * Gx[i];
                edge_y += luminance * Gy[i];

            }

            half edge = 1 - abs(edge_x) - abs(edge_y);

            return smoothstep(_Params.x - _Params.z * 0.05, _Params.x + _Params.z * 0.05, edge);

        }

        half Sobel(Varyings_Sobel p)
        {
            //Sobel算子
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
                luminance = Luminance(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, p.uv[i]));

                edge_x += luminance * Gx[i];
                edge_y += luminance * Gy[i];

            }

            half edge = 1 - abs(edge_x) - abs(edge_y);

            return smoothstep(_Params.x - _Params.z * 0.05, _Params.x + _Params.z * 0.05, edge);

        }
        

        float4 fragShading_Sharpening_Roberts(Varyings_Roberts v) : SV_TARGET
        {
            half edge = Roberts(v);

            float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, v.uv[0]);
            color = lerp(_BackgroundColor, color, _Params.w);
            return lerp(_LineColor, color, edge);
        }

        float4 fragShading_Sharpening_Prewitt(Varyings_Sobel v) : SV_TARGET
        {
             half edge = Prewitt(v);

            float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, v.uv[0]);
            color = lerp(_BackgroundColor, color, _Params.w);
            return lerp(_LineColor, color, edge);

        }

        float4 fragShading_Sharpening_Sobel(Varyings_Sobel v) : SV_TARGET
        {
             half edge = Sobel(v);

            float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, v.uv[0]);
            color = lerp(_BackgroundColor, color, _Params.w);
            return lerp(_LineColor, color, edge);

        }

        ENDHLSL

        Pass
        {

            HLSLPROGRAM

            #pragma vertex vertexCompute_Roberts
            #pragma fragment fragShading_Sharpening_Roberts

            ENDHLSL
        }

        Pass
        {

            HLSLPROGRAM

            #pragma vertex vertexCompute_Prewitt
            #pragma fragment fragShading_Sharpening_Prewitt

            ENDHLSL
        }

        Pass
        {

            HLSLPROGRAM

            #pragma vertex vertexCompute_Prewitt
            #pragma fragment fragShading_Sharpening_Sobel

            ENDHLSL
        }
    }
}
