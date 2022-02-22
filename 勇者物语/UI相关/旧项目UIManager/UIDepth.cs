using UnityEngine;
using System.Collections;
using System.Linq;

[ExecuteInEditMode]
public class UIDepth : MonoBehaviour
{

    public UIWidget weidget;
    public UIPanel panel;
     
    public int depth = 1;

    private UIPanel selfPanel;
    private UIWidget selfWeidget;
    [HideInInspector]
    public bool onlyOnece = false;
    [HideInInspector]
    public bool startsMaxDepth;
    [HideInInspector]
    public bool maxDepthIgnore;

    // Use this for initialization
    void Start()
    {
        if (!weidget && !panel)
            panel = transform.parent.GetComponentInParent<UIPanel>();

        selfPanel = GetComponent<UIPanel>();
        if (!selfPanel)
            selfWeidget = GetComponent<UIWidget>();


        if (startsMaxDepth)
        {
            int maxDepth = UIPanelNextDepth();
            if (selfPanel)
            {
                selfPanel.depth = maxDepth;
            }
            else if (selfWeidget)
            {
                selfWeidget.depth = maxDepth;
            }
            enabled = false;
            return;
        }

        UpdateDepth();

        if (onlyOnece)
        {
            enabled = false;
        }



    }
    private static string[] skipPanels = new string[] { "TopPanel", "BottomPanel" };

    public static int UIPanelNextDepth()
    {
        UIPanel[] panels;

        panels = (UIPanel[])GameObject.FindObjectsOfType(typeof(UIPanel));


        int maxDepth = -1;
        foreach (var panel in panels)
        {

            if (skipPanels.Contains(panel.name))
                continue;
            maxDepth = Mathf.Max(maxDepth, panel.depth);
        }
        return maxDepth + 1;
    }

    // Update is called once per frame
    void Update()
    {

        UpdateDepth();

    }

    public void UpdateDepth()
    {
        int depthVal = depth;
        if (panel)
        {
            depthVal = panel.depth + depth;
        }
        else if (weidget)
        {
            depthVal = weidget.depth + depth;
        }

        if (selfPanel)
        {
            selfPanel.depth = depthVal;
        }
        else if (selfWeidget)
        {
            selfWeidget.depth = depthVal;
        }
    }

}
