using System;
using System.Xml;
#if UNITY_EDITOR
#endif

public class ITrifle
{
    public bool IsNeedSimulate{get;private set;}//网络玩家处理技能时需不需要模拟

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