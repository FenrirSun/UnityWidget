using UnityEngine;

/// <summary>
/// Simple script that lets you localize a UIWidget.
/// </summary>

[RequireComponent(typeof(O_UIWidget))]
public class O_UILocalize : MonoBehaviour
{
    public string filename = "TipsLocal"; //本地化文件名
	public int  key;        //id

	string mLanguage;
	bool mStarted = false;

    public void Refresh(int index)
    {
        O_UIWidget w = GetComponent<O_UIWidget>();
        O_UILabel lbl = w as O_UILabel;
        string val = O_Localization.instance.GetText(filename, index);
        if (lbl != null)
        {
            int index1 = lbl.text.IndexOf('[');
            int index2 = lbl.text.IndexOf(']');
            string color = null;
            if (index1 > -1)
            {
                color = lbl.text.Substring(index1, index2 + 1);
            }
            //保留颜色信息
            if (string.IsNullOrEmpty(color))
                lbl.text = val;
            else
                lbl.text = color + val;
        }
        key = index;
    }

	/// <summary>
	/// This function is called by the Localization manager via a broadcast SendMessage.
	/// </summary>
	void OnLocalize (O_Localization loc)
	{
		if (mLanguage != loc.currentLanguage)
        {
			O_UIWidget w = GetComponent<O_UIWidget>();
			O_UILabel lbl = w as O_UILabel;
			O_UISprite sp = w as O_UISprite;

			// If no localization key has been specified, use the label's text as the key
            //if (string.IsNullOrEmpty(mLanguage) && string.IsNullOrEmpty(key) && lbl != null) key = lbl.text;

            string val = loc.GetText(filename, key);
			if (lbl != null)
			{
                int index1 = lbl.text.IndexOf('[');
                int index2 = lbl.text.IndexOf(']');
                string color = null;
                if (index1 > -1)
                {
                    color = lbl.text.Substring(index1, index2 + 1);
                }
                //保留颜色信息
                if (string.IsNullOrEmpty(color))
                    lbl.text = val;
                else
                    lbl.text = color + val;
			}
			else if (sp != null)
			{
				sp.spriteName = val;
				sp.MakePixelPerfect();
			}
			mLanguage = loc.currentLanguage;
		}
	}

	/// <summary>
	/// Localize the widget on enable, but only if it has been started already.
	/// </summary>
	void OnEnable ()
	{
		if (mStarted && O_Localization.instance != null) OnLocalize(O_Localization.instance);
	}

	/// <summary>
	/// Localize the widget on start.
	/// </summary>
	void Start ()
	{
		mStarted = true;
		if (O_Localization.instance != null) OnLocalize(O_Localization.instance);
	}
}