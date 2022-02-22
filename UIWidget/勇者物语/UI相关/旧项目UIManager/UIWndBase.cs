using UnityEngine;
using System.Collections;
using System.Linq;
public class UIWndBase  {
    public int id
    {
        get;
        protected set;
    }
    public bool isMovable { get; protected set; }
    public UIWndManager.eUILayer layer
    {
        get;
        protected set;
    }
    public GameObject wnd { get; protected set; }
    public UIPanel panel { get; protected set; }
    public void Create(UIWndManager.eUILayer layer, GameObject wnd, int id, object data)
    {
        var uiAttributes = (UIWndAttributes)this.GetType().GetCustomAttributes(typeof(UIWndAttributes), true).FirstOrDefault();

        if (uiAttributes != null)
        {
            isMovable = uiAttributes.movable;
        }

        this.wnd = wnd;
        this.id = id;
        this.layer = layer;
        panel = wnd.GetComponent<UIPanel>();
        if (panel == null)
        {
            Debug.LogError("Panel is null!!!");
        }
        else
        {
            panel.depth = id;
        }

        Open(data);
        AddListener();
    }
    public virtual void Update(float ealpsed_sec)
    { }
    protected virtual void Open(object data)
    {
        
    }
    protected virtual void AddListener()
    {
    }
    public virtual void Close()
    {
        UnityEngine.Object.Destroy(wnd);
    }
    public virtual void ClickMask(GameObject go) { }
}
