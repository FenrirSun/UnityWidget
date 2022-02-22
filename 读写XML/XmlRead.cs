using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CXmlRead
{
    private XmlElement m_pkEle;

    public CXmlRead(XmlElement pkEle) { m_pkEle = pkEle; }
    public void SetXmlElement(XmlElement pkEle) { m_pkEle = pkEle; }

    public XmlNode GetChild(string name)
    {
        return m_pkEle.SelectSingleNode(name);
    }
    public bool HasChild(string name)
    {
        return m_pkEle.SelectSingleNode(name) != null;
    }
    public bool HasAttribute(string szAttr)
    {
        return m_pkEle.HasAttribute(szAttr);
    }
    public int Int(string szAttr) { return Int(szAttr, 0); }
    public int Int(string szAttr, int iDefault)
    {
        int ret;
        if (m_pkEle.HasAttribute(szAttr))
        {
            string str = m_pkEle.GetAttribute(szAttr);
            ret = int.Parse(str);
        }
        else
        {
            ret = iDefault;
        }
        return ret;
    }

    public byte Byte(string szAttr) { return Byte(szAttr, 0); }
    public byte Byte(string szAttr, byte iDefault)
    {
        byte ret;
        if (m_pkEle.HasAttribute(szAttr))
        {
            string str = m_pkEle.GetAttribute(szAttr);
            ret = byte.Parse(str);
        }
        else
        {
            ret = iDefault;
        }
        return ret;
    }

    public bool Bool(string szAttr) { return Bool(szAttr, false); }
    public bool Bool(string szAttr, bool bDefault)
    {
        bool ret;
        if (m_pkEle.HasAttribute(szAttr))
        {
            string str = m_pkEle.GetAttribute(szAttr);
            ret = ((str == "1") || (str == "true") || (str == "True"));
        }
        else
        {
            ret = bDefault;
        }
        return ret;
    }

    public double Double(string szAttr) { return Double(szAttr, 0.0); }
    public double Double(string szAttr, double dDefault)
    {
        double ret;
        if (m_pkEle.HasAttribute(szAttr))
        {
            string str = m_pkEle.GetAttribute(szAttr);
            ret = double.Parse(str);
        }
        else
        {
            ret = dDefault;
        }
        return ret;
    }

    public float Float(string szAttr) { return Float(szAttr, 0.0f); }
    public float Float(string szAttr, float fDefault)
    {
        float ret;
        if (m_pkEle.HasAttribute(szAttr))
        {
            string str = m_pkEle.GetAttribute(szAttr);
            ret = float.Parse(str);
        }
        else
        {
            ret = fDefault;
        }
        return ret;
    }

    public string Str(string szAttr) { return Str(szAttr, ""); }
    public string Str(string szAttr, string szDefault)
    {
        string ret;
        if (m_pkEle.HasAttribute(szAttr))
        {
            ret = m_pkEle.GetAttribute(szAttr);
        }
        else
        {
            ret = szDefault;
        }
        return ret;
    }

    public Vector3 Point2(string szAttr)
    {
        List<float> vec = new List<float>();
        Vec(szAttr, vec);
        if (vec.Count == 2)
        {
            return new Vector2(vec[0], vec[1]);
        }
        else
        {
            return Vector2.zero;
        }
    }

    public Vector3 Point3(string szAttr)
    {
        List<float> vec = new List<float>();
        Vec(szAttr, vec);
        if (vec.Count == 3)
        {
            return new Vector3(vec[0], vec[1], vec[2]);
        }
        else
        {
            return Vector3.zero;
        }
    }


    public void Vec<T>(string szAttr, List<T> vec)
    {
        string szValue = Str(szAttr);
        if (szValue.Length > 0)
        {
            string[] result = szValue.Split(new char[] {' ',','});
            foreach (string str in result)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    vec.Add((T)Convert.ChangeType(str, typeof(T)));
                }
            }
        }
    }

    public T Attr<T>(string szAttr, T def)
    {
        T ret;
        if (m_pkEle.HasAttribute(szAttr))
        {
            string str = m_pkEle.GetAttribute(szAttr);
            ret = (T)Convert.ChangeType(str, typeof(T));
        }
        else
        {
            ret = def;
        }
        return ret;
    }

    public static XmlElement GetRootElement(string file)
    {
        XmlElement root = null;

        XmlDocument doc = new XmlDocument();
        try
        {
            TextAsset text = Resources.Load(file) as TextAsset;
            doc.LoadXml(text.text);
            root = doc.DocumentElement;
        }
        catch (System.Exception ex)
        {
            Debug.Log(file + " xml load failed!" + ex.ToString());
        }

        return root;
    }
}
