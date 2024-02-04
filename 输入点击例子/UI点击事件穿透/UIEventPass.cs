using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIEventPass : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler {
    public GameObject exclusiveObj;
    public bool passClick = true;
    public bool passDownAndUp = true;
    public bool passEnterAndExit = true;
    
    private GameObject lastDownObj;

    //监听按下
    public void OnPointerDown(PointerEventData eventData) {
        if (!passDownAndUp)
            return;
        
        lastDownObj = PassEvent(eventData, ExecuteEvents.pointerDownHandler);
    }

    //监听抬起
    public void OnPointerUp(PointerEventData eventData) {
        if (!passDownAndUp)
            return;
        
        if (lastDownObj != null)
        {
            // 上一次按下的UI，抬起时也传递消息，防止在其他位置抬起导致事件传递不到
            ExecuteEvents.Execute(lastDownObj.gameObject, eventData, ExecuteEvents.pointerUpHandler);
            lastDownObj = null;
        }
        PassEvent(eventData, ExecuteEvents.pointerUpHandler);
    }

    //监听点击
    public void OnPointerClick(PointerEventData eventData) {
        if (!passClick)
            return;
        
        // PassEvent(eventData, ExecuteEvents.submitHandler);
        PassEvent(eventData, ExecuteEvents.pointerClickHandler);
    }

    // 监听进入，监听这个是因为如果后面是button的话不监听就没按下效果了
    public void OnPointerEnter(PointerEventData eventData) {
        if (!passEnterAndExit)
            return;
        
        PassEvent(eventData, ExecuteEvents.pointerEnterHandler);
    }

    // 监听退出
    public void OnPointerExit(PointerEventData eventData) {
        if (!passEnterAndExit)
            return;
        
        PassEvent(eventData,ExecuteEvents.pointerExitHandler);
    }

    //把事件传递给后面一个UI组件
    public GameObject PassEvent<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function)
        where T : IEventSystemHandler {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);
        GameObject current = data.pointerCurrentRaycast.gameObject;
        for (int i = 0; i < results.Count; i++) {
            if (results[i].gameObject == exclusiveObj) {
                return null;
            }
            if (current != results[i].gameObject
                && results[i].gameObject != gameObject) {
                //Debug.Log($"-------------{function.GetType().FullName}");
                ExecuteEvents.Execute(results[i].gameObject, data, function);
                //RaycastAll后ugui会自己排序，这里 return 的话只响应下面最近的一个组件
                return results[i].gameObject;
            }
        }

        return null;
    }
}