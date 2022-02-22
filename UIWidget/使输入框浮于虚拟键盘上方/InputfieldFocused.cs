using UnityEngine;
using System.Collections;
using UnityEngine.UI;

///<summary>
///UI组件，用于ios端弹出输入框之后移动到输入框上方，此组件挂载在输入框inputField上
///author: sunhong
///2020-01-09 21-57-04
///</summary>
[RequireComponent(typeof(InputField), typeof(RectTransform))]
public class InputfieldFocused : MonoBehaviour
{
    InputfieldSlideScreen slideScreen;
    InputField inputField;
    RectTransform _selfRect;
    RectTransform selfRect
    {
        get
        {
            if (_selfRect == null)
                _selfRect = transform.rectTransform();
            return _selfRect;
        }
    }
//#if UNITY_IOS

    void Start()
    {
        slideScreen = gameObject.GetComponentInParent<InputfieldSlideScreen>();
        inputField = transform.GetComponent<InputField>();
        inputField.shouldHideMobileInput = true;
    }

    void Update()
    {
        if (inputField.isFocused)
        {
            // Input field focused, let the slide screen script know about it.
            slideScreen.InputFieldActive = true;
            if (slideScreen.childRectTransform != selfRect)
                slideScreen.childRectTransform = selfRect;
        }
    }

//#endif
}
