using UnityEngine;
using System;
using System.Collections;

//动态表情
[RequireComponent(typeof(UITexture))]
public class TextureExpression : MonoBehaviour 
{
    //行
    private int row;
    //列
    private int column;
    private Vector2 uvrectSize;
    private UITexture mainTexture;
    //当前显示到第几帧
    private int curIndex;

    //通过此方法设置表情
    [ContextMenu("SetEmojiInLabel")]
    public void SetTexture(string textureName)
    {
        mainTexture = gameObject.GetComponent<UITexture>();
        mainTexture.mainTexture = Resources.Load("Emoji/" + textureName) as Texture;

        string[] names = mainTexture.mainTexture.name.Split(new char[] { '_' });
        row = Convert.ToInt32(names[1]);
        column = Convert.ToInt32(names[2]);
        curIndex = 1;

        uvrectSize = new Vector2(1f / column, 1f / row);
    }

    //帧间隔时间
    private float frameTime = 0.2f;
    private float nowTime = 0;
	void Update () 
    {
        if ((Time.time - nowTime) > frameTime)
        {
            nowTime = Time.time;
            curIndex++;
            while (curIndex > row * column)
            {
                curIndex -= row * column;
            }

            Vector2 curPosition = new Vector2(((curIndex - 1) % column) * uvrectSize.x, 1 - (Mathf.Floor((curIndex - 1) / column) + 1) * uvrectSize.y);
            mainTexture.uvRect = new Rect(curPosition, uvrectSize);
        }
	}
}
