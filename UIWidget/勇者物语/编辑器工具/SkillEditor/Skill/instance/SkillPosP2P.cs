using System;
using UnityEngine;

public class CSkillPosP2P
{
    public CXmlSkillPosP2P Xml { get; private set; }
    private CSkillEvent Ev{get;set;}
    public Vector3 Pos { get { return SkillPos1.Pos; } }
    public bool IsEnd { get; set; }
    public CSkillPos SkillPos1 { get; private set; }
    public CSkillPos SkillPos2 { get; private set; }
    private float V0 { get; set; }
    private float LifeEndTime { get; set; }


    public CSkillPosP2P(CXmlSkillPosP2P xml, CSkillEvent ev)
    {
        Xml = xml;
        Ev = ev;
        IsEnd = false;
    }

    public void Init()
    {
        SkillPos1 = new CSkillPos(Xml.XmlPos1, Ev);
        SkillPos1.Init();
        SkillPos2 = new CSkillPos(Xml.XmlPos2, Ev);
        SkillPos2.Init();
        V0 = Xml.V0;
        LifeEndTime = Time.time + Xml.LifeSeconds;
    }

    public void Tick()
    {
        if (!IsEnd)
        {
            if (Time.time > LifeEndTime)
            {
                IsEnd = true;
            }
            else
            {
                if (SkillPos2.Xml.IsAutoChange)
                {
                    SkillPos2.Tick();
                }

                Vector3 ptDir = SkillPos2.Pos - SkillPos1.Pos;
                float fLen = ptDir.magnitude;
                float fInterval = Time.deltaTime;
                float fTravelLen = V0 * fInterval + 0.5f * Xml.A * fInterval * fInterval;
                if (fTravelLen > fLen)
                {
                    IsEnd = true;
                    SkillPos1.Pos = SkillPos2.Pos;
                }
                else
                {
                    ptDir.Normalize();
                    Vector3 ptNewPos = SkillPos1.Pos + fTravelLen * ptDir;
                    SkillPos1.Pos = ptNewPos;
                    V0 += Xml.A * fInterval;
                }
            }
        }
    }
}