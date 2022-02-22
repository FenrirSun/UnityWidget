using System;
using UnityEngine;

public class CSkillPos
{
    public CXmlSkillPos Xml{get;private set;}
    public Vector3 Pos{get;set;}
    private CSkillEvent Ev{get;set;}


    public CSkillPos(CXmlSkillPos xml,CSkillEvent ev)
    {
        Xml = xml;
        Ev = ev;
        Pos = Vector3.zero;
    }

    public void Init()
    {
        CalPos();
    }

    public void Tick()
    {
        if (Xml.IsAutoChange)
        {
            CalPos();
        }
    }

    private void CalPos()
    {
         Vector3 ptPos = Xml.GetPosAfterCreate(Ev);

        if (ptPos != Vector3.zero)
        {
            Pos = ptPos;
        }
   }
}