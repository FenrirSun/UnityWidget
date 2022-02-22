using System;
using System.Xml;

public class CLandChecker:IChecker
{
    public override void Init(XmlElement ele)
    {
    }

    public override bool Check()
    {
        return true;
    }
}