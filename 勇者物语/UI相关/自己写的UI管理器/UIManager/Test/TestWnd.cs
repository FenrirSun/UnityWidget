using UnityEngine;
using System.Collections;

public class TestWnd : UIWindowBase 
{
    public override void Open()
    {
        base.Open();
        IsMask = false;
        CanMarginClick = false;
    }
    public override void AddEventListener()
    {
        UIEventListener.Get(editObject["CloseBtn"]).onClick = OpenWnd2;
        UIEventListener.Get(editObject["CloseAllBtn"]).onClick = CloseAll;
    }

    private void CloseAll(GameObject go)
    {
        UIManager.Instance.CloseAllWnd();
    }

    private void OpenWnd2(GameObject go)
    {
        UIManager.Instance.Open<TestWnd2>(UIManager.EUILayer.WndLayer);
    }
}
