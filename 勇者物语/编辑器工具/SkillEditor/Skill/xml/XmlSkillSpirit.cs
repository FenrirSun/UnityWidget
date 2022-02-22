using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
public class CXmlSkillSpirit
{
    public CXmlSkillValueOfLvl<float> healNum;
    public CXmlSkillValueOfLvl<float> limitNum;

    public CXmlSkillSpirit() { }
    public void Init(XmlElement ele)
    {
        healNum = new CXmlSkillValueOfLvl<float>(ele, "healNum", 0);
        limitNum = new CXmlSkillValueOfLvl<float>(ele, "limitNum", 0);
    }

#if UNITY_EDITOR
    public void Draw()
    {
        GUILayout.BeginVertical("box");

        if (healNum == null) { healNum = new CXmlSkillValueOfLvl<float>("",0); }
        if (limitNum == null) { limitNum = new CXmlSkillValueOfLvl<float>("", 0); }
        GUILayout.EndVertical();
    }
#endif
}
