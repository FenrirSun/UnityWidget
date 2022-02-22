using System;
using UnityEngine;
public class CSkillHitAction
{
    public CXmlSkillHitAction Xml { get; set; }
    public CSkillEvent Ev { get; private set; }
    public CSkillHitRoute SkillHitRoute { get; private set; }
    public CSkillInfluence SkillInfluence { get; private set; }
    public CSkillDir SkillDir { get; private set; }


    public CSkillHitAction(CXmlSkillHitAction xml, CSkillInfluence influence, CSkillEvent ev)
    {
        Xml = xml;
        Ev = ev;
        SkillInfluence = influence;
    }

    public void Init()
    {
        if (Xml.XmlDir != null)
        {
            SkillDir = new CSkillDir(Xml.XmlDir, Ev);
            SkillDir.Init();
        }
        if (Ev.Target != null)
        {
            string where = null;
            string statename = Ev.Target.m_ML2FsmComp.GetCurState().GetStateName();
            if (statename=="StateHitUp")
            {
                where = "air";
            }
            else if (statename == "StateStandup")
            {
                where = "fall";
            }
            else
            {
                where = "land";
            }
           // Debug.Log("CXmlSkillHitRoute");
            CXmlSkillHitRoute route = Xml.GetRoute(where);
            if (route != null)
            {
              //  Debug.Log("CSkillHitRoute");
			    SkillHitRoute = new CSkillHitRoute(route, Ev, this);
                SkillHitRoute.Init();
            }
        }
    }
}