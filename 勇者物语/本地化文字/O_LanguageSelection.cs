//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright ?2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Turns the popup list it's attached to into a language selection list.
/// </summary>

[RequireComponent(typeof(O_UIPopupList))]
[AddComponentMenu("NGUI/Version2.0.7/Interaction/Language Selection")]
public class O_LanguageSelection : MonoBehaviour
{
	O_UIPopupList mList;

	void Start ()
	{
		mList = GetComponent<O_UIPopupList>();
		UpdateList();
		mList.eventReceiver = gameObject;
		mList.functionName = "OnLanguageSelection";
	}

	void UpdateList ()
	{
        //if (Localization.instance != null && Localization.instance.listLanguageName != null)
        //{
        //    mList.items.Clear();

        //    for (int i = 0, imax = Localization.instance.listLanguageName.Count; i < imax; ++i)
        //    {
        //        //TextAsset asset = Localization.instance.languages[i];
        //        //if (asset != null) 
        //        string name = Localization.instance.listLanguageName[i];
        //        mList.items.Add(Localization.instance.GetLocalizeLangName(name));
        //    }
        //    mList.selection = Localization.instance.GetLocalizeLangName(Localization.instance.currentLanguage);
        //}
	}

	void OnLanguageSelection (string language)
	{
		if (O_Localization.instance != null)
		{
            O_Localization.instance.currentLanguage = O_Localization.instance.GetLangNameByLocalizeName(language);
		}
	}
}