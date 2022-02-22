using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;
using System.Security.Cryptography;

/// <summary>
/// 公用函数类
/// </summary>

public class CommonFun
{
    /// <summary>
    /// 显示或隐藏gameobject,包括所有子节点
    /// </summary>
    /// <param name="tsRoot">gameobject的transform属性</param>
    /// <param name="bShow"></param>
    public static void ShowAll(Transform tsRoot, bool bShow)
    {
        tsRoot.gameObject.active = bShow;
        for (int i = 0; i < tsRoot.childCount; ++i)
        {
            tsRoot.GetChild(i).gameObject.active = bShow;
            ShowAll(tsRoot.GetChild(i), bShow);
        }
    }

    /// <summary>
    /// 批量设置gameobject所有子节点的layer
    /// </summary>
    /// <param name="tsRoot">gameobject的transform属性</param>
    /// <param name="bShow"></param>
    public static void SetLayer(Transform tsRoot, int layer)
    {
        tsRoot.gameObject.layer = layer;

        for (int i = 0; i < tsRoot.childCount; ++i)
        {
            tsRoot.GetChild(i).gameObject.layer = layer;
            SetLayer(tsRoot.GetChild(i), layer);
        }
    }

    public static int ConvertToCounterClock(int ClockAangle)
    {
        return 90 - ClockAangle;
    }

    //字符串转 Vector3
    public static Vector3 Point3(string szValue)
    {
        List<float> vec = new List<float>();
        Vec(szValue, vec);
        if (vec.Count == 3)
        {
            return new Vector3(vec[0], vec[1], vec[2]);
        }
        else
        {
            return Vector3.zero;
        }
    }
    public static void Vec<T>(string szValue, List<T> vec)
    {
        if (szValue.Length > 0)
        {
            string[] result = szValue.Split(new char[] { ' ', ',' });
            foreach (string str in result)
            {
                vec.Add((T)Convert.ChangeType(str, typeof(T)));
            }
        }
    }

    public static string ConvertVector3ToString(Vector3 vec, string split)
    {
        string strVec = vec.x.ToString() + split + vec.y.ToString() + split + vec.z.ToString();
        return strVec;
    }

    public static string ListToStr(List<string> data)
    {
        string str = "";
        for (int i = 0; i < data.Count; ++ i )
        {
            str += data[i];
            if (i != data.Count -1)
            {
                str += ",";
            }
        }
        return str;
    }

    public static void SetSprite(GameObject go, string path, string spriteName)
    {
        if (go == null)
            return;

        Transform transform = null;
        if (path != "")
            transform = go.transform.Find(path);
        else
            transform = go.transform;
        if (transform == null)
            return;

        Component spriteComp = transform.gameObject.GetComponent("O_UISprite");
        if (spriteComp == null)
            return;

        (spriteComp as O_UISprite).spriteName = spriteName;
    }

    public static void SetColliderEnable(GameObject go, string path, bool enable)
    {
        if (go == null)
            return;

        Transform transform = null;
        if (path != "")
            transform = go.transform.Find(path);
        else
            transform = go.transform;
        if (transform == null)
            return;

        Component colliderComp = transform.gameObject.GetComponent("BoxCollider");
        if (colliderComp == null)
            return;

        (colliderComp as BoxCollider).enabled = enable;
    }

    public static void SetLabelText(GameObject go, string path, string text)
    {
        if (go == null)
            return;

        Transform transform = null;
        if (path != "")
            transform = go.transform.Find(path);
        else
            transform = go.transform;
        if (transform == null)
            return;

        Component labelComp = transform.gameObject.GetComponent("O_UILabel");
        if (labelComp == null)
            return;
        (labelComp as O_UILabel).text = text;
    }

    public static string GetName(string text)
    {
        string name = text;
        if (name.Contains(":"))
        {
            int index = name.IndexOf(":");
            name = name.Remove(index);
        }
        return name;
    }

    public static void SetLabelColor(GameObject go, string path, string color)
    {
        if (go == null)
            return;

        Transform transform = null;
        if (path != "")
            transform = go.transform.Find(path);
        else
            transform = go.transform;
        if (transform == null)
            return;

        Component labelComp = transform.gameObject.GetComponent("O_UILabel");
        if (labelComp == null)
            return;

        (labelComp as O_UILabel).text = color + (labelComp as O_UILabel).text;
    }

    public static void SetLabelColor(GameObject go, string path, Color color)
    {
        if (go == null)
            return;

        Transform transform = null;
        if (path != "")
            transform = go.transform.Find(path);
        else
            transform = go.transform;
        if (transform == null)
            return;

        Component labelComp = transform.gameObject.GetComponent("O_UILabel");
        if (labelComp == null)
            return;

        (labelComp as O_UILabel).color = color;
    }

    public static void SetProgressBarValue(GameObject go, string path, float value)
    {
        if (go == null)
            return;

        Transform transform = null;
        if (path != "")
            transform = go.transform.Find(path);
        else
            transform = go.transform;
        if (transform == null)
            return;

        Component sliderComp = transform.gameObject.GetComponent("O_UISlider");
        if (sliderComp == null)
            return;

        (sliderComp as O_UISlider).sliderValue = value;
    }

    public static void UILocalRefresh(GameObject go, string path, int id)
    {
        if (go == null)
            return;

        Transform transform = null;
        if (path != "")
            transform = go.transform.Find(path);
        else
            transform = go.transform;
        if (transform == null)
            return;

        Component localComp = transform.GetComponent("O_UILocalize");
        if (localComp == null || (localComp is O_UILocalize) == false)
            return;

        (localComp as O_UILocalize).Refresh(id);
    }

    public static string strKey = "menghuan";
    public static string strIV = "amiaoyou";
    // 文件解密加载
    public static void XmlLoadDecrypt(XmlDocument xmlDoc, string fileName)
    {
        FileStream fileStream = new FileStream(fileName, FileMode.Open);
        byte[] bsXml = new byte[fileStream.Length];
        fileStream.Read(bsXml, 0, bsXml.Length);
        fileStream.Close();

        MemoryStream ms = new MemoryStream();
        DESCryptoServiceProvider tdes = new DESCryptoServiceProvider();
        CryptoStream encStream = new CryptoStream(ms, tdes.CreateDecryptor(Encoding.UTF8.GetBytes(strKey), Encoding.UTF8.GetBytes(strIV)), CryptoStreamMode.Write);
        encStream.Write(bsXml, 0, bsXml.Length);
        encStream.FlushFinalBlock();

        xmlDoc.Load(new MemoryStream(ms.ToArray()));
    }

    // 文件加密存储
    public static void XmlSaveEncrypt(XmlDocument xmlDoc, string fileName)
    {
        if (!File.Exists(fileName))
            File.Create(fileName).Close();

        FileStream fileStream = new FileStream(fileName, FileMode.Truncate);
        MemoryStream msXml = new MemoryStream();
        xmlDoc.Save(msXml);

        DESCryptoServiceProvider tdes = new DESCryptoServiceProvider();
        CryptoStream cs = new CryptoStream(fileStream, tdes.CreateEncryptor(Encoding.UTF8.GetBytes(strKey), Encoding.UTF8.GetBytes(strIV)), CryptoStreamMode.Write);
        cs.Write(msXml.ToArray(), 0, msXml.ToArray().Length);
        cs.FlushFinalBlock();

        msXml.Close();
        fileStream.Close();
    }

    public static string strKey2 = "amiaoyou";
    public static string strIV2 = "menghuan";
    // 字符串加密
    public static string Encrypt(string _strQ)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(_strQ);
        MemoryStream ms = new MemoryStream();
        DESCryptoServiceProvider tdes = new DESCryptoServiceProvider();
        CryptoStream encStream = new CryptoStream(ms, tdes.CreateEncryptor(Encoding.UTF8.GetBytes(strKey2), Encoding.UTF8.GetBytes(strIV2)), CryptoStreamMode.Write);
        encStream.Write(buffer, 0, buffer.Length);
        encStream.FlushFinalBlock();
        return Convert.ToBase64String(ms.ToArray()).Replace("+", "%");
    }

    // 字符串解密
    public static string Decrypt(string _strQ)
    {
        _strQ = _strQ.Replace("%", "+");
        byte[] buffer = Convert.FromBase64String(_strQ);
        MemoryStream ms = new MemoryStream();
        DESCryptoServiceProvider tdes = new DESCryptoServiceProvider();
        CryptoStream encStream = new CryptoStream(ms, tdes.CreateDecryptor(Encoding.UTF8.GetBytes(strKey2), Encoding.UTF8.GetBytes(strIV2)), CryptoStreamMode.Write);
        encStream.Write(buffer, 0, buffer.Length);
        encStream.FlushFinalBlock();
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    public static string UTF8ToUnicodeString(string utf8String)
    {
        return utf8String;
    }

    public static void QuitGame()
    {
        GameRecord.Instance().SaveLocalFile();
        GameApp.GetGameServerHandler().StopGameModule();
        GameApp.GetGameServerHandler().SetSingle();
        GameApp.GetGameServerHandler().SetLocal();

        GameApp.GetSceneManager().EnterMap("Login");
        GameApp.GetUIManager().CloseAllWnd();
        UICameraMgr.mInstance.ClearAllGameObjects();
        if (GameApp.GetWorldManager().MainPlayer != null)
        {
            GameApp.GetWorldManager().MainPlayer.Release();
            GameApp.GetWorldManager().MainPlayer = null;
        }
        GameApp.GetAudioManager().StopMusic(0f);
    }

    public static T XmlGetValue<T>(XmlElement elem, string propName, T defaultValue)
    {
        if (elem == null)
        {
            Debug.LogWarning("<CommonFunc.XmlGetValue Error> elem == null, propName = " + propName);
            return defaultValue;
        }

        string value = elem.GetAttribute(propName);
        if (value == "")
            return defaultValue;
        else
            return (T)System.Convert.ChangeType(value, typeof(T));
    }

    public static void SetStatic(Transform tranRoot, bool value)
    {
        if (tranRoot == null)
            return;

        tranRoot.gameObject.isStatic = value;
        foreach (Transform tran in tranRoot)
        {
            if (tran.childCount == 0)
            {
                tran.gameObject.isStatic = value;
            }
            else
            {
                SetStatic(tran, value);
            }
        }
    }

}
