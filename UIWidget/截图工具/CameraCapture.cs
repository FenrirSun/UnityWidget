using UnityEngine;
using UnityEngine.Experimental.Rendering;
using System.IO;

[ExecuteInEditMode]
public class CameraCapture : MonoBehaviour
{
    // 截取的像素尺寸
    public int resWidth = 4000;
    public int resHeight = 3000;
    public Camera captureCamera;

    private string FilePath
    {
        get
        {
            return Application.dataPath + "/../../AdaptiveLearningNoCode/CameraCapture";
        }
    }

    void OnGUI()
    {
        // 这里用个简单的OnGUI的按钮来触发截图
        if (GUI.Button(new Rect(10, 10, 100, 30), "Camera Capture"))
        {
            Capture();
            Debug.Log("Shot!");
        }

        if (GUI.Button(new Rect(10, 40, 100, 30), "Open Directory"))
        {
            if(Directory.Exists(FilePath))
            {
                string path = FilePath.Replace("/", "\\");
                System.Diagnostics.Process.Start("Explorer.exe", path);
            }
            else
            {
                Debug.LogError("目录不存在，无法打开");
            }
        }
    }

    void Capture()
    {
        if (captureCamera == null)
            captureCamera = Camera.main;

        if(!Directory.Exists(FilePath))
        {
            Directory.CreateDirectory(FilePath);
        }

        // create an renderTexture to asve the image data from camera
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        captureCamera.targetTexture = rt;

        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGBA32, false);

        //render from camera
        captureCamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);

        captureCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // Save to png file
        byte[] bytes = screenShot.EncodeToPNG();
        string filename = ScreenShotName(resWidth, resHeight);
        System.IO.File.WriteAllBytes(filename, bytes);

        Debug.Log(string.Format("Took Screenshot to: {0}", filename));
    }

    public string ScreenShotName(int width, int height)
    {
        // 输入路径和文件名（自带尺寸和日期）
        return string.Format("{0}/screen_{1}x{2}_{3}.png",
                                  FilePath,
                                  width, height,
                                  System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));

    }
}

