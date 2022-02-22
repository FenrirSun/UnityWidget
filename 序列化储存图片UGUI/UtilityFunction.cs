using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


public class UtilityFunction  
{
    public static string strKey = "infinity";
    public static string strIV = "metalgear";
    public static string avatarPath = "Sprite/Avatar/";
    public static string modelPath = "Sprite/Model/";
    public static string equipPath = "Sprite/Equip/";

    /// <summary>
    /// 从Prefab获取sprite
    /// </summary>
    /// <param name="folderName">该UI所在文件夹的名字</param>
    /// <param name="spriteName">所需sprite的名字</param>
    /// <param name="extendedStr">文件夹子路径，可忽略</param>
    /// <returns></returns>
    public static Sprite GetSpriteByName(string folderName, string spriteName, string extendedStr = null)
    {
        string goPath = "";
        if (string.IsNullOrEmpty(extendedStr))
        {
            goPath = string.Format("UI/{0}/{0}DynSprite", folderName);
        }
        else
        {
            goPath = string.Format("UI/{1}/{0}/{0}DynSprite", folderName, extendedStr);
        }
        GameObject go = Resources.Load(goPath) as GameObject;
        if (go == null)
        {
            Debug.LogError(string.Format("There is no prefab of {0}. Be sure that type is right and you had make atlas of type", folderName));
            return null;
        }
        AtlasMap am = go.GetComponent<AtlasMap>();
        if (am == null)
        {
            Debug.LogError(string.Format("There is no AtlasMap on prefab of {0}.Be sure MakeAtlasMaker work", folderName));
            return null;
        }
        return am.GetSpriteByName(spriteName);
    }

    // 字符串加密
    public static string Encrypt(string _strQ)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(_strQ);
        MemoryStream ms = new MemoryStream();
        DESCryptoServiceProvider tdes = new DESCryptoServiceProvider();
        CryptoStream encStream = new CryptoStream(ms, tdes.CreateEncryptor(Encoding.UTF8.GetBytes(strKey), Encoding.UTF8.GetBytes(strIV)), CryptoStreamMode.Write);
        encStream.Write(buffer, 0, buffer.Length);
        encStream.FlushFinalBlock();
        return Convert.ToBase64String(ms.ToArray());
    }

    // 字符串解密
    public static string Decrypt(string _strQ)
    {
        byte[] buffer = Convert.FromBase64String(_strQ);
        MemoryStream ms = new MemoryStream();
        DESCryptoServiceProvider tdes = new DESCryptoServiceProvider();
        CryptoStream encStream = new CryptoStream(ms, tdes.CreateDecryptor(Encoding.UTF8.GetBytes(strKey), Encoding.UTF8.GetBytes(strIV)), CryptoStreamMode.Write);
        encStream.Write(buffer, 0, buffer.Length);
        encStream.FlushFinalBlock();
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    public static Sprite LoadSprite(string name)
    {
        Sprite sprite = Resources.Load<Sprite>(avatarPath + name) as Sprite;
        if (sprite == null)
        {
            Debug.LogError("No Sprite Found!!");
        }
        return sprite;
    }

    public static Sprite LoadModel(string name)
    {
        Sprite sprite = Resources.Load<Sprite>(modelPath + name) as Sprite;
        if (sprite == null)
        {
            Debug.LogError("No Model Found!!");
        }
        return sprite;
    }

    public static Sprite LoadEquip(string name)
    {
        Sprite sprite = Resources.Load<Sprite>(equipPath + name) as Sprite;
        if (sprite == null)
        {
            Debug.LogError("No Sprite Found!!");
        }
        return sprite;
    }

    //类实例的深度复制
    public static T DeepClone<T>(T RealObject)
    {
        using (Stream objectStream = new MemoryStream())
        {
            //利用 System.Runtime.Serialization序列化与反序列化完成引用对象的复制
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(objectStream, RealObject);
            objectStream.Seek(0, SeekOrigin.Begin);
            return (T)formatter.Deserialize(objectStream);
        }
    } 

}
