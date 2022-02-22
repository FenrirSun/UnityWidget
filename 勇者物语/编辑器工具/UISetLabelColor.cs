using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

public class UISetLabelColor : EditorWindow {

    public enum FontType
    {
        普通,
        粗体,
        斜体,
        粗体_斜体,
    }
    [MenuItem("TNN/设置字体颜色")]
    static void SetColor() 
    {
        EditorWindow.GetWindow<UISetLabelColor>();
    }

    private string mainColor = "";
    private string outLineColor = "";
    private int fontSize = 24;
    private FontType fontType = FontType.普通;
    private int outlineSize = 2;

    private bool isSetMianColor = false;
    private bool isSetOutLine = false;
    private bool isSetFontSize = false;
    private bool isSetFontType = false;
    void OnGUI()
    {

        EditorGUILayout.BeginHorizontal();
        //isSetMianColor = EditorGUILayout.Toggle( isSetMianColor);
        mainColor = EditorGUILayout.TextField("主颜色：", mainColor);
        if (Selection.activeGameObject != null)
        {
            if(mainColor.Length == 6)
            {
                string str = "(";
                str += Convert.ToInt32(mainColor.Substring(0, 2), 16).ToString();
                str += ",";
                str += Convert.ToInt32(mainColor.Substring(2, 2), 16).ToString();
                str += ",";
                str += Convert.ToInt32(mainColor.Substring(4, 2), 16).ToString();
                str += ")";
                str += "(";
                str += ((float)Convert.ToInt32(mainColor.Substring(0, 2), 16)) / 255.0f;
                str += ",";
                str += ((float)Convert.ToInt32(mainColor.Substring(2, 2), 16)) / 255.0f;
                str += ",";
                str += ((float)Convert.ToInt32(mainColor.Substring(4, 2), 16)) / 255.0f;
                str += ")";
                EditorGUILayout.LabelField(str);
            }
            if (GUILayout.Button("设置"))
            {
                O_UILabel label = Selection.activeGameObject.GetComponent<O_UILabel>();
                if (label != null)
                {
                    float r = ((float)Convert.ToInt32(mainColor.Substring(0, 2), 16)) / 255.0f;
                    float g = ((float)Convert.ToInt32(mainColor.Substring(2, 2), 16)) / 255.0f;
                    float b = ((float)Convert.ToInt32(mainColor.Substring(4, 2), 16)) / 255.0f;
                    label.color = new Color(r, g, b, 1);
                    Debug.Log("Color ->r:" + Convert.ToInt32(mainColor.Substring(0, 2), 16) + "g:" + Convert.ToInt32(mainColor.Substring(2, 2), 16) + "b:" + Convert.ToInt32(mainColor.Substring(4, 2), 16));

                }
                //label.MakePixelPerfect();
            }
            if(GUILayout.Button("COPY COLOR"))
            {
                if (mainColor.Length == 6)
                {
                    string str = "";
                    str += "(";
                    str += ((float)Convert.ToInt32(mainColor.Substring(0, 2), 16)) / 255.0f;
                    str += "f,";
                    str += ((float)Convert.ToInt32(mainColor.Substring(2, 2), 16)) / 255.0f;
                    str += "f,";
                    str += ((float)Convert.ToInt32(mainColor.Substring(4, 2), 16)) / 255.0f;
                    str += "f)";

                    EditorGUIUtility.systemCopyBuffer = str;

                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        //isSetOutLine = EditorGUILayout.Toggle(isSetOutLine);
        outLineColor = EditorGUILayout.TextField("效果颜色：", outLineColor);
        //outlineSize = EditorGUILayout.IntField("效果像素：", outlineSize);
        if (Selection.activeGameObject != null)
        {
            if (GUILayout.Button("设置"))
            {
                O_UILabel label = Selection.activeGameObject.GetComponent<O_UILabel>();
                //if (outlineSize == 0)
                //{
                //    label.effectStyle = O_UILabel.Effect.None;
                //}
                //else
                //{
                //    label.effectStyle = O_UILabel.Effect.Outline;
                //}
                float r = ((float)Convert.ToInt32(outLineColor.Substring(0, 2), 16)) / 255.0f;
                float g = ((float)Convert.ToInt32(outLineColor.Substring(2, 2), 16)) / 255.0f;
                float b = ((float)Convert.ToInt32(outLineColor.Substring(4, 2), 16)) / 255.0f;
                label.effectColor = new Color(r, g, b, 1);
                Debug.Log("Effect Color ->r:" + Convert.ToInt32(outLineColor.Substring(0, 2), 16) + "g:" + Convert.ToInt32(outLineColor.Substring(2, 2), 16) + "b:" + Convert.ToInt32(outLineColor.Substring(4, 2), 16));

                //label.effectDistance = new Vector2(outlineSize * 0.5f, outlineSize * 0.5f);

                //label.MakePixelPerfect();
            }
        }
        EditorGUILayout.EndHorizontal();
        return;
        EditorGUILayout.BeginHorizontal();
        //isSetFontSize = EditorGUILayout.Toggle( isSetFontSize);
        fontSize = EditorGUILayout.IntField("字体大小：", fontSize);
        if (Selection.activeGameObject != null)
        {
            if (GUILayout.Button("设置"))
            {
                UILabel label = Selection.activeGameObject.GetComponent<UILabel>();
                label.fontSize = fontSize;

                label.MakePixelPerfect();
            }
        }
        EditorGUILayout.EndHorizontal();

        //EditorGUILayout.BeginHorizontal();
        ////isSetFontType = EditorGUILayout.Toggle( isSetFontType);
        //fontType = (FontType)EditorGUILayout.EnumPopup("字体类型：", fontType);
        //if (Selection.activeGameObject != null)
        //{
        //    if (GUILayout.Button("设置"))
        //    {
        //        UILabel label = Selection.activeGameObject.GetComponent<UILabel>();

        //        switch (fontType)
        //        {
        //            case FontType.普通:
        //                {
        //                    label.text = ClearString(label.text);
        //                    break;
        //                }
        //            case FontType.粗体:
        //                {
        //                    label.text = "[b]" + ClearString(label.text) + "[/b]";
        //                    break;
        //                }
        //            case FontType.斜体:
        //                {
        //                    label.text = "[i]" + ClearString(label.text) + "[/i]";
        //                    break;
        //                }
        //            case FontType.粗体_斜体:
        //                {
        //                    label.text = "[b][i]" + ClearString(label.text) + "[/b][/i]";
        //                    break;
        //                }
        //        }
        //        label.MakePixelPerfect();
        //    }
        //}
        //EditorGUILayout.EndHorizontal();
        
        if (Selection.activeGameObject != null)
        {
            if (GUILayout.Button("设置"))
            {
                UILabel label = Selection.activeGameObject.GetComponent<UILabel>();
                if (label != null)
                {
                    float r = ((float)Convert.ToInt32(mainColor.Substring(0, 2), 16)) / 255.0f;
                    float g = ((float)Convert.ToInt32(mainColor.Substring(2, 2), 16)) / 255.0f;
                    float b = ((float)Convert.ToInt32(mainColor.Substring(4, 2), 16)) / 255.0f;
                    label.color = new Color(r, g, b, 1);
                    Debug.Log("Color ->r:" + Convert.ToInt32(mainColor.Substring(0, 2), 16) + "g:" + Convert.ToInt32(mainColor.Substring(2, 2), 16) + "b:" + Convert.ToInt32(mainColor.Substring(4, 2), 16));

                    if (outlineSize == 0)
                    {
                        label.effectStyle = UILabel.Effect.None;
                    }
                    else
                    {
                        label.effectStyle = UILabel.Effect.Outline;

                        r = ((float)Convert.ToInt32(outLineColor.Substring(0, 2), 16)) / 255.0f;
                        g = ((float)Convert.ToInt32(outLineColor.Substring(2, 2), 16)) / 255.0f;
                        b = ((float)Convert.ToInt32(outLineColor.Substring(4, 2), 16)) / 255.0f;
                        label.effectColor = new Color(r, g, b, 1);
                        Debug.Log("Effect Color ->r:" + Convert.ToInt32(outLineColor.Substring(0, 2), 16) + "g:" + Convert.ToInt32(outLineColor.Substring(2, 2), 16) + "b:" + Convert.ToInt32(outLineColor.Substring(4, 2), 16));

                        label.effectDistance = new Vector2(outlineSize * 0.5f, outlineSize * 0.5f);
                    }


                    label.fontSize = fontSize;

                    switch (fontType)
                    {
                        case FontType.普通:
                            {
                                label.text = ClearString(label.text);
                                break;
                            }
                        case FontType.粗体:
                            {
                                label.text = "[b]" + ClearString(label.text) + "[/b]";
                                break;
                            }
                        case FontType.斜体:
                            {
                                label.text = "[i]" + ClearString(label.text) + "[/i]";
                                break;
                            }
                        case FontType.粗体_斜体:
                            {
                                label.text = "[b][i]" + ClearString(label.text) + "[/b][/i]";
                                break;
                            }
                    }

                    label.MakePixelPerfect();
                }
            }
        }
    }

    string ClearString(string str)
    {
        if(str.StartsWith("[b]"))
        {
            str = str.Substring(3, str.Length - 7);
        }
        if (str.StartsWith("[i]"))
        {
            str = str.Substring(3, str.Length - 7);
        }
        if (str.StartsWith("[i]"))
        {
            str = str.Substring(3, str.Length - 7);
        }
        if (str.StartsWith("[b]"))
        {
            str = str.Substring(3, str.Length - 7);
        }

        return str;
    }
}
