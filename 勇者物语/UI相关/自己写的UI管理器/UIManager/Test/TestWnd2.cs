using UnityEngine;
using System.Collections;

public class TestWnd2 : UIWindowBase 
{
    private float time;

    public override void Open()
    {
        base.Open();
        IsMask = true;
        CanMarginClick = true;
        time = Time.time;
    }
    public override void AddEventListener()
    {
        UIEventListener.Get(editObject["CloseBtn"]).onClick = CloseWnd2;
        UIEventListener.Get(editObject["CloseBack"]).onClick = CloseTestWnd;

    }

    private void CloseTestWnd(GameObject go)
    {
        UIManager.WindowInfo backWnd = UIManager.Instance.GetWindow<TestWnd>();
        if (backWnd != null)
        {
            UIManager.Instance.Close(backWnd.name);
        }
    }

    public void CloseWnd2(GameObject go)
    {
        UIManager.Instance.Close<TestWnd2>();
    }

    public override void Update()
    {
        if (Time.time - time > 1.0f)
        {
            time = Time.time;
            editLabel["Label"].text = time.ToString();
        }
    }
}
