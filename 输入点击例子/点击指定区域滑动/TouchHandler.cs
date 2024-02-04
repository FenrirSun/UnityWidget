using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchHandler : ZComponentBase {
    private bool touching = false;
    private int fingerId;
    private Vector2 beginPos = Vector2.zero;


    public event Action<Vector2> OnTouchBegan;
    public event Action<Vector2, Vector2> OnTouchMoved;
    public event Action<Vector2, Vector2> OnTouchEnded;


    public void Update() {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown (0)) {
         EmitTouchBegin(0, Input.mousePosition);
        } else if (Input.GetMouseButton (0)) {
            EmitTouchMove(Input.mousePosition);
        } else if (Input.GetMouseButtonUp (0)) {
            EmitTouchEnd(Input.mousePosition);
        }
#else
        for (int i = 0; i < Input.touchCount; i++) {
            var touch = Input.GetTouch(i);
            var phase = touch.phase;
            if (touching) {
                if (touch.fingerId == fingerId) {
                    switch (phase) {
                        case TouchPhase.Moved:
                            EmitTouchMove(touch.position);
                            break;
                        case TouchPhase.Ended:
                        case TouchPhase.Canceled:
                            EmitTouchEnd(touch.position);
                            break;
                    }
                }
            } else if (phase == TouchPhase.Began) {
                EmitTouchBegin(touch.fingerId, touch.position);
            }
        }
#endif
    }

    private bool Blocked(Vector2 pos) {
        var im = (CustomStandaloneInputModule)EventSystem.current.currentInputModule;
        var raw = im.GetPointerData();
        if (touching) {
            if (raw.TryGetValue(fingerId, out var evt)) {
                if (evt == null || evt.pointerEnter == null || !evt.pointerEnter.CompareTag("NotStopJoystick")) {
                    EmitTouchEnd(pos);
                    return true;
                }
            }
        } else {
            var allBlocked = true;
            foreach (var kv in raw) {
                if (kv.Value != null && kv.Value.pointerEnter != null && kv.Value.pointerEnter.CompareTag("NotStopJoystick")) {
                    allBlocked = false;
                    break;
                }
            }


            if (allBlocked) {
                EmitTouchEnd(pos);
                return true;
            }
        }
        return false;
    }

    void EmitTouchEnd(Vector2 pos) {
        if (touching && OnTouchEnded != null) {
            OnTouchEnded(pos, beginPos);
        }
        touching = false;
    }

    void EmitTouchMove(Vector2 pos) {
        if (Blocked(pos)) {
            return;
        }
        if (touching && OnTouchMoved != null) {
            OnTouchMoved(pos, beginPos);
        }
    }

   void EmitTouchBegin(int finger, Vector2 pos) {
        if (Blocked(pos)) {
            return;
        }
      touching = true;
        fingerId = finger;
      beginPos = pos;
        if (OnTouchBegan!= null) {
         OnTouchBegan(pos);
      }
   }

    protected bool IsTouching() {
        return touching;
    }
}