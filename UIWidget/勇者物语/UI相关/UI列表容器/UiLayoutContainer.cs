using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class UiLayoutContainer : MonoBehaviour {

    public struct Item
    {
        public GameObject Gobj;
        public Vector3 Size;
    }

    public class Line
    {
        public float Width;
        public float Height;
        public List<Item> Items = new List<Item>();
    }

    public float Space;
    private BoxCollider _bc;
    private float Limit
    {
        get
        {
            if (_bc == null)
            {
                _bc = GetComponent<BoxCollider>();
            }
            return _bc.size.x;
        }
    }
    private List<Line> _container = new List<Line> { new Line() };
    private float ContainerHeight
    {
        get
        {
            float ret = 0f;
            foreach (var line in _container)
            {
                ret += (line.Height + Space);
            }
            ret -= Space;
            return ret;
        }
    }
    private int _lineNum;

	void Start () 
    {
        
	}

    public void Push(List<GameObject> gobjList)
    {
        foreach (var gobj in gobjList)
        {
            Add(gobj);
        }
    }

    public void Add(GameObject gobj)
    {
        if (gobj == null)
        {
            return;
        }

        var boxCollider = gobj.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            return;
        }

        float itemWidth = boxCollider.size.x;
        if (itemWidth == 0 || itemWidth > Limit)
        {
            return;
        }
        
        gobj.transform.parent = gameObject.transform;
        gobj.transform.localScale = gameObject.transform.localScale;
        Line line = _container[_lineNum];
        if ((line.Width + itemWidth + Space) > Limit)
        {
            line = new Line();
            _container.Add(line);
            ++_lineNum;
        }

        line.Width += (line.Width == 0) ? itemWidth : (itemWidth + Space);
        line.Height = Mathf.Max(boxCollider.size.y, line.Height);
        line.Items.Add(new Item
        {
            Gobj = gobj,
            Size = boxCollider.size
        });
    }

    public void Refresh()
    {
        float startY = ContainerHeight / 2;
        for (int i = 0; i < _container.Count; i++)
        {
            var line = _container[i];
            float startX = -line.Width / 2;
            for (int j = 0; j < line.Items.Count; j++)
			{
                var item = line.Items[j];
                item.Gobj.transform.localPosition = new Vector3
                {
                    x = startX + item.Size.x/2,
                    y = startY - item.Size.y/2,
                    z = 1
                };
                startX += (j == 0) ? item.Size.x : (item.Size.x + Space);
			}
            startY -= (i == 0) ? line.Height : (line.Height + Space);
        }
    }
}
