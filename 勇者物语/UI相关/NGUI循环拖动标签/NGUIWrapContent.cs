using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EquipUIWrapContent : MonoBehaviour
{
    public delegate void OnInitializeItem(GameObject go, int wrapIndex, int realIndex);

    /// <summary>
    /// Width or height of the child items for positioning purposes.
    /// </summary>

    public int itemSize = 100;
    /// <summary>
    /// Whether the content will be automatically culled. Enabling this will improve performance in scroll views that contain a lot of items.
    /// </summary>
    [HideInInspector]
    public bool cullContent = false;

    /// <summary>
    /// Minimum allowed index for items. If "min" is equal to "max" then there is no limit.
    /// For vertical scroll views indices increment with the Y position (towards top of the screen).
    /// </summary>
    [HideInInspector]
    public int minIndex = 0;

    /// <summary>
    /// Maximum allowed index for items. If "min" is equal to "max" then there is no limit.
    /// For vertical scroll views indices increment with the Y position (towards top of the screen).
    /// </summary>
    [HideInInspector]
    public int maxIndex = 0;

    /// <summary>
    /// Callback that will be called every time an item needs to have its content updated.
    /// The 'wrapIndex' is the index within the child list, and 'realIndex' is the index using position logic.
    /// </summary>

    public OnInitializeItem onInitializeItem;

    Transform mTrans;
    O_UIPanel mPanel;
    O_UIDraggablePanel mScroll;
    bool mHorizontal = false;
    bool mFirstTime = true;
    List<Transform> mChildren = new List<Transform>();

    public float m_PanelY = 0;
    public float m_OriginPanelY = 0;

    /// <summary>
    /// Initialize everything and register a callback with the UIPanel to be notified when the clipping region moves.
    /// </summary>

    protected virtual void Start()
    {
        SortBasedOnScrollMovement();
        WrapContent();
        if (itemSize <= 0)
        {
            itemSize = 1;
        }
        mFirstTime = false;
    }

    /// <summary>
    /// Immediately reposition all children.
    /// </summary>

    [ContextMenu("Sort Based on Scroll Movement")]
    public void SortBasedOnScrollMovement()
    {
        if (!CacheScrollView()) return;

        // Cache all children and place them in order
        mChildren.Clear();
        for (int i = 0; i < mTrans.childCount; ++i)
            mChildren.Add(mTrans.GetChild(i));

        // Sort the list of children so that they are in order
        ResetChildPositions();
    }

    /// <summary>
    /// Immediately reposition all children, sorting them alphabetically.
    /// </summary>

    [ContextMenu("Sort Alphabetically")]
    public void SortAlphabetically()
    {
        if (!CacheScrollView()) return;

        // Cache all children and place them in order
        mChildren.Clear();
        for (int i = 0; i < mTrans.childCount; ++i)
            mChildren.Add(mTrans.GetChild(i));

        // Sort the list of children so that they are in order
        mChildren.Sort(O_UIGrid.SortByName);
        ResetChildPositions();
    }

    /// <summary>
    /// Cache the scroll view and return 'false' if the scroll view is not found.
    /// </summary>

    protected bool CacheScrollView()
    {
        mTrans = transform;
        mPanel = NGUITools.FindInParents<O_UIPanel>(gameObject);
        m_OriginPanelY = mPanel.clipRange.y;
        mScroll = mPanel.GetComponent<O_UIDraggablePanel>();
        if (mScroll == null) return false;
        if (mScroll.scale.y <= 0) mHorizontal = true;
        else if (mScroll.scale.y > 0) mHorizontal = false;
        else return false;
        return true;
    }

    /// <summary>
    /// Helper function that resets the position of all the children.
    /// </summary>

    void ResetChildPositions()
    {
        for (int i = 0, imax = mChildren.Count; i < imax; ++i)
        {
            Transform t = mChildren[i];
            t.localPosition = mHorizontal ? new Vector3(i * itemSize, 0f, 0f) : new Vector3(0f, -i * itemSize, 0f);
            UpdateItem(t, i);
        }
    }

    /// <summary>
    /// Wrap all content, repositioning all children as needed.
    /// </summary>

    public void WrapContent()
    {
        float extents = itemSize * mChildren.Count * 0.5f;
        Vector3[] corners = mPanel.worldCorners;
        for (int i = 0; i < 4; ++i)
        {
            Vector3 v = corners[i];
            v = mTrans.InverseTransformPoint(v);
            corners[i] = v;
        }

        Vector3 center = Vector3.Lerp(corners[0], corners[2], 0.5f);
        bool allWithinRange = true;
        float ext2 = extents * 2f;

        if (mHorizontal)
        {
            float min = corners[0].x - itemSize;
            float max = corners[2].x + itemSize;

            for (int i = 0, imax = mChildren.Count; i < imax; ++i)
            {
                Transform t = mChildren[i];
                float distance = t.localPosition.x - center.x;

                if (distance < -extents)
                {
                    Vector3 pos = t.localPosition;
                    pos.x += ext2;
                    distance = pos.x - center.x;
                    int realIndex = Mathf.RoundToInt(pos.x / itemSize);

                    if (minIndex == maxIndex || (minIndex <= realIndex && realIndex <= maxIndex))
                    {
                        t.localPosition = pos;
                        UpdateItem(t, i);
                    }
                    else allWithinRange = false;
                }
                else if (distance > extents)
                {
                    Vector3 pos = t.localPosition;
                    pos.x -= ext2;
                    distance = pos.x - center.x;
                    int realIndex = Mathf.RoundToInt(pos.x / itemSize);

                    if (minIndex == maxIndex || (minIndex <= realIndex && realIndex <= maxIndex))
                    {
                        t.localPosition = pos;
                        UpdateItem(t, i);
                    }
                    else allWithinRange = false;
                }
                else if (mFirstTime) UpdateItem(t, i);

                if (cullContent)
                {
                    distance += 0 - mTrans.localPosition.x;
                    if (!UICamera.IsPressed(t.gameObject))
                        NGUITools.SetActive(t.gameObject, (distance > min && distance < max), false);
                }
            }
        }
        else
        {
            float min = corners[0].y - itemSize;
            float max = corners[2].y + itemSize;

            for (int i = 0, imax = mChildren.Count; i < imax; ++i)
            {
                Transform t = mChildren[i];
                float distance = t.localPosition.y - center.y;

                if (distance < -extents)
                {
                    Vector3 pos = t.localPosition;
                    pos.y += ext2;
                    distance = pos.y - center.y;
                    int realIndex = Mathf.RoundToInt(pos.y / itemSize);

                    if (minIndex == maxIndex || (minIndex <= realIndex && realIndex <= maxIndex))
                    {
                        t.localPosition = pos;
                        UpdateItem(t, i);
                    }
                    else allWithinRange = false;
                }
                else if (distance > extents)
                {
                    Vector3 pos = t.localPosition;
                    pos.y -= ext2;
                    distance = pos.y - center.y;
                    int realIndex = Mathf.RoundToInt(pos.y / itemSize);

                    if (minIndex == maxIndex || (minIndex <= realIndex && realIndex <= maxIndex))
                    {
                        t.localPosition = pos;
                        UpdateItem(t, i);
                    }
                    else allWithinRange = false;
                }
                else if (mFirstTime) UpdateItem(t, i);

                if (cullContent)
                {
                    distance += 0 - mTrans.localPosition.y;
                    if (!UICamera.IsPressed(t.gameObject))
                        NGUITools.SetActive(t.gameObject, (distance > min && distance < max), false);
                }
            }
        }
        mScroll.restrictWithinPanel = !allWithinRange;
    }

    /// <summary>
    /// Sanity checks.
    /// </summary>

    void OnValidate()
    {
        if (maxIndex < minIndex)
            maxIndex = minIndex;
        if (minIndex > maxIndex)
            maxIndex = minIndex;
    }

    /// <summary>
    /// Want to update the content of items as they are scrolled? Override this function.
    /// </summary>

    void Update() { WrapContent();}

    void LateUpdate()
    {
        CenterOn();
    }

    //void FixedUpdate()
    //{
    //    if (mPanel == null || mScroll == null) { return; }
    //    m_PanelY = mPanel.clipRange.y;

    //    if (isPress)
    //    {
    //        if (mScroll.isDrag == false)
    //        {
    //            isPress = false;
    //            shouldMove = true;
    //            isCheck = false;
    //            float length = m_PanelY - m_OriginPanelY;
    //            length = Mathf.Abs(length) - ((int)(Mathf.Abs(length) / itemSize)) * itemSize;

    //            int count = Mathf.RoundToInt(m_PanelY / (float)itemSize);

    //            shouldMoveLength = itemSize * count;
    //        }
    //    }
    //    else
    //    {
    //        if (mScroll.isDrag == true)
    //        {
    //            isPress = true;
    //            isCheck = false;
    //            shouldMove = false;
    //        }
    //    }

    //    if (shouldMove)
    //    {
    //        float curLength = Mathf.Abs(shouldMoveLength - m_PanelY);

    //        if (curLength > moveSize * Time.fixedDeltaTime)
    //        {
    //            if (curLength > moveSize * Time.fixedDeltaTime * 30)
    //            {
    //                mScroll.MoveRelative(new Vector2(0, moveSize * Time.fixedDeltaTime * Mathf.Sign(shouldMoveLength - m_PanelY) * -3));
    //            }
    //            else if (curLength > moveSize * Time.fixedDeltaTime * 30 && curLength < moveSize * Time.fixedDeltaTime * 20)
    //            {
    //                mScroll.MoveRelative(new Vector2(0, moveSize * Time.fixedDeltaTime * Mathf.Sign(shouldMoveLength - m_PanelY) * -2));
    //            }
    //            else
    //            {
    //                mScroll.MoveRelative(new Vector2(0, moveSize * Time.fixedDeltaTime * Mathf.Sign(shouldMoveLength - m_PanelY) * -1));
    //            }

    //            isCheck = false;
    //        }
    //        else
    //        {
    //            if (isCheck == false)
    //            {
    //                isCheck = true;
    //            }
    //            else
    //            {
    //                shouldMove = false;
    //            }
    //            mScroll.MoveRelative(new Vector2(0, shouldMoveLength - m_PanelY));
    //        }
    //    }
    //}

    bool isPress = false;
    [HideInInspector]
    public bool shouldMove = false;
    [HideInInspector]
    public bool isCheck = false;
    float shouldMoveLength = 0;
    public float moveSize = 30f;
    public void CenterOn()
    {
        if (mPanel == null || mScroll == null) { return; }
        m_PanelY = mPanel.clipRange.y;

        if(isPress)
        {
            if (mScroll.isDrag == false)
            {
                isPress = false;
                shouldMove = true;
                isCheck = false;
                float length = m_PanelY - m_OriginPanelY;
                length = Mathf.Abs(length) - ((int)( Mathf.Abs(length) / itemSize)) * itemSize;

                int count = Mathf.RoundToInt(m_PanelY / (float)itemSize);

                shouldMoveLength = itemSize * count;
            }
        }
        else
        {
            if (mScroll.isDrag == true)
            {
                isPress = true;
                isCheck = false;
                shouldMove = false;
            }
        }

        if(shouldMove)
        {
            float curLength = Mathf.Abs(shouldMoveLength - m_PanelY);
            
            if (curLength > moveSize * Time.deltaTime)
            {
                if (curLength > moveSize * Time.deltaTime * 30)
                {
                    mScroll.MoveRelative(new Vector2(0, moveSize * Time.deltaTime * Mathf.Sign(m_PanelY - shouldMoveLength) * 4));
                }
                else if (curLength > moveSize * Time.deltaTime * 30 && curLength < moveSize * Time.deltaTime * 15)
                {
                    mScroll.MoveRelative(new Vector2(0, moveSize * Time.deltaTime * Mathf.Sign(m_PanelY - shouldMoveLength) * 2));
                }
                else
                {
                    mScroll.MoveRelative(new Vector2(0, moveSize * Time.deltaTime * Mathf.Sign(m_PanelY - shouldMoveLength) * 1));
                }

                isCheck = false;
            }
            else
            {
                if(isCheck == false)
                {
                    isCheck = true;
                }
                else
                {
                    shouldMove = false;
                }
                mScroll.MoveRelative(new Vector2(0, m_PanelY - shouldMoveLength));
            }
        }
    }
    public void MoveLength(float length)
    {
        shouldMove = true;
        isCheck = false;
        shouldMoveLength = mPanel.clipRange.y + length;
    }
    protected virtual void UpdateItem(Transform item, int index)
    {
        if (onInitializeItem != null)
        {
            int realIndex = (mScroll.scale.y > 0) ?
                Mathf.RoundToInt(item.localPosition.y / itemSize) :
                Mathf.RoundToInt(item.localPosition.x / itemSize);
            onInitializeItem(item.gameObject, index, realIndex);
        }
    }
}
