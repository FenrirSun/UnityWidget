using System;
using UnityEngine;

public class CSkillDir
{
    public CXmlSkillDir Xml{get;private set;}
    public Vector3 Dir{get;set;}
    private CSkillEvent Ev{get;set;}
    //public CSkillPos SkillPos1 { get; private set; }
    //public CSkillPos SkillPos2 { get; private set; }

    public CSkillDir(CXmlSkillDir xml, CSkillEvent ev)
    {
        Xml = xml;
        Ev = ev;
    }

    public void Init()
    {
        CalDir();
    }

    public void Tick()
    {
        if (Xml.IsAutoChange)
        {
            CalDir();
        }
    }

    private void CalDir()
    {
        Vector3 ptDir = Xml.GetDirAfterCreate(Ev);
        if (ptDir != Vector3.zero)
        {
            Dir = ptDir;
        }
    }
}