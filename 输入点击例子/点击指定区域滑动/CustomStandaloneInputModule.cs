using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomStandaloneInputModule : StandaloneInputModule {
    public Dictionary<int, PointerEventData> GetPointerData() {
        return m_PointerData;
    }

    public PointerEventData GetPointerDataTouch(int fingerId) {
        if (m_PointerData.ContainsKey(fingerId)) {
            return m_PointerData[fingerId];
        } else {
            return null;
        }
    }

    public PointerEventData GetPointerDataTouch0() {
#if UNITY_EDITOR || UNITY_STANDALONE
        var key = kMouseLeftId;
#else
        if (Input.touchCount <= 0) {
            return null;
        }
        var key = Input.GetTouch(0).fingerId;
#endif
        if (m_PointerData.ContainsKey(key)) {
            return m_PointerData[key];
        } else {
            return null;
        }
    }
}
