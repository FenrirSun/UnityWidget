using System;
using UnityEngine;

public class CSkillPosCircle
{
    public CXmlSkillPosCircle Xml { get; private set; }
    private CSkillEvent Ev { get; set; }
    public bool IsEnd { get; set; }
    public CSkillPos SkillPos { get; private set; }
    public CSkillDir SkillDir { get; private set; }
    public Vector3 Pos { get; set; }

    private float AngleBegin;// 旋转开始时的角度
    private float TimeBegin;//旋转开始的时间
    public CSkillPosCircle(CXmlSkillPosCircle xml, CSkillEvent ev)
    {
        Xml = xml;
        Ev = ev;
        IsEnd = false;
    }

    public void Init()
    {
        SkillDir = new CSkillDir(Xml.XmlDir, Ev);
        SkillDir.Init();

        SkillPos = new CSkillPos(Xml.XmlPos, Ev);
        SkillPos.Init();

        Vector3 dir = SkillDir.Dir;
        AngleBegin = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
        AngleBegin += Xml.AngleOffset;
        TimeBegin = Time.time;

        CalPos();
    }

    public void Tick()
    {
        if (!IsEnd)
        {
            if (Time.time > TimeBegin + Xml.LifeSeconds)
            {
                IsEnd = true;
            }
            else
            {
                if (SkillPos.Xml.IsAutoChange)
                {
                    SkillPos.Tick();
                }
                CalPos();
            }
        }
    }

    private void CalPos()
    {
        float angle = AngleBegin + (Time.time - TimeBegin) * Xml.AngleV0;
        Vector3 pos = SkillPos.Pos;
        pos.x += Xml.R * Mathf.Cos(angle * Mathf.Deg2Rad);
        pos.z += Xml.R * Mathf.Sin(angle * Mathf.Deg2Rad);
        Pos = pos;
    }
}