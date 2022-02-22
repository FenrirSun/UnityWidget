using UnityEngine;
using UnityEditor;

using System.IO;
using System.Collections;

public class SreenshotForTerrain : MonoBehaviour
{

      static string filePath;
      public static Camera cam = null;
      public static Object clone ;

      [MenuItem("MapEditor/Get map")]
      static void showDialog()
      {

          Object camPre = Resources.Load("UI/HUD/Prefabs/Screenshot");
          if (camPre == null)
          {
              Debug.Log("camfre  ==null");
              return;
          }
          clone = Instantiate(camPre, Vector3.zero, Quaternion.identity);
          //cam.orthographic = true;
          
          Camera[] arrcam = Camera.allCameras;
          for (int i = 0; i < arrcam.Length; i++)
          {
              if (arrcam[i].name == "ScreenshotCamera")
              {
                  cam = arrcam[i];
                  //cam.enabled = true;
                  //arrcam[i].enabled = true;
              }
          }
          
          if (cam == null)
          {
              Debug.Log("没找到摄像机......");
          }
          
          CameraParameters();
          
          filePath = Application.dataPath + "/Resources/MiniMap.png";
          SavePNG();

          //删除截图用相机
          DestroyImmediate(clone);
          EditorUtility.DisplayDialog("消息", "截图成功", "OK");
          //cam.enabled = false;
      }

      static void SavePNG()
      {

          int width = 256;//Screen.width;//自己设,需要的图片尺寸
          int height = 256;//Screen.height;
          RenderTexture rt = new RenderTexture(width, height, 2);
          cam.targetTexture = rt;
          cam.Render();//手动开启渲染

          Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
          RenderTexture.active = rt;//设置当前RenderTexture;
          tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
          tex.Apply();
          RenderTexture.active = null;//关
          byte[] bytes = tex.EncodeToPNG();
          cam.targetTexture = null;//相机render置空
          
          File.WriteAllBytes(filePath, bytes);

      }

      static void CameraParameters()
      {

          var flag1 = GameObject.Find("ScreenshotFlag1");
          var flag2 = GameObject.Find("ScreenshotFlag2");

          Debug.Log(flag1.transform.position.x - flag2.transform.position.x);
          Debug.Log(flag1.transform.position.z - flag2.transform.position.z);

          if (flag1 == null || flag2 == null)
          {
              Debug.Log("没找到ScreenshotFlag");
          }

          var length = flag2.transform.position.x - flag1.transform.position.x;
          var width = flag2.transform.position.z - flag1.transform.position.z;

          var standard = (length > width) ? length : width;

          //相机位置
          cam.transform.position = new Vector3(flag1.transform.position.x + length / 2, 200, flag1.transform.position.z + width / 2);
          Debug.Log("cam  position"+ cam.transform.position.x);
          //相机旋转
          cam.transform.eulerAngles = new Vector3(90, 0, 0);
                   
          //正交相机的size
          cam.orthographicSize = standard/2;

      }
     
 }
