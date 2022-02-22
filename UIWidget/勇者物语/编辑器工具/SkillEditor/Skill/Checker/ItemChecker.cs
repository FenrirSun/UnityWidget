using System;
using System.Xml;

public class CItemChecker:IChecker
{
    public override void Init(XmlElement ele)
    {
    }

    public override bool Check()
    {
        return true;
    }
}