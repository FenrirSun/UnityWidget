using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActiveDraggablePanelNew : MonoBehaviour
{
    public int KeepCount = 10;  // 围绕面板中心点，上下两边各保留10个
    public int MinCount = 5;    // 当某一边少于5个时刷新列表
    public int OnePageCount = 5;     // 一页显示5个
    public float cellHeight = 108f;       // item间距
    public List<NetItemInfo> lstData = new List<NetItemInfo>();
    public int CurStartIndex = 0;  // 当前的显示列表的开始索引

    public delegate void IntDelegate(int value);
    public IntDelegate onFresh = null;

    private bool isStop = false;
    private float startStopTime = 0;

    public void StopOneSecond()
    {
        isStop = true;
        startStopTime = Time.time;
    }

    void Update()
    {
        if (isStop)
        {
            if (Time.time > startStopTime + 1f)
            {
                isStop = false;
            }
            else
                return;
        }

        int ret = NeedFresh();
        if (ret != 0 && onFresh != null)
            onFresh(ret);
    }

    public int GetMaxCount()
    {
        return 2 * KeepCount + OnePageCount;
    }

    // 是否需要刷新 0不刷新 -1左刷新 1右刷新
    private int NeedFresh()
    {
        // 长度没超长
        if (lstData.Count <= 2 * KeepCount + OnePageCount)
        {
            return 0;
        }

        // 到达左边不用左刷新
        // 到达右边不用右刷新
        if ((GetCurShowIndex() < (CurStartIndex + MinCount)) && ArriveLeft() == false)
        {
            return -1;
        }
        else if ((GetCurShowIndex() > (CurStartIndex + 2 * KeepCount + OnePageCount - MinCount - OnePageCount)) && ArriveRight() == false)
        {
            return 1;
        }

        return 0;
    }

    // 当前显示的最前面一个的index
    private int GetCurShowIndex()
    {
        return ((int)gameObject.transform.localPosition.y - 132 ) / (int)cellHeight;
    }

    private bool ArriveLeft()
    {
        return (CurStartIndex == 0);
    }

    private bool ArriveRight()
    {
        return CurStartIndex >= (lstData.Count - 2 * KeepCount - OnePageCount);
    }

}
