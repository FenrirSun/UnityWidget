using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

///<summary>
///UI组件，用于ios端弹出输入框之后移动到输入框上方, 此组件挂载在面板跟节点Canvas上
///author: sunhong
///2020-01-09 21-57-04
///</summary>
[RequireComponent(typeof(RectTransform), typeof(Canvas))]
public class InputfieldSlideScreen : MonoBehaviour
{
    //这两个变量由InputfieldFocused赋值
    [HideInInspector]
    public bool InputFieldActive = false;
    [HideInInspector]
    public RectTransform childRectTransform;

    //私有变量，自己赋值
    private Canvas canvas;
    private RectTransform selfRectTransform;

    private void Awake()
    {
        canvas = transform.GetComponent<Canvas>();
        selfRectTransform = transform.GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (canvas == null || selfRectTransform == null)
            return;

#if UNITY_IOS || UNITY_ANDROID

        if (InputFieldActive && TouchScreenKeyboard.visible)
        {
            selfRectTransform.anchoredPosition = Vector2.zero;

            //Vector3[] corners = { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
            Rect rect = RectTransformExtension.GetScreenRect(childRectTransform, canvas);
            float keyboardHeight = TouchScreenKeyboard.area.height;

            float heightPercentOfKeyboard = keyboardHeight / Screen.height * 100f;
            float heightPercentOfInput = (Screen.height - (rect.y + rect.height)) / Screen.height * 100f;

            if (heightPercentOfKeyboard > heightPercentOfInput)
            {
                // keyboard covers input field so move screen up to show keyboard
                float differenceHeightPercent = heightPercentOfKeyboard - heightPercentOfInput;
                float newYPos = selfRectTransform.rect.height / 100f * differenceHeightPercent;

                Vector2 newAnchorPosition = Vector2.zero;
                newAnchorPosition.y = newYPos;
                selfRectTransform.anchoredPosition = newAnchorPosition;
            }
            else
            {
                // Keyboard top is below the position of the input field, so leave screen anchored at zero
                selfRectTransform.anchoredPosition = Vector2.zero;
            }
        }
        else
        {
            // No focus or touchscreen invisible, set screen anchor to zero
            selfRectTransform.anchoredPosition = Vector2.zero;
        }
        InputFieldActive = false;

#else

        //PC端测试用
        if (InputFieldActive)
        {
            selfRectTransform.anchoredPosition = Vector2.zero;

            //Vector3[] corners = { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
            Rect rect = RectTransformExtension.GetScreenRect(childRectTransform, canvas);
            float keyboardHeight = Screen.height / 3 * 2;

            float heightPercentOfKeyboard = keyboardHeight / Screen.height * 100f;
            float heightPercentOfInput = (Screen.height - (rect.y + rect.height)) / Screen.height * 100f;

            if (heightPercentOfKeyboard > heightPercentOfInput)
            {
                // keyboard covers input field so move screen up to show keyboard
                float differenceHeightPercent = heightPercentOfKeyboard - heightPercentOfInput;
                float newYPos = selfRectTransform.rect.height / 100f * differenceHeightPercent;

                Vector2 newAnchorPosition = Vector2.zero;
                newAnchorPosition.y = newYPos;
                selfRectTransform.anchoredPosition = newAnchorPosition;
            }
            else
            {
                // Keyboard top is below the position of the input field, so leave screen anchored at zero
                selfRectTransform.anchoredPosition = Vector2.zero;
            }
        }
        else
        {
            // No focus or touchscreen invisible, set screen anchor to zero
            selfRectTransform.anchoredPosition = Vector2.zero;
        }
        InputFieldActive = false;

#endif

    }

}

public static class RectTransformExtension
{
    /// <summary>
    /// 移动端输入时将输入框自动移动到键盘上方，PC端没用
    /// </summary>
    /// <param name="input"></param>
    public static void SetAboveTouchScreenKeyboard(this InputField input)
    {

//#if UNITY_IOS || UNITY_ANDROID
        if (input == null)
            return;

        Canvas parentCanvas = input.gameObject.GetComponentInParent<Canvas>();
        if (parentCanvas == null)
            return;

        var focus = input.gameObject.GetComponent<InputfieldFocused>();
        if (focus == null)
            input.gameObject.AddComponent<InputfieldFocused>();

        var screen = parentCanvas.gameObject.GetComponent<InputfieldSlideScreen>();
        if (screen == null)
            parentCanvas.gameObject.AddComponent<InputfieldSlideScreen>();
//#endif

    }


    /// <summary>
    /// 获取RectTransform在屏幕控件的区域
    /// </summary>
    /// <param name="rectTransform"></param>
    /// <param name="canvas"></param>
    /// <returns></returns>
    public static Rect GetScreenRect(this RectTransform rectTransform, Canvas canvas)
    {
        Vector3[] corners = new Vector3[4];
        Vector3[] screenCorners = new Vector3[2];

        rectTransform.GetWorldCorners(corners);

        if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
        {
            screenCorners[0] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[1]);
            screenCorners[1] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[3]);
        }
        else
        {
            screenCorners[0] = RectTransformUtility.WorldToScreenPoint(null, corners[1]);
            screenCorners[1] = RectTransformUtility.WorldToScreenPoint(null, corners[3]);
        }

        screenCorners[0].y = Screen.height - screenCorners[0].y;
        screenCorners[1].y = Screen.height - screenCorners[1].y;

        return new Rect(screenCorners[0], screenCorners[1] - screenCorners[0]);
    }

}

