using System;
using System.Xml;
#if UNITY_EDITOR
#endif

public class ITrifle
{
    public bool IsNeedSimulate{get;private set;}//������Ҵ�����ʱ�費��Ҫģ��

    public ITrifle(bool bNeedSimulate)
    {
        IsNeedSimulate = bNeedSimulate;
    }
    public virtual void Init(XmlElement ele){}
    public virtual void DoTrifle(CSkillEvent ev) { }
    public virtual void EndTrifle(CSkillEvent ev) { }

#if UNITY_EDITOR
    public virtual void Draw() 
    {

    }
    public virtual void Export(XmlDocument doc, XmlNode parent, string name = "trifle")
    {

    }
#endif

}