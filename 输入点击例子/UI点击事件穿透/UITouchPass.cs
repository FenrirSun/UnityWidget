using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
 
public class UITouchPass : MonoBehaviour, IPointerClickHandler,
    IMoveHandler,IPointerDownHandler, IPointerUpHandler,IPointerEnterHandler,ISelectHandler, IDeselectHandler
    , ISubmitHandler, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler
{
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        PassEvent(eventData, ExecuteEvents.pointerClickHandler);
    }
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        PassEvent(eventData, ExecuteEvents.pointerDownHandler);
        if (Input.GetButtonDown("Submit"))
           ExecuteEvents.Execute(eventData.pointerCurrentRaycast.gameObject, eventData, ExecuteEvents.submitHandler);
    }
 
    public virtual void OnPointerUp(PointerEventData eventData)
    {
        PassEvent(eventData, ExecuteEvents.pointerUpHandler);
    }
 
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        PassEvent(eventData, ExecuteEvents.pointerEnterHandler);
    }
 
    public virtual void OnSelect(BaseEventData eventData)
    {
        PassEvent(eventData, ExecuteEvents.selectHandler);
    }
 
    public virtual void OnDeselect(BaseEventData eventData)
    {
        PassEvent(eventData, ExecuteEvents.deselectHandler);
    }
 
    public virtual void OnSubmit(BaseEventData eventData)
    {
        PassEvent(eventData, ExecuteEvents.submitHandler);
    }
 
    public virtual void OnMove(AxisEventData eventData)
    {
        PassEvent(eventData, ExecuteEvents.moveHandler);
    }
 
    GameObject CacheGameObject;
    public virtual void OnInitializePotentialDrag(PointerEventData eventData)
    {
        CacheGameObject = PassEvent(eventData, ExecuteEvents.initializePotentialDrag);
    }
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
         PassEvent(eventData, ExecuteEvents.beginDragHandler);
    }
    public virtual void OnDrag(PointerEventData eventData)
    {
        ExecuteEvents.Execute(CacheGameObject, eventData, ExecuteEvents.dragHandler);
    }
    public virtual void OnEndDrag(PointerEventData eventData)
    {
        ExecuteEvents.Execute(CacheGameObject, eventData, ExecuteEvents.endDragHandler);
        CacheGameObject = null;
    }
    public virtual void OnScroll(PointerEventData eventData)
    {
        ExecuteEvents.Execute(CacheGameObject, eventData, ExecuteEvents.scrollHandler);
    }
 
    List<RaycastResult> result = new List<RaycastResult>();
    GameObject PassEvent<T>(BaseEventData data, ExecuteEvents.EventFunction<T> function) where T : IEventSystemHandler
    {
        PointerEventData eventData = data as PointerEventData;
        var pointerGo = eventData.pointerCurrentRaycast.gameObject?? eventData.pointerDrag;
        EventSystem.current.RaycastAll(eventData, result);
        foreach (var item in result)
        {
            var go = item.gameObject;
            if (go != null && go != pointerGo)
            {
                var excuteGo = ExecuteEvents.GetEventHandler<T>(go);
                if (excuteGo)
                {
                    if (excuteGo.TryGetComponent<UITouchPass>(out var __))
                        return null;
                    ExecuteEvents.Execute(excuteGo, data, function);
                    return excuteGo;
                }
                else
                {
                    if(go.TryGetComponent<UnityEngine.UI.Graphic>(out var com))
                    {
                        if (com.raycastTarget) return null;
                    }
                }
            }
        }
        return null;
    }
}