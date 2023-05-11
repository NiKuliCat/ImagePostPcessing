using UnityEngine;
using System;
using System.IO;


public class SaveImage : MonoBehaviour
{

    private  Camera camera;


    private  RenderTexture rt;


    private void Start()
    {
        SaveTexture();
    }
    private void OnEnable()
    {
        InitCamera();
        GetRT();
    }

    [ExecuteAlways]
    void InitCamera()
    {
        if (camera != null)
            return;
        GameObject TempCamera = new GameObject("TempCamera");
        TempCamera.transform.parent = transform;
        camera = TempCamera.AddComponent<Camera>();
        camera.CopyFrom(transform.GetComponent<Camera>());
        camera.targetTexture = GetRT();
        camera.Render();
    }
    [ExecuteAlways]
    RenderTexture GetRT()
    {
        if(rt != null) 
            return rt;

        rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.Default);
        return rt;
    }



   public  void SaveTexture()
    {
        if(camera == null || rt == null)
        {
            Debug.Log("Init Error");
            return;
        }
        Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32,false);
        RenderTexture.active = rt;
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();
        byte[] data = texture.EncodeToPNG();
        string path = string.Format("Assets/Image/Denoising/Gaussian_{0}_{1}_{2}.png", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
        
        File.WriteAllBytes(path, data);
        Destroy(texture);
        texture = null;
        Debug.Log("±£´æ³É¹¦£¡" + path);
    }

    



}
