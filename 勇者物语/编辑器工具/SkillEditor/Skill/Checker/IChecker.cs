using System;
using System.Xml;

public class IChecker
{
    public virtual bool  Check(){return true;}
    public virtual void Init(XmlElement ele){}      
}