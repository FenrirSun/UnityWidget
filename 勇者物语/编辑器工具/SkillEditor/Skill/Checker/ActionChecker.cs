using System;
using System.Xml;

public class CActionChecker:IChecker
{
    public override void Init(XmlElement ele)
    {
    }

    public override bool Check()
    {
        return true;
    }
}