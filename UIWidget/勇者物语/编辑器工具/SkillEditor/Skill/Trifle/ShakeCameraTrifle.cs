using System;
using System.Xml;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CShakeCameraTrifle:ITrifle
{
    private float Seconds{get;set;}
    private float Scale { get; set; }

    public CShakeCameraTrifle():base(true)
    {
    }

    public override void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        Seconds = kXmlRead.Float("seconds");
        Scale = kXmlRead.Float("scale");
    }

    public override void DoTrifle(CSkillEvent ev)
    {
        GameObject camController = GameObject.Find("CameraShakeController");
        if (camController == null)
        {
            camController = (GameObject)GameObject.Instantiate(Resources.Load("LogicPrefabs/Misc/CameraShakeController"));
            camController.name = "CameraShakeController";
        }
        if (camController == null)
            Debug.LogError("camController == null");

        CameraShakeController camShake = (CameraShakeController)camController.GetComponent("CameraShakeController");
        if (camShake != null)
        {
            camShake.Shake(1.0f);
            Camera mainCamera = Camera.main;
            ThirdPersonCam thirdPersonCam = (ThirdPersonCam)mainCamera.gameObject.GetComponent(typeof(ThirdPersonCam));
            if (thirdPersonCam != null)
                thirdPersonCam.SetShakeCamera(true, Seconds, Scale);
        }
   
    }

    public override void EndTrifle(CSkillEvent ev)
    {
        GameObject camController = GameObject.Find("CameraShakeController");
        if (camController == null)
        {
            camController = (GameObject)GameObject.Instantiate(Resources.Load("LogicPrefabs/Misc/CameraShakeController"));
            camController.name = "CameraShakeController";
        }
        if (camController == null)
            Debug.LogError("camController == null");

        CameraShakeController camShake = (CameraShakeController)camController.GetComponent("CameraShakeController");
        if (camShake != null)
        {
            Camera mainCamera = Camera.main;
            ThirdPersonCam thirdPersonCam = (ThirdPersonCam)mainCamera.gameObject.GetComponent(typeof(ThirdPersonCam));
            if (thirdPersonCam != null)
                thirdPersonCam.StopCameraShaking();
        }
    }

#if UNITY_EDITOR
    public override void Draw()
    {
        GUILayout.BeginHorizontal("box");

        EditorGUILayout.LabelField("震动摄像机:", GUILayout.Width(120));

        EditorGUILayout.LabelField("震动时间:", GUILayout.Width(60));
        Seconds = EditorGUILayout.FloatField(Seconds);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("震动幅度:", GUILayout.Width(60));
        Scale = EditorGUILayout.FloatField(Scale);
        EditorGUILayout.Space();

        GUILayout.EndHorizontal();
    }
    public override void Export(XmlDocument doc, XmlNode parent, string name = "trifle")
    {
        XmlElement trifle = doc.CreateElement(name);

        trifle.SetAttribute("name", "ShakeCamera");
        trifle.SetAttribute("seconds", Seconds.ToString());
        trifle.SetAttribute("scale", Scale.ToString());
        parent.AppendChild(trifle);
    }
#endif
}