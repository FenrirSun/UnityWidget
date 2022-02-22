using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CXmlSkillRange
{
    public static float LockTargetRangeDelta = 0f;
    public class CXmlShape
    {
        public virtual void Init(XmlElement ele) { }
        public virtual bool IsInRange(Vector3 ptCenter, Vector3 ptDir, Vector3 ptPos, CharacterBase character) { return false; }

#if UNITY_EDITOR
        public virtual void Draw() { }
        public virtual void Export(XmlDocument doc, XmlNode parent, string name = "pos")
        {
        }
#endif
    }

    public class CXmlRect : CXmlShape
    {
        private CXmlSkillValueOfLvl<float> A;
        private CXmlSkillValueOfLvl<float> B;
        private float H { get; set; }

        public override void Init(XmlElement ele)
        {
            CXmlRead kXmlRead = new CXmlRead(ele);
            A = new CXmlSkillValueOfLvl<float>(ele, "a", 0.0f);
            B = new CXmlSkillValueOfLvl<float>(ele, "b", 0.0f);
            H = kXmlRead.Float("h");
        }

        public override bool IsInRange(Vector3 ptCenter, Vector3 ptDir, Vector3 ptPos, CharacterBase character)
        {
            Vector3 dir = ptDir;
            dir.y = 0.0f;
            Vector3 right = Vector3.Cross(Vector3.up, dir);
            right.y = 0.0f;
            Vector3 q = ptPos - ptCenter;
            q.y = 0.0f;
            if (Vector3.Dot(q, dir) < 0.0f)
            {
                return false;
            }

            {
                float a = dir.x;
                float b = dir.z;
                float s = Mathf.Abs(b * q.x - a * q.z) / Mathf.Sqrt(a * a + b * b);
                if (s > A.GetValue(character)/ 2)
                {
                    return false;
                }
            }
            {
                float a = right.x;
                float b = right.z;
                float s = Mathf.Abs(b * q.x - a * q.z) / Mathf.Sqrt(a * a + b * b);
                if (s > B.GetValue(character))
                {
                    return false;
                }
            }
            return true;
        }

#if UNITY_EDITOR
        public override void Draw()
        {
            EditorGUILayout.BeginHorizontal("box");

            if (A == null)
            {
                A = new CXmlSkillValueOfLvl<float>("长", 0.0f);
            }
            else
            {
                A.SetName("长");
            }

            if (B == null)
            {
                B = new CXmlSkillValueOfLvl<float>("宽", 0.0f);
            }
            else
            {
                B.SetName("宽");
            }

            A.Draw();
            B.Draw();
            EditorGUILayout.LabelField("高:", GUILayout.Width(30));
            H = EditorGUILayout.FloatField(H);
            EditorGUILayout.Space();

            EditorGUILayout.EndHorizontal();
        }
        public override void Export(XmlDocument doc, XmlNode parent, string name = "rect")
        {
            XmlElement rect = doc.CreateElement(name);

            rect.SetAttribute("h",H.ToString());
            A.Export(doc, rect, "a");
            B.Export(doc, rect, "b");
            parent.AppendChild(rect);
        }
#endif
    }

    public class CXmlCircle : CXmlShape
    {
        private CXmlSkillValueOfLvl<float> r;
        private float h { get; set; }

        public override void Init(XmlElement ele)
        {
            CXmlRead kXmlRead = new CXmlRead(ele);
            h = kXmlRead.Float("h");
            r = new CXmlSkillValueOfLvl<float>(ele, "r", 0.0f);
        }
        public override bool IsInRange(Vector3 ptCenter, Vector3 ptDir, Vector3 ptPos, CharacterBase character)
        {
            float R = r.GetValue(character);
            if (character == null)
            {
                R += CXmlSkillRange.LockTargetRangeDelta;
            }
            return (ptPos - ptCenter).sqrMagnitude <= R * R;
        }
#if UNITY_EDITOR
        public override void Draw() 
        {
            EditorGUILayout.BeginHorizontal("box");

            if(r == null)
            {
                r = new CXmlSkillValueOfLvl<float>("半径",0.0f);
            }
            else{
                r.SetName("半径");
            }
            EditorGUILayout.LabelField("柱高:", GUILayout.Width(30));
            h = EditorGUILayout.FloatField(h);
            EditorGUILayout.Space();

            r.Draw();

            EditorGUILayout.EndHorizontal();
        }
        public override void Export(XmlDocument doc, XmlNode parent, string name = "circle")
        {
            XmlElement circle = doc.CreateElement(name);

            circle.SetAttribute("h", h.ToString());
            r.Export(doc, circle, "r");
            parent.AppendChild(circle);
        }
#endif
    }

    public class CXmlFan : CXmlShape
    {
        private float a { get; set; }
        private CXmlSkillValueOfLvl<float> r;
        private float h { get; set; }

        public override void Init(XmlElement ele)
        {
            CXmlRead kXmlRead = new CXmlRead(ele);
            a = kXmlRead.Float("a");
            r = new CXmlSkillValueOfLvl<float>(ele, "r", 0.0f);
            h = kXmlRead.Float("h");
        }

        public override bool IsInRange(Vector3 ptCenter, Vector3 ptDir, Vector3 ptPos, CharacterBase character)
        {
            Vector3 q = ptPos - ptCenter;
            float R = r.GetValue(character);
            if (character == null)
            {
                R += CXmlSkillRange.LockTargetRangeDelta;
            }
            if (q.sqrMagnitude < R * R)
            {
                Vector3 p = ptDir;
                p.Normalize();
                q.Normalize();
                float fAlpha = Mathf.Acos(Vector3.Dot(p, q));
                if (fAlpha * 360.0f / Mathf.PI < a)
                {
                    return true;
                }
            }
            return false;
        }

#if UNITY_EDITOR
        public override void Draw()
        {
            EditorGUILayout.BeginHorizontal("box");

            if (r == null)
            {
                r = new CXmlSkillValueOfLvl<float>("半径", 0.0f);
            }
            else
            {
                r.SetName("半径");
            }
            EditorGUILayout.LabelField("柱高:", GUILayout.Width(30));
            h = EditorGUILayout.FloatField(h);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("夹角:", GUILayout.Width(30));
            a = EditorGUILayout.FloatField(a);
            EditorGUILayout.Space();
            r.Draw();

            EditorGUILayout.EndHorizontal();
        }
        public override void Export(XmlDocument doc, XmlNode parent, string name = "fan")
        {
            XmlElement fan = doc.CreateElement(name);
            fan.SetAttribute("a", a.ToString());
            fan.SetAttribute("h", h.ToString());
            r.Export(doc, fan, "r");
            parent.AppendChild(fan);
        }
#endif
    }

    public class CXmlRing : CXmlShape
    {
        private float rIn { get; set; }
        public CXmlSkillValueOfLvl<float> rOut;
        private float h { get; set; }

        public override void Init(XmlElement ele)
        {
            CXmlRead kXmlRead = new CXmlRead(ele);
            rIn = kXmlRead.Float("rIn");
            rOut = new CXmlSkillValueOfLvl<float>(ele, "rOut", 0.0f);
            h = kXmlRead.Float("h");
        }
        public override bool IsInRange(Vector3 ptCenter, Vector3 ptDir, Vector3 ptPos, CharacterBase character)
        {
            float rSqr = (ptPos - ptCenter).sqrMagnitude;
            float ROut = rOut.GetValue(character);
            return (rSqr >= rIn * rIn && rSqr <= ROut * ROut);
        }

#if UNITY_EDITOR
        public override void Draw()
        {
            EditorGUILayout.BeginHorizontal("box");

            if (rOut == null)
            {
                rOut = new CXmlSkillValueOfLvl<float>("环的外径", 0.0f);
            }
            else
            {
                rOut.SetName("环的外径");
            }

            EditorGUILayout.LabelField("环的内径:", GUILayout.Width(60));
            rIn = EditorGUILayout.FloatField(rIn);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("柱高:", GUILayout.Width(30));
            h = EditorGUILayout.FloatField(h);
            EditorGUILayout.Space();

            rOut.Draw();

            EditorGUILayout.EndHorizontal();
        }
        public override void Export(XmlDocument doc, XmlNode parent, string name = "ring")
        {
            XmlElement ring = doc.CreateElement(name);

            ring.SetAttribute("h", h.ToString());
            ring.SetAttribute("rIn", rIn.ToString());
            rOut.Export(doc, ring, "rOut");
            parent.AppendChild(ring);
        }
#endif
    }

    public string TargetType { get; private set; }
    private bool IsRandom { get; set; }
    private CXmlShape XmlShape { get; set; }
    private CXmlSkillPos XmlPos { get; set; }
    private CXmlSkillDir XmlDir { get; set; }

    public CXmlSkillRange() { }
    public void Init(XmlElement ele)
    {
        CXmlRead kXmlRead = new CXmlRead(ele);
        TargetType = kXmlRead.Str("targetType");
        IsRandom = kXmlRead.Bool("random");
        string type = kXmlRead.Str("type");

        foreach (XmlNode node in ele.ChildNodes)
        {
            switch (node.Name)
            {
                case "pos":
                    {
                        string[] szPermission = { "preEvent", "self", "ground", "save", "preEffect","effect" };
                        XmlPos = new CXmlSkillPos();
                        XmlPos.Init(node as XmlElement, szPermission, szPermission.Length);
                    }
                    break;
                case "dir":
                    {
                        string[] szPermission = { "preEvent", "self", "ground", "twoPoint", "save", "preEffect" };
                        XmlDir = new CXmlSkillDir();
                        XmlDir.Init(node as XmlElement, szPermission, szPermission.Length);
                    }
                    break;
            }

            if (node.Name == type)
            {
                switch (type)
                {
                    case "circle":
                        XmlShape = new CXmlCircle();
                        break;
                    case "fan":
                        XmlShape = new CXmlFan();
                        break;
                    case "rect":
                        XmlShape = new CXmlRect();
                        break;
                    case "ring":
                        XmlShape = new CXmlRing();
                        break;
                    default:

                        break;
                }
                if (XmlShape != null)
                {
                    XmlShape.Init(node as XmlElement);
                }
                else
                {
                    Debug.LogError("range type = " + type + "not defined!");
                }
            }
        }

    }

#if UNITY_EDITOR
    private bool m_Draw = false;
    private string[] m_RangeType = new string[4] { "circle", "fan", "rect", "ring" };
    private string[] m_TargetType = new string[6] { "self ","team ","monster ","player ","herb ","mine" };
    public string m_type;
    public bool m_Effective = false;
    public bool m_IsDrawEffectToggle = false;

    public void Draw()
    {
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        if (m_IsDrawEffectToggle)
        {
            m_Effective = EditorGUILayout.Toggle(m_Effective, GUILayout.Width(10));
        } 
        m_Draw = EditorGUILayout.Foldout(m_Draw, "Range");
        GUILayout.EndHorizontal();

        if (string.IsNullOrEmpty(m_type))
        {
            m_type = m_RangeType[0];
        }
        int index = -1;
        for (int i = 0; i < m_RangeType.Length; i++)
        {
            if (m_type == m_RangeType[i])
            {
                index = i;
                break;
            }
        }
        if (index == -1)
        {
            m_type = m_RangeType[0];
            index = 0;
        }

        if (string.IsNullOrEmpty(TargetType))
        {
            TargetType = m_TargetType[2];
        }
        int index_target = -1;
        for (int i = 0; i < m_TargetType.Length; i++)
        {
            if (TargetType == m_TargetType[i])
            {
                index_target = i;
                break;
            }
        }
        if (index_target == -1)
        {
            TargetType = m_TargetType[2];
            index_target = 2;
        }

        if (m_Draw)
        {

            switch (m_type)
            {
                case "circle":
                    {
                        if (XmlShape == null || !(XmlShape is CXmlCircle))
                        {
                            XmlShape = new CXmlCircle();
                        }
                        break;
                    }
                case "fan":
                    {
                        if (XmlShape == null || !(XmlShape is CXmlFan))
                        {
                            XmlShape = new CXmlFan();
                        }
                        break;
                    }
                case "rect":
                    {
                        if (XmlShape == null || !(XmlShape is CXmlRect))
                        {
                            XmlShape = new CXmlRect();
                        }
                        break;
                    }
                case "ring":
                    {
                        if (XmlShape == null || !(XmlShape is CXmlRing))
                        {
                            XmlShape = new CXmlRing();
                        }
                        break;
                    }
            }

            if (XmlPos == null)
            {
                string[] szPermission = { "self", "target", "ground", "event", "p2pPos", "circlePos" };
                XmlPos = new CXmlSkillPos();
                XmlPos.InitEditor(szPermission, "位置");
            }
            else
            {
                string[] szPermission = { "self", "target", "ground", "event", "p2pPos", "circlePos" };

                XmlPos.InitEditor(szPermission, "位置");
            }
            if (XmlDir == null)
            {
                string[] szPermission = { "self", "target", "ground", "twoPoint", "event" };
                XmlDir = new CXmlSkillDir();
                XmlDir.InitEditor(szPermission, "方向");
            }
            else
            {
                string[] szPermission = { "self", "target", "ground", "twoPoint", "event" };
                XmlDir.InitEditor(szPermission, "方向");
            }

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("随机:", GUILayout.Width(30));
            IsRandom = EditorGUILayout.Toggle(IsRandom);
            EditorGUILayout.Space();

            if (index != -1)
            {
                EditorGUILayout.LabelField("形状:", GUILayout.Width(50));
                m_type = m_RangeType[EditorGUILayout.Popup(index, m_RangeType)];
                EditorGUILayout.Space();
            }

            if(index_target != -1)
            {
                EditorGUILayout.LabelField("目标:", GUILayout.Width(50));
                TargetType = m_TargetType[EditorGUILayout.Popup(index_target, m_TargetType)];
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndHorizontal();

            if (XmlShape != null)
            {
                XmlShape.Draw();
            }

            XmlPos.Draw();
            XmlDir.Draw();
        }
        GUILayout.EndVertical();
    }

    public void Export(XmlDocument doc, XmlNode parent)
    {

        if (string.IsNullOrEmpty(m_type))
        {
            m_type = m_RangeType[0];
        }

        if (string.IsNullOrEmpty(TargetType))
        {
            TargetType = m_TargetType[2];
        }

        XmlElement range = doc.CreateElement("range");
        //-----------------------------------------------------
        range.SetAttribute("type", m_type);
        range.SetAttribute("targetType", TargetType);
        range.SetAttribute("random", IsRandom.ToString());
        //-----------------------------------------------------
        if (XmlShape != null)
        {
            XmlShape.Export(doc, range, m_type);
        }
        if (XmlPos != null)
        {
            XmlPos.Export(doc, range, "pos");
        }
        if (XmlDir != null)
        {
            XmlDir.Export(doc, range, "dir");
        }
        parent.AppendChild(range);
    }
#endif

    public void GetTargets(CSkill skill, CSkillEvent preEv, List<CharacterBase> targets, int iMax, CharacterBase character)
    {
        Vector3 ptCenter = XmlPos.GetPosBeforeCreate(skill, preEv, character);
        ptCenter.y = 0.0f;
        Vector3 ptDir = XmlDir.GetDirBeforeCreate(skill, preEv, character);


        Func<CharacterBase, bool> CheckIfInRangeAndAddIt = player =>
        {
            Transform tran = player.GetTransformByDummy("Dummy_Hit");
            if (tran != null)
            {
                Vector3 pos = tran.position;
                pos.y = 0.0f;
                if (XmlShape.IsInRange(ptCenter, ptDir, pos, skill != null ? skill.Owner : null))
                {
                    targets.Add(player);
                    return true;
                }
                else
                {
                     return false;
                }
            }
            else
            {    
                 Debug.Log("error Dummy_Hit=" + player.Property.InstanceID);
                 return false;
            }
        };

        if (GameApp.GetSceneManager().CheckCombatMode(eCombatMode.CM_PK) || 
            GameApp.GetSceneManager().CheckCombatMode(eCombatMode.CM_AutoFight) || 
            (GameApp.GetSceneManager().IsFieldFight()) || 
            GameApp.GetSceneManager().CheckCombatMode(eCombatMode.CM_DuoRenPK))
        {
            if (TargetType == "monster")
            {
                foreach (CharacterBase player in GameApp.GetWorldManager().Players.Values)
                {
                    if (skill != null && skill.Owner == player)
                    {
                        continue;
                    }
                    if (player.Dead == false && player.CanBeAttack())
                    {
                        if (character != null && (character == player || character.m_GroupType == player.m_GroupType))
                        {
                            continue;
                        }
                        if(character==null && GameApp.GetWorldManager().MainPlayer.m_GroupType==player.m_GroupType)
                        {
                             continue;
                        }
                        CheckIfInRangeAndAddIt(player);
                    }
                }
            }
        }

        switch (TargetType)
        {
            case "monster":
                {
                    foreach (CharacterBase monster in GameApp.GetWorldManager().Monsters.Values)
                    {
                        if (skill != null && skill.Owner == monster)  
                        {
                            continue;
                        }
                        if (monster!=null && monster.Dead == false && monster.CanBeAttack()) 
                        {
                            CheckIfInRangeAndAddIt(monster);
                        }
                    }
                }
                break;
            case "player":
                {
                    foreach (CharacterBase player in GameApp.GetWorldManager().Players.Values)
                    {
                        if (skill != null && skill.Owner == player)
                        {
                            continue;
                        }
                        if (player.Dead == false && player.CanBeAttack())
                        {
                            if (character != null && (character == player || character.m_GroupType == player.m_GroupType))
                            {
                                continue;
                            }
                            CheckIfInRangeAndAddIt(player);
                        }
                    }

                    foreach (var pet in GameApp.GetWorldManager().Pets.Values)
                    {
                        if (skill != null && skill.Owner == pet)
                        {
                            continue;
                        }
                        if (pet.Dead == false && pet.CanBeAttack())
                        {
                            CheckIfInRangeAndAddIt(pet);
                        }
                    }

                    if (GameApp.GetWorldManager().MainPlayer.CanBeAttack())
                    {
                        CheckIfInRangeAndAddIt(GameApp.GetWorldManager().MainPlayer);
                    }
                }
                break;
            case "all":
                {
                    foreach (var monster in GameApp.GetWorldManager().Monsters.Values)
                    {
                        if (skill != null && skill.Owner == monster)
                        {
                            continue;
                        }

                        if (monster.Dead == false && monster.CanBeAttack())
                        {
                            CheckIfInRangeAndAddIt(monster);
                        }
                    }
                    foreach (var player in GameApp.GetWorldManager().Players.Values)
                    {
                        if (skill != null && skill.Owner == player)
                        {
                            continue;
                        }
                        if (player.Dead == false && player.CanBeAttack())
                        {
                            if (character != null && (character == player || character.m_GroupType == player.m_GroupType))
                            {
                                continue;
                            }
                            CheckIfInRangeAndAddIt(player);
                        }
                    }
                    if (GameApp.GetWorldManager().MainPlayer.CanBeAttack())
                    {
                        CheckIfInRangeAndAddIt(GameApp.GetWorldManager().MainPlayer);
                    }
                }
                break;
        }

        if (targets.Count > iMax)
        {
            if (IsRandom)
            {
                //打乱顺序
                var rnd = GameApp.Instance().CommonRand;
                for (int i = targets.Count - 1; i > 0; --i)
                {
                    int index = rnd.Next(i + 1);
                    CharacterBase t = targets[index];
                    targets[index] = targets[i];
                    targets[i] = t;
                }
            }
            else
            {
                targets.Sort(new Sorter(GameApp.GetWorldManager().MainPlayer.m_ObjInstance.transform.position));
            }
        }
    }

    class Sorter : IComparer<CharacterBase>
    {
        private Vector3 ptPos;
        public Sorter(Vector3 ptPos)
        {
            this.ptPos = ptPos;
        }
        public int Compare(CharacterBase x, CharacterBase y)
        {
            float l = (x.m_ObjInstance.transform.position - ptPos).sqrMagnitude;
            float r = (y.m_ObjInstance.transform.position - ptPos).sqrMagnitude;
            if (l < r)
                return -1;
            else if (l > r)
                return 1;
            else
                return 0;
        }
    }
}