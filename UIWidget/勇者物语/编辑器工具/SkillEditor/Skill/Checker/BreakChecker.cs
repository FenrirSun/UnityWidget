using System;
using System.Xml;

public class CBreakChecker:IChecker
{
    public override void Init(XmlElement ele)
    {
    }

    public override bool Check()
    {
        return true;
    }
}