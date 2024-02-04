using System;
using UnityEngine;

public class SmoothMove : TouchHandler {
    public event Action<float> OnValueChangedX;
    public event Action<float> OnValueChangedY;
    public event Action OnTouchEnd;
    
    private Vector2 lastPos;

    private void Awake() {
        OnTouchBegan += TouchBegan;
        OnTouchMoved += TouchMoved;
        OnTouchEnded += TouchEnded;
    }
    
    private void TouchBegan(Vector2 pos) {
        lastPos = pos;
    } 
    
    private void TouchMoved(Vector2 pos, Vector2 begin) {
        var d =  lastPos - pos;
        OnValueChangedX?.Invoke(d.x);
        OnValueChangedY?.Invoke(d.y);
        lastPos = pos;
    }
    
    private void TouchEnded(Vector2 pos, Vector2 begin) {
        OnTouchEnd?.Invoke();
    }
}