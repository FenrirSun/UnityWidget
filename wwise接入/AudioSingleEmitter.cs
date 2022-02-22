using System;
using UnityEngine;

/// <summary>
/// 单条音频播放源，可独立 播放 / 关闭 音频
/// 30 秒后会自动释放，如果是长时间的音频需要调一下 SetRecycleTime
/// </summary>
public class AudioSingleEmitter : MonoBehaviour, IReuseableGameObj
{
    public bool IsActive { get; set; }

    private float _autoRecycleTime;

    private void Awake() {
        IsActive = false;
        ZAudio.Instance.SetSingleEmitterListener(this);
    }

    public AudioSingleEmitter PlaySound(string soundName) {
        if (!IsActive || string.IsNullOrEmpty(soundName) || !gameObject) {
            return this;
        }

        var eventName = ZAudio.GetPlayEventName(soundName);
        ZAudio.Instance.PlayEvent(eventName, gameObject);
        return this;
    }

    public AudioSingleEmitter StopSound(string soundName) {
        if (!IsActive || string.IsNullOrEmpty(soundName) || !gameObject) {
            return this;
        }

        var eventName = ZAudio.GetStopEventName(soundName);
        ZAudio.Instance.PlayEvent(eventName, gameObject);
        return this;
    }

    public AudioSingleEmitter SetRecycleTime(float time) {
        _autoRecycleTime = time + Time.time;
        return this;
    }

    public void Free(float delayTime = 1f) {
        SetRecycleTime(delayTime);
    }

    private void Update() {
        if (IsActive) {
            if (Time.time > _autoRecycleTime) {
                IsActive = false;
                ZAudio.Instance.RecycleEmitter(this);
            }
        }
    }

    #region IReuseableGameObj - Impliment

    public GameObject Prefab { get; set; }

    public GameObject GameObject => gameObject;

    public void WillReuse() {
        // 30秒自动回收时间
        SetRecycleTime(30f);
        IsActive = true;
    }

    public void OnRecycle() { }

    // 这个时间是缓存池使用的时间
    public float RecycleTime { get; set; }

    #endregion
}