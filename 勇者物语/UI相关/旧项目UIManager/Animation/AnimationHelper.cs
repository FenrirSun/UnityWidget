using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Code.External.Engine;

public class AnimationHelper : MonoBehaviour
{

    [System.Serializable]
    public class AnimationCurveValueItem
    {
        public string name;
        public AnimationCurve curve;
    }

    [SerializeField]
    public AnimationCurveValueItem[] items;

    private static Dictionary<string, AnimationCurve> _items;

    static AnimationHelper()
    {
        _items = new Dictionary<string, AnimationCurve>(System.StringComparer.InvariantCultureIgnoreCase);
    }
    public static AnimationCurve Get(string resType, string resName)
    {
        AnimationCurve curve;
        if (!_items.TryGetValue(resName, out curve))
        {
            curve = ((AnimationCurveAsset)Resources.Load(SplitName(resType,resName),typeof(AnimationCurveAsset))).Curve;
            _items[resName] = curve;
        }

        return curve;
    }
    public static AnimationCurve LineIn
    {
        get { return Get("Curve", "LineIn"); }
    }
    public static AnimationCurve BounceIn
    {
        get { return Get("Curve", "BounceIn"); }
    }
    public static AnimationCurve CubeIn
    {
        get { return Get("Curve", "CubeIn"); }
    }
    public static AnimationCurve CubeOut
    {
        get { return Get("Curve", "CubeOut"); }
    }
    public static AnimationCurve InOut
    {
        get { return Get("Curve", "InOut"); }
    }
    public static AnimationCurve BounceOutEnd
    {
        get { return Get("Curve", "BounceOutEnd"); }
    }

    public static string SplitName(string resType, string resName)
    {
        string str = string.Empty;

        str = string.Format("Extranal/{0}/{1}",resType,resName);

        return str;
    }
    public static float UIScaleRate
    {
        get
        {
            UIRoot root = (UIRoot)GameObject.FindObjectOfType(typeof(UIRoot));
            float screenScale = root.activeHeight / (float)Screen.height;
            return screenScale;
        }
    }

    public static float UIScaleAnimation(GameObject go, bool forward = true)
    {
        Vector3 from;
        Vector3 to;
        float duration = 0.3f;
        AnimationCurve curv;

        from = Vector3.one * 0.8f;
        to = Vector3.one;
        curv = BounceIn;

        return UIScaleAnimation(go, from, to, duration, curv, forward);
    }
    public static float UIScaleAnimation(GameObject go, Vector3 from, Vector3 to, float duration, bool forward = true)
    {
        return UIScaleAnimation(go, from, to, duration, LineIn, forward);
    }
    public static float UIScaleAnimation(GameObject go, Vector3 from, Vector3 to, float duration, AnimationCurve curve, bool forward = true)
    {
        return UIScaleAnimation(go, from, to, duration, LineIn,null, forward);


        if (!go)
            return 0;

        TweenScale anim;//= go.EnsureComponent<TweenScale>();

        anim = UITweener.Begin<TweenScale>(go, duration);
        // go.transform.localScale = from;

        anim.animationCurve = curve;
        anim.from = from;
        anim.to = to;
        anim.duration = duration;
        anim.style = UITweener.Style.Once;
        anim.eventReceiver = null;
        anim.callWhenFinished = null;
        anim.delay = StartedDelay;

        StartedDelay = 0;

        anim.Sample(0, false);

        if (duration <= 0f)
        {
            anim.Sample(1f, true);
            anim.enabled = false;
        }
        else
        {
            if (forward)
                anim.PlayForward();
            else
                anim.PlayReverse();
        }
        return duration;
    }

    public static float UIScaleAnimation(GameObject go, Vector3 from, Vector3 to, float duration, AnimationCurve curve, EventDelegate.Callback del, bool forward = true)
    {
        if (!go)
            return 0;

        TweenScale anim;//= go.EnsureComponent<TweenScale>();

        anim = UITweener.Begin<TweenScale>(go, duration);
        // go.transform.localScale = from;

        anim.animationCurve = curve;
        anim.from = from;
        anim.to = to;
        anim.duration = duration;
        anim.style = UITweener.Style.Once;
        anim.AddOnFinished(del);
        anim.eventReceiver = null;
        anim.callWhenFinished = null;
        anim.delay = StartedDelay;

        StartedDelay = 0;

        anim.Sample(0, false);

        if (duration <= 0f)
        {
            anim.Sample(1f, true);
            anim.enabled = false;
        }
        else
        {
            if (forward)
                anim.PlayForward();
            else
                anim.PlayReverse();
        }
        return duration;
    }
    public static float UIAlphaAnimation(GameObject go, float from, float to, float duration, bool forward = true)
    {
        return UIAlphaAnimation(go, ValueType.Absolute, from, ValueType.Absolute, to, duration, LineIn, forward);
    }
    public static float UIAlphaAnimation(GameObject go, float from, float to, float duration, AnimationCurve curve, bool forward = true)
    {
        return UIAlphaAnimation(go, ValueType.Absolute, from, ValueType.Absolute, to, duration, curve, forward);
    }
    public static float UIAlphaAnimation(GameObject go, ValueType fromType, float from, ValueType toType, float to, float duration, AnimationCurve curve, bool forward)
    {
        if (!go)
            return 0;
        TweenAlpha anim;// = go.EnsureComponent<TweenAlpha>();
        anim = UITweener.Begin<TweenAlpha>(go, duration);

        float originAlpha = 0;
        var rect = go.GetComponent<UIRect>();
        if (!rect)
            Debug.LogError("gameobject not UIRect");

        originAlpha = rect.alpha;

        if (fromType == ValueType.Relative)
        {
            from += originAlpha;
        }

        if (toType == ValueType.Relative)
        {
            to += originAlpha;
        }


        //rect.alpha = from;
        // anim.enabled = true;
        anim.duration = duration;
        anim.style = UITweener.Style.Once;
        anim.animationCurve = curve;
        anim.from = from;
        anim.to = to;
        anim.delay = StartedDelay;
        StartedDelay = 0;
        anim.Sample(0, false);

        if (duration <= 0f)
        {
            anim.Sample(1f, true);
            anim.enabled = false;
        }
        else
        {
            if (forward)
                anim.PlayForward();
            else
                anim.PlayReverse();
        }
        return duration;
    }
    public static float UIPositionAnimation(GameObject go, Vector3 from, Vector3 to, float duration)
    {
        return UIPositionAnimation(go, from, to, duration, LineIn, true);
    }
    public static float UIPositionAnimation(GameObject go, Vector3 from, Vector3 to, float duration, AnimationCurve curve, bool forward = true)
    {
        if (!go)
            return 0;

        TweenPosition anim = go.EnsureComponent<TweenPosition>();
        
        anim.enabled = true;
        anim.eventReceiver = null;
        anim.callWhenFinished = null;
        anim.style = UITweener.Style.Once;
        anim.duration = duration;
        anim.animationCurve = curve;
        anim.from = from;
        anim.to = to;
        anim.delay = StartedDelay;
        StartedDelay = 0;
        // go.transform.localPosition = from;
        anim.Sample(0, false);

        if (duration <= 0f)
        {
            anim.Sample(1f, true);
            anim.enabled = false;
        }
        else
        {
            if (forward)
                anim.PlayForward();
            else
                anim.PlayReverse();
        }
        return duration;
    }
    public static float UIRotationAnimation(GameObject go, Vector3 from, Vector3 to, float duration, AnimationCurve curve, bool forward = true)
    {
        if (!go)
            return 0;

        TweenRotation anim = go.EnsureComponent<TweenRotation>();

        anim.enabled = true;
        anim.eventReceiver = null;
        anim.callWhenFinished = null;
        anim.style = UITweener.Style.Once;
        anim.duration = duration;
        anim.animationCurve = curve;
        anim.from = from;
        anim.to = to;
        anim.delay = StartedDelay;
        StartedDelay = 0;
        // go.transform.localPosition = from;
        anim.Sample(0, false);

        if (duration <= 0f)
        {
            anim.Sample(1f, true);
            anim.enabled = false;
        }
        else
        {
            if (forward)
                anim.PlayForward();
            else
                anim.PlayReverse();
        }
        return duration;
    }
        //.ABSOLUTE, Animation.RELATIVE_TO_SELF, or Animation.RELATIVE_TO_PARENT.
    public enum ValueType
    {
        /// <summary>
        /// 绝对值
        /// </summary>
        Absolute = 0,
        /// <summary>
        /// 相对值
        /// </summary>
        Relative = 1,
        //RelativeToSelf = 1,
        RelativeToParent = 2,
    }
    public static float MaskPlayForward(GameObject go)
    {
        if (!go)
            return 0;

        return UIAlphaAnimation(go, ValueType.Relative, 0, ValueType.Absolute, 0.5f, 0.3f, LineIn, true);
    }
    public static float MaskPlayReverse(GameObject go)
    {
        if (!go)
            return 0;
        return UIAlphaAnimation(go, ValueType.Relative, 0, ValueType.Absolute, 0.005f, 0.3f, LineIn, false);
    }
    public static float BoxPlayForward(GameObject go)
    {
        if (!go)
            return 0;
        return UIScaleAnimation(go, Vector3.one * 0.5f, Vector3.one, 0.3f, AnimationHelper.BounceIn);
    }
    public static float BoxPlayForward(GameObject go, EventDelegate.Callback del)
    {
        if (!go)
            return 0;
        return UIScaleAnimation(go, Vector3.one * 0.5f, Vector3.one, 0.3f, AnimationHelper.BounceIn, del);
    }
    public static float BoxPlayReverse(GameObject go)
    {
        if (!go)
            return 0;
        return UIScaleAnimation(go, Vector3.one, Vector3.one * 0.3f, 0.3f, AnimationHelper.CubeIn);
    }
    public static float BoxPlayReverse(GameObject go, EventDelegate.Callback del)
    {
        if (!go)
            return 0;
        return UIScaleAnimation(go, Vector3.one, Vector3.one * 0.3f, 0.3f, AnimationHelper.CubeIn, del);
    }
    public static float UITopInPlay(GameObject go, float duration, AnimationCurve curve, bool forward = true)
    {
        if (!go)
            return 0;

        var bounds = NGUIMath.CalculateRelativeWidgetBounds(go.transform);//0
        Vector3 from = new Vector3(0, Screen.height * UIScaleRate * 0.5f + bounds.size.y * 0.5f, 0);
        from.y -= GetUIParentAbsPosition(go.transform).y;
        float t = UIPositionAnimation(go, from, go.transform.localPosition, duration, curve, forward);
        return t;
    }
    public static float UIBottomInPlay(GameObject go, float duration, AnimationCurve curve, bool forward = true)
    {
        if (!go)
            return 0;

        var bounds = NGUIMath.CalculateRelativeWidgetBounds(go.transform);//1
        Vector3 from = new Vector3(0, -(Screen.height * UIScaleRate * 0.5f + bounds.size.y * 0.5f), 0);
        from.y -= GetUIParentAbsPosition(go.transform).y;
        float t = UIPositionAnimation(go, from, go.transform.localPosition, duration, curve, forward);
        return t;
    }
    public static float UILeftInPlay(GameObject go, float duration, AnimationCurve curve, bool forward = true)
    {
        if (!go)
            return 0;

        var bounds = NGUIMath.CalculateRelativeWidgetBounds(go.transform);
        Vector3 from = new Vector3(-(Screen.width * UIScaleRate * 0.5f + bounds.size.x * 0.5f), 0, 0);

        from.x -= GetUIParentAbsPosition(go.transform).x;

        float t = UIPositionAnimation(go, from, go.transform.localPosition, duration, curve, forward);
        return t;
    }
    public static float UIRightInPlay(GameObject go, float duration, AnimationCurve curve, bool forward = true)
    {
        if (!go)
            return 0;

        var bounds = NGUIMath.CalculateRelativeWidgetBounds(go.transform);
        Vector3 from = new Vector3(Screen.width * UIScaleRate * 0.5f + bounds.size.x * 0.5f, 0, 0);
        from.x -= GetUIParentAbsPosition(go.transform).x;
        float t = UIPositionAnimation(go, from, go.transform.localPosition, duration, curve, forward);
        return t;
    }
    public static float StartedDelay;
    public static Vector3 GetUIAbsPosition(Transform t)
    {

        Vector3 pos = t.localPosition;
        t = t.parent;

        while (t.parent && t != t.root)
        {
            pos += t.localPosition;
            t = t.parent;
        }
        return pos;
    }
    public static Vector3 GetUIParentAbsPosition(Transform t)
    {
        if (!t.parent)
            return Vector3.zero;
        Vector3 pos = t.parent.localPosition;
        t = t.parent;

        while (t.parent && t != t.root)
        {
            pos += t.localPosition;
            t = t.parent;
        }
        return pos;
    }

}

