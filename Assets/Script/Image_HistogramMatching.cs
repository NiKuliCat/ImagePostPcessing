using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class Image_HistogramMatching : MonoBehaviour
{
    public Texture2D texture;
    public Texture2D templateTexture;
    private Texture2D modify_texture;
    class Data
    {
        public float num;
        public float frequency;
        public float frequncy_C;//累计频率
        public Data()
        {
            num = 0;
            frequency = 0;
            frequncy_C = 0;
        }
    }

    class Color32Data
    {
        public Data[] datas = new Data[3]; 

        public Color32Data()
        {
            datas[0] = new Data();
            datas[1] = new Data();
            datas[2] = new Data();
        }
    }
    class LUT
    {
        public int target_r;
        public int target_g;
        public int target_b;

        public LUT()
        {
            target_r = 0;
            target_g = 0;
            target_b = 0;
        }
    }

    private Dictionary<int, Color32Data> pixelsDictionary = new Dictionary<int, Color32Data>();
    private Dictionary<int, Color32Data> template_pixelsDictionary = new Dictionary<int, Color32Data>();
    
    private Color32[] color32s;
    private Color32[] template_color32s;
    private Color32[] color_Modify;

    private LUT[] lut = new LUT[256];//look-up table
    private void OnEnable()
    {
        for (int i = 0; i <= 255; i++)
        {
            pixelsDictionary.Add(i, new Color32Data());
            template_pixelsDictionary.Add(i, new Color32Data());
        }

        for(int i = 0;i <= 255; i++)
        {
            lut[i] = new LUT();
        }
      
    }

    private void Start()
    {
          ReadTexture(texture, templateTexture);
    }

    void ReadTexture(Texture2D texture,Texture2D template)
    {
        color32s = texture.GetPixels32();
        template_color32s = template.GetPixels32();

        RGB_HistogramData(pixelsDictionary, color32s);
        RGB_HistogramData(template_pixelsDictionary, template_color32s);

        Debug.Log("Read RGB data completion");

        RGB_Frequncy(pixelsDictionary);
        RGB_Frequncy(template_pixelsDictionary);

        Debug.Log("compute RGB frequncy completion");


        ComputeTargetRGBToLut();
        GetModifyColor();
        SaveTexture();

    }

    //统计原始图像的rgb直方图
    void RGB_HistogramData(Dictionary<int, Color32Data> dict, Color32[] colors)
    {
        for (int i = 0; i < colors.Length; i++)
        {
            var col = colors[i];

            dict[col.r].datas[0].num++;//r
            dict[col.g].datas[1].num++;//g
            dict[col.b].datas[2].num++;//b
        }

    }

    //统计原始图像的频率
    void RGB_Frequncy(Dictionary<int, Color32Data> dict)
    {

        for (int i = 0; i <= 255; i++)
        {
            dict[i].datas[0].frequency = dict[i].datas[0].num / color32s.Length;
            dict[i].datas[1].frequency = dict[i].datas[1].num / color32s.Length;
            dict[i].datas[2].frequency = dict[i].datas[2].num / color32s.Length;
        }

        for (int i = 0; i <= 255; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                dict[i].datas[0].frequncy_C += dict[j].datas[0].frequency;
                dict[i].datas[1].frequncy_C += dict[j].datas[1].frequency;
                dict[i].datas[2].frequncy_C += dict[j].datas[2].frequency;
            }
        }
     
    }

    void ComputeTargetRGBToLut()
    {
        for(int i = 0;i <= 255;i++)
        {
            var min_r = Mathf.Abs(pixelsDictionary[i].datas[0].frequncy_C - template_pixelsDictionary[0].datas[0].frequncy_C);
            var min_g = Mathf.Abs(pixelsDictionary[i].datas[1].frequncy_C - template_pixelsDictionary[0].datas[1].frequncy_C);
            var min_b = Mathf.Abs(pixelsDictionary[i].datas[2].frequncy_C - template_pixelsDictionary[0].datas[2].frequncy_C);

            int gray_r = 0;
            int gray_g = 0;
            int gray_b = 0;
            for (int j = 1; j <= 255;j++)
            {
                if(min_r > Mathf.Abs(pixelsDictionary[i].datas[0].frequncy_C - template_pixelsDictionary[j].datas[0].frequncy_C))
                {
                    min_r = Mathf.Abs(pixelsDictionary[i].datas[0].frequncy_C - template_pixelsDictionary[j].datas[0].frequncy_C);
                    gray_r = j;
                }

                if (min_g > Mathf.Abs(pixelsDictionary[i].datas[1].frequncy_C - template_pixelsDictionary[j].datas[1].frequncy_C))
                {
                    min_g = Mathf.Abs(pixelsDictionary[i].datas[1].frequncy_C - template_pixelsDictionary[j].datas[1].frequncy_C);
                    gray_g = j;
                }
                if (min_b > Mathf.Abs(pixelsDictionary[i].datas[2].frequncy_C - template_pixelsDictionary[j].datas[2].frequncy_C))
                {
                    min_b = Mathf.Abs(pixelsDictionary[i].datas[2].frequncy_C - template_pixelsDictionary[j].datas[2].frequncy_C);
                    gray_b = j;
                }
            }

            lut[i].target_r = gray_r;
            lut[i].target_g = gray_g;
            lut[i].target_b = gray_b;
        }

        Debug.Log("look-up table init completion");
      

    }

    void GetModifyColor()
    {
        color_Modify = color32s;

        for (int i = 0; i < color32s.Length; i++)
        {
            color_Modify[i] = GetColor32ByLUT(i);
        }
    }

    Color32 GetColor32ByLUT(int i)
    {
        return new Color32(((byte)lut[color32s[i].r].target_r), ((byte)lut[color32s[i].g].target_g), ((byte)lut[color32s[i].b].target_b), color32s[i].a);
    }


    void SaveTexture( )
    {
        if (color_Modify == null)
            return;

        modify_texture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);

        modify_texture.SetPixels32(color_Modify);
        modify_texture.Apply();

        //保存
        string path = string.Format("Assets/Image/HistogramMatching/modify_{0}_{1}_{2}.png", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
        File.WriteAllBytes(path, modify_texture.EncodeToPNG());

        Debug.Log("sava modify texture !");

    }

}
