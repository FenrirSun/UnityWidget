using UnityEngine;
using System.Collections;

public class UIWndMargin : UIWindowBase
{
    public GameObject Colider;
    public UIEventListener.VoidDelegate onMaskClicked;

    public override void Open()
    {
        IsMask = false;
        IsMovable = true;
        CanMarginClick = false;
        Colider = wndRoot.transform.Find("Colider").gameObject;
        AddEventListener();
    }

    public override void AddEventListener()
    {
        UIEventListener.Get(Colider).onClick = OnSelfClicked;
    }

    public void OnSelfClicked(GameObject go)
    {
        onMaskClicked(go);
        base.OnClose();
    }
}
