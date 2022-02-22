using UnityEngine;
using System.Collections;
[UIWndAttributes("Scripts/UIManager/Test/Panel", flags = UIWndFlags.BackGroundMask)]
public class TestClass1 : UIWndBase
{
    protected override void Open(object data)
    {
        base.Open(data);
        AnimationHelper.BoxPlayForward(wnd);
    }

    public override void Close()
    {
        AnimationHelper.BoxPlayReverse(wnd, CloseEvent);
    }

    public void CloseEvent()
    {
        Debug.LogError("Event");
        base.Close();
    }
}
