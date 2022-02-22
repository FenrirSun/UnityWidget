using System;
using UnityEngine;
public class CSkillInfluence
{
    public CXmlSkillInfluence Xml { get; set; }
    public CSkillEvent Ev{get;private set;}
    public CSkillHitAction SkillHitAction { get; private set; }

    public CSkillInfluence(CXmlSkillInfluence xml, CSkillEvent ev)
    {
        Xml = xml;
        Ev = ev;
    }

    public void Init()
    {
        if (CanInfluence())
        {
            if (Xml.XmlHitAction != null)
            {
              //  Debug.Log("XmlHitAction");
                SkillHitAction = new CSkillHitAction(Xml.XmlHitAction, this, Ev);
                SkillHitAction.Init();
            }

            if (Xml.XmlGravity != null)
            {
                Xml.XmlGravity.Gravity(Ev);
            }
        }

        //if (Xml.XmlBuffs != null)
        //{
        //    foreach (CXmlSkillBuff buff in Xml.XmlBuffs)
        //    {
        //        buff.Do(Ev.Owner, Ev.Target, Xml.DamagePower.GetValue(Ev.Owner));
        //    }
        //}
    }

    public bool CanInfluence()
    {
        if (Ev.Owner != null && Ev.Target != null)
        {
          //  Debug.Log("DamagePower=" + Xml.DamagePower.GetValue(Ev.Owner) + "  Owerpower=" + Ev.Owner.GetDamagePower() + "  targetPower=" + Ev.Target.GetSteelPower());
            return Xml.DamagePower.GetValue(Ev.Owner) + Ev.Owner.GetDamagePower() > Ev.Target.GetSteelPower();
        }
        else
        {
            if (Ev.Owner == null)
            {
                Debug.Log("CanInfluence is error!");
            }
            if (Ev.Target == null)
            {
                Debug.Log("CanInfluence 2");
            }
            return false;
        }
    }
}