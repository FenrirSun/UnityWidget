using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ChatTest2 : MonoBehaviour 
{
    public GameObject button;
    public UISprite backSprite;
    //输入框
    public UIInput m_chatInput;
    //文字模板
    public UILabel textObj;
    //表情模板
    public GameObject emojiObj;
    //碰撞框模板 
    public GameObject colliderObj;

    //显示文本
    private string showText;
    //替代表情的空格
    private string space = "     ";
    //输入文本宽度
    private Vector2 showTextLength;
    //表情位置
    private List<Vector3> eList;
    //道具碰撞框位置
    private List<List<Vector3>> iList;
    //位置碰撞框位置
    private List<List<Vector3>> pList;
    //表情宽度
    private float emojiWidth = 16;

	void Start () 
    {
        eList = new List<Vector3>();
        iList = new List<List<Vector3>>();
        pList = new List<List<Vector3>>();
        button.GetComponent<UIButton>().onClick.Add(new EventDelegate(SetSingleDialog)); 
	}

    private void SetSingleDialog()
    {
        CalculateExpressionPos();

        textObj.text = showText;

        for (int i = 0; i < eList.Count; i++)
        {
            GameObject emojiTemp = GameObject.Instantiate(emojiObj);
            emojiTemp.transform.parent = textObj.transform.parent;
            emojiTemp.transform.localScale = Vector3.one;
            emojiTemp.transform.localPosition = new Vector3(eList[i].x + 4, 14 - eList[i].y, 0);
            emojiTemp.GetComponent<TextureExpression>().SetTexture("emoji2_3_3");
            emojiTemp.SetActive(true);
        }

        for (int i = 0; i < iList.Count; i++)
        {
            for(int j = 0; j < iList[i].Count; j++)
            {
                GameObject colliderTemp = GameObject.Instantiate(colliderObj);
                colliderTemp.transform.parent = textObj.transform.parent;
                colliderTemp.transform.localScale = Vector3.one;
                colliderTemp.transform.localPosition = new Vector3(iList[i][j].x - 10, 15 - iList[i][j].y, 0);
            }
        }

        for (int i = 0; i < pList.Count; i++)
        {
            for (int j = 0; j < pList[i].Count; j++)
            {
                GameObject colliderTemp = GameObject.Instantiate(colliderObj);
                colliderTemp.transform.parent = textObj.transform.parent;
                colliderTemp.transform.localScale = Vector3.one;
                colliderTemp.transform.localPosition = new Vector3(pList[i][j].x - 10, 15 - pList[i][j].y, 0);
            }
        }

        //设置背景图大小，比文字多固定距离
        backSprite.width = Mathf.RoundToInt(showTextLength.x + 40f);
        backSprite.height = Mathf.RoundToInt(showTextLength.y + 16f);
    }

    private void CalculateExpressionPos()
    {
        NGUIText.dynamicFont = textObj.trueTypeFont;
        NGUIText.finalSize = textObj.defaultFontSize;
        //NGUIText.alignment = NGUIText.Alignment.Left;
        NGUIText.regionWidth = textObj.width;
        NGUIText.spacingY = textObj.spacingY;
        NGUIText.spacingX = textObj.spacingX;
        NGUIText.Update();
        eList.Clear();
        iList.Clear();
        pList.Clear();
        string text = m_chatInput.value;
        string expr = @"#\w{2}";
        string totalr = @"(<[\w]+>|\[\w+\s\d{1,4},\d{1,4}\])";
        string itemr = @"<[\w]+>";
        string posr = @"\[\w+\s\d{1,4},\d{1,4}\]"; 
        MatchCollection emojiList = Regex.Matches(text, expr);
        foreach (Match m in emojiList)
        {
            int index = text.IndexOf(m.ToString());
            string preText = text.Substring(0, index);

            eList.Add(NGUIText.CalculatePrintedSize(preText + "a"));
            text = text.Remove(index, 3);
            text = text.Insert(index, space);
        }
        showTextLength = NGUIText.CalculatePrintedSize(text);
        if (showTextLength.y > NGUIText.finalSize + NGUIText.spacingY) showTextLength.x = textObj.width;
        MatchCollection totalList = Regex.Matches(text, totalr);
        MatchCollection itemList = Regex.Matches(text, itemr);
        MatchCollection posList = Regex.Matches(text, posr);
        //需要先分别添加碰撞框，再统一添加颜色和下划线，不然会相互影响
        for (int i = 0; i < itemList.Count; i++)
        {
            Match m = itemList[i];
            string preText = text.Substring(0, m.Index);
            List<Vector3> charList = new List<Vector3>();
            char[] mChar = m.ToString().ToCharArray();
            for (int j = 0; j < mChar.Length; j++)
            {
                preText += mChar[j];
                if (j != 0 && j != mChar.Length - 1)
                {
                    charList.Add(NGUIText.CalculatePrintedSize(preText));
                }
            }
            iList.Add(charList);
        }

        for (int i = 0; i < posList.Count; i++)
        {
            Match m = posList[i];
            string preText = text.Substring(0, m.Index);
            List<Vector3> charList = new List<Vector3>();
            char[] mChar = m.ToString().ToCharArray();
            for (int j = 0; j < mChar.Length; j++)
            {
                preText += mChar[j];
                if (j != 0 && j != mChar.Length - 1)
                {
                    charList.Add(NGUIText.CalculatePrintedSize(preText));
                }
            }
            pList.Add(charList);
        }

        for (int i = totalList.Count - 1; i >= 0; i--)
        {
            Match m = totalList[i];
            string preText = text.Substring(0, m.Index);
            List<Vector3> charList = new List<Vector3>();
            char[] mChar = m.ToString().ToCharArray();

            text = text.Insert(m.Index + mChar.Length, @"[-][/u]");
            if (Regex.IsMatch(m.ToString(), itemr))
            {
                text = text.Insert(m.Index, "[ffff00][u]");
            }
            else
            {
                text = text.Insert(m.Index, "[ffea00][u]");
            }
        }

        showText = text;
    }
}
