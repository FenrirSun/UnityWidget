using UnityEngine;
using System.Collections;
using System;
public enum UIWndFlags
{
    None = 0x0,
    BackGroundMask = 0x1,
}
public class UIWndAttributes : Attribute {
    public UIWndAttributes() { }

    public UIWndAttributes(string path) { this.path = path; }
    public UIWndFlags flags = UIWndFlags.None;
    public string path;
    public bool movable;
}
