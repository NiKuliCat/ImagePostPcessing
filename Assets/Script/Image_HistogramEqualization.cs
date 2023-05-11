using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class Image_HistogramEqualization : MonoBehaviour
{
    public Texture2D texture; // initial texture
    private Color32 Gray_MAX, Gray_MIN; // texture gray range
    private Texture2D modify_tex; 
    class data
    {
       public float num;
       public float frequency; 
       public float frequncy_C;//�ۼ�Ƶ��
       public data()
       {
           num = 0;
           frequency = 0;
           frequncy_C = 0;
       }
    }

    //gray value is key , data is value
    private Dictionary<int,data> pixelsDictionary = new Dictionary<int,data>();

    private Color32[] colors; // ���ԭʼͼ�����ɫ����
    private Color32[] colors_modify; 

    private void Start()
    {
        ReadTextureData(texture);
    }
    private void OnEnable()
    {
        for (int i = 0; i <= 255; i++)
        {
            pixelsDictionary.Add(i, new data());
        }
    }

    void ReadTextureData(Texture2D tex)
    {
        //��ȡͼƬ����
        colors = tex.GetPixels32();
        Gray_MAX = Gray_MIN = colors[0];
        //ͳ��ÿһ�Ҷȼ�����������
        for(int i  = 0; i < colors.Length; i++)
        {
            pixelsDictionary[colors[i].r].num++;

            Gray_MAX = Gray_MAX.r < colors[i].r ? colors[i] : Gray_MAX;
            Gray_MIN = Gray_MIN.r > colors[i].r ? colors[i] : Gray_MIN;

        }

        //ͳ��ÿһ�Ҷȼ��ĳ���Ƶ��
        for(int i = 0; i<=255;i++)
        {
            pixelsDictionary[i].frequency = pixelsDictionary[i].num / colors.Length;
        }

        //ͳ��ÿһ�Ҷȼ����ۼ�Ƶ��
        for (int i = 0; i <= 255; i++)
        {
            for(int j = 0;j<=i;j++)
            {
                pixelsDictionary[i].frequncy_C += pixelsDictionary[j].frequency;
            }
        }


        Debug.Log("read end");

        Texture_HistogramEqualization();
    }

    void Texture_HistogramEqualization()
    {
        colors_modify = colors;

        //���Բ�ֵ��ɫ
        for(int i = 0;i<colors.Length;i++)
        {
            colors_modify[i] = Color32.Lerp(Gray_MIN,Gray_MAX,pixelsDictionary[colors[i].r].frequncy_C) ;
        }

        //д��������
        modify_tex = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);

        modify_tex.SetPixels32(colors_modify);
        modify_tex.Apply();

        //����
        string path = string.Format("Assets/Image/modify_{0}_{1}_{2}.png", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
        File.WriteAllBytes(path, modify_tex.EncodeToPNG());

        Debug.Log("sava modify texture !");

    }

}
