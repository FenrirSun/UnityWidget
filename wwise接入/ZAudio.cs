using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Sirenix.OdinInspector;
using UnityEngine.Networking;

public class ZAudio : Singleton<ZAudio>
{
    // event名和bank的映射表
    private Dictionary<string, string> _eventBankDic = new Dictionary<string, string>();
    // 已加载的bank信息
    [ShowInInspector, DisableInEditorMode]
    private Dictionary<AudioBankScenario, List<string>> loadedBanks = new Dictionary<AudioBankScenario, List<string>>();
    // 音效层信息
    [ShowInInspector, DisableInEditorMode]
    private Dictionary<AudioLayerEnum, AudioLayer> audioLayers = new Dictionary<AudioLayerEnum, AudioLayer>();
    // 启用调试信息
    public bool EnableDebug { get; set; } = false;
    public bool Inited = false;

    public IEnumerator Init() {
        // 初始化触发器和监听器
        audioLayers.Clear();
        foreach (AudioLayerEnum layer in Enum.GetValues(typeof(AudioLayerEnum))) {
            CheckLayer(layer);
        }

        // 读取映射表
        yield return LoadEventBankDict();

        // 初始化Bank
        loadedBanks.Clear();
        foreach (AudioBankScenario scenario in Enum.GetValues(typeof(AudioBankScenario))) {
            loadedBanks[scenario] = new List<string>();
        }

        while (!AkSoundEngine.IsInitialized()) {
            yield return null;
        }
        
        LoadDefaultBank();
        InitPool();
        AddListener();
        Inited = true;
    }

    private void AddListener() {
        var ec = GetEventComp();
        ec.Listen<GameEvents.OnGameStateChange>(evt =>
        {
            if (evt.state == GameState.HOME) {
                ChangeScenario(AudioBankScenario.IN_MAIN);
            } else if (evt.state == GameState.MATCH3) {
                ChangeScenario(AudioBankScenario.IN_LEVEL);
            }
        });
        // 这里设置音乐和音效开关只改变音量大小，方便恢复时自然过渡
        ec.Listen<ZSettingEvents.MusicEnabled>((evt) =>
        {
            audioLayers[AudioLayerEnum.BGM].isPlaying = evt.enabled;
            SetLayerVolume(AudioLayerEnum.BGM, evt.enabled ? 1f : 0f);
        });
        ec.Listen<ZSettingEvents.SoundEnabled>(evt =>
        {
            audioLayers[AudioLayerEnum.SOUND].isPlaying = evt.enabled;
            SetLayerVolume(AudioLayerEnum.SOUND, evt.enabled ? 1f : 0f);
        });
        ec.Listen<GameEvents.UpdateAudioByAd>(evt =>
        {
            if (evt.on) {
                // 恢复
                SetLayerVolume(AudioLayerEnum.BGM, audioLayers[AudioLayerEnum.BGM].isPlaying ? 1f : 0f);
                SetLayerVolume(AudioLayerEnum.SOUND, audioLayers[AudioLayerEnum.SOUND].isPlaying ? 1f : 0f);
            } else {
                // 关闭
                SetLayerVolume(AudioLayerEnum.BGM, 0f);
                SetLayerVolume(AudioLayerEnum.SOUND, 0f);
            }
        });
    }

    public void PlayEvent(string eventName, AudioLayerEnum layer = AudioLayerEnum.SOUND) {
        if (!IsSoundBankLoaded(eventName))
            return;

        AkSoundEngine.PostEvent(eventName, audioLayers[layer].emitter);
    }
    
    public void PlaySound(string soundName, AudioLayerEnum layer = AudioLayerEnum.SOUND) {
        if (string.IsNullOrEmpty(soundName)) {
            return;
        }

        CheckSoloLayer(layer, soundName);

        var eventName = GetPlayEventName(soundName);
        PlayEvent(eventName, layer);
    }

    [Obsolete("请配合音效使用Event代替, 在wwise中实现随机")]
    public void PlayRandomSound(IList<string> sounds) {
        if (ZGameSettings.Instance.SoundEnabled && sounds != null && sounds.Count > 0) {
            var s = sounds[UnityEngine.Random.Range(0, sounds.Count)];
            PlaySound(s);
        }
    }
    
    private void SetLayerVolume(AudioLayerEnum layer, float volume) {
        var layerInfo = audioLayers[layer];
        if (!GameUtils.FloatEqual(volume, layerInfo.volume)) {
            layerInfo.volume = volume;
            AkSoundEngine.SetGameObjectOutputBusVolume(layerInfo.emitter, layerInfo.listener, volume);
        }

        if (layer == AudioLayerEnum.SOUND) {
            SetSingleEmittersVolume(volume);
        }
    }

    // 在某一个GameObject上播放
    public void PlayEvent(string eventName, GameObject obj) {
        if (string.IsNullOrEmpty(eventName)) {
            return;
        }

        if (!IsSoundBankLoaded(eventName)) {
            return;
        }

        AkSoundEngine.PostEvent(eventName, obj);
        AkSoundEngine.SetListeners(obj,
            new[] { AkSoundEngine.GetAkGameObjectID(audioLayers[AudioLayerEnum.SOUND].listener) }, 1);
    }

    public void StopSound(string soundName, AudioLayerEnum layer) {
        if (string.IsNullOrEmpty(soundName)) {
            return;
        }

        var eventName = GetStopEventName(soundName);
        PlayEvent(eventName, layer);
    }

    public void StopSound(GameObject obj) {
        AkSoundEngine.StopAll(obj);
    }

    public void PlayButtonSound() {
        // PlaySound("btn_click");
    }
    
    public void Pause() {
        PlayEvent("pause");
    }

    public void Resume() {
        PlayEvent("resume");
    }

    public void StopAllSound() {
        AkSoundEngine.StopAll();
    }
    
    public void SetState(string stateGroup, string state) {
        AkSoundEngine.SetState(stateGroup, state);
    }

    public void SetSwitch(string group, string switchState, AudioLayerEnum layer) {
        AkSoundEngine.SetSwitch(group, switchState, audioLayers[layer].emitter);
    }

    public void SetTRPC(string group, float value) {
        AkSoundEngine.SetRTPCValue(group, value);
    }

    public static string GetPlayEventName(string soundName) {
        return "Play_" + soundName;
    }

    public static string GetStopEventName(string soundName) {
        return "Stop_" + soundName;
    }
    
    private void CheckLayer(AudioLayerEnum layerEnum) {
        if (!audioLayers.ContainsKey(layerEnum)) {
            var newLayer = new AudioLayer(layerEnum);
            newLayer.emitter = new GameObject($"emitter_{layerEnum.ToString()}");
            newLayer.emitter.transform.SetParent(transform);
            newLayer.emitter.transform.localPosition = Vector3.zero;
            newLayer.listener = new GameObject($"listener_{layerEnum.ToString()}");
            newLayer.listener.transform.SetParent(transform);
            newLayer.listener.transform.localPosition = Vector3.zero;
            audioLayers[layerEnum] = newLayer;
            
            AkAudioListener akAudioListenerBgm = newLayer.listener.AddComponent<AkAudioListener>();
            akAudioListenerBgm.SetIsDefaultListener(false);
            akAudioListenerBgm.listenerId = (int)layerEnum;
            AkSoundEngine.SetGameObjectOutputBusVolume(newLayer.emitter, newLayer.listener, newLayer.volume);
            AkSoundEngine.SetListeners(newLayer.emitter, new[] { AkSoundEngine.GetAkGameObjectID(newLayer.listener) }, 1);
        }
    }

    private void CheckSoloLayer(AudioLayerEnum layer, string sound) {
        if (!IsLayerSolo(layer))
            return;
        
        StopSound(audioLayers[layer].lastPlay, layer);
        if (audioLayers.ContainsKey(layer)) {
            audioLayers[layer].lastPlay = sound;
        }
    }

    // 是否为独奏层
    private bool IsLayerSolo(AudioLayerEnum layerEnum) {
        
        // 暂时不处理独奏，由wwise引擎来处理关闭原BGM
        // if (AudioConfig != null && AudioConfig.soloLayer != null) {
        //     return AudioConfig.soloLayer.Contains(layerEnum);
        // }

        return false;
    }

    private void Debug(string info) {
        if (EnableDebug) {
            ZDEBUG.ERROR("音频错误", info);
        }
    }

    #region SingleEmitterPool

    private float singleEmitterVolume;
    public GameObject emitterTemplate;
    private GameObjPool emitterPool;
    private List<AudioSingleEmitter> usingemitter;

    private void InitPool() {
        emitterTemplate = new GameObject($"singleEmitterTemplate"); 
        emitterTemplate.transform.SetParent(transform);
        emitterTemplate.transform.localPosition = Vector3.zero;
        emitterTemplate.AddComponent<AudioSingleEmitter>();
        
        var poolObj = new GameObject($"singleEmitterPool"); 
        poolObj.transform.SetParent(transform);
        poolObj.transform.localPosition = Vector3.zero;
        emitterPool = poolObj.AddComponent<GameObjPool>();

        usingemitter = new List<AudioSingleEmitter>();
    }
    
    public AudioSingleEmitter GetSingleEmitter() {
        var emitter = emitterPool.Fetch<AudioSingleEmitter>(emitterTemplate);
        usingemitter.Add(emitter);
        emitter.name = $"single_{usingemitter.Count}";
        emitter.transform.SetParent(transform);
        AkSoundEngine.SetGameObjectOutputBusVolume(emitter.gameObject, audioLayers[AudioLayerEnum.SOUND].listener, singleEmitterVolume);
        return emitter;
    }

    public void RecycleEmitter(AudioSingleEmitter emitter) {
        StopSound(emitter.gameObject);
        if (usingemitter.Contains(emitter)) {
            usingemitter.Remove(emitter);
        }
        emitterPool.Recycle(emitter);
    }

    public void SetSingleEmitterListener(AudioSingleEmitter emitter) {
        AkSoundEngine.SetListeners(emitter.gameObject,
            new[] { AkSoundEngine.GetAkGameObjectID(audioLayers[AudioLayerEnum.SOUND].listener) }, 1);
    }

    private void SetSingleEmittersVolume(float volume) {
        if (GameUtils.FloatEqual(volume, singleEmitterVolume))
            return;
        
        singleEmitterVolume = volume;
        var listener = audioLayers[AudioLayerEnum.SOUND].listener;
        foreach (var emitter in usingemitter) {
            AkSoundEngine.SetGameObjectOutputBusVolume(emitter.gameObject, listener, volume);
        }
    }
    
    #endregion
    
    #region Bank

    private ZAudioBankConfig _audioConfig;
    private ZAudioBankConfig AudioConfig
    {
        get
        {
            if (_audioConfig == null) {
                _audioConfig = Resources.Load<ZAudioBankConfig>($"Data/{ZAudioBankConfig.FileName}");
            }

            return _audioConfig;
        }
    }


    // bank和配置文件的根目录
    private string _basePath;

    private string BasePath
    {
        get
        {
            if (_basePath == null) {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                _basePath = Path.Combine(Application.streamingAssetsPath, "Audio/GeneratedSoundBanks/Mac/");
#elif UNITY_ANDROID
                _basePath = Path.Combine(Application.streamingAssetsPath, "Audio/GeneratedSoundBanks/Android/");
#elif UNITY_IOS
                _basePath = Path.Combine(Application.streamingAssetsPath, "Audio/GeneratedSoundBanks/iOS/");
#else
                _basePath = Path.Combine(Application.streamingAssetsPath, "Audio/GeneratedSoundBanks/Windows/");
#endif
            }

            return _basePath;
        }
    }

    // 切换场景，加载卸载对应场景的Bank
    public void ChangeScenario(AudioBankScenario scenario) {
        AkSoundEngine.StopAll();
        foreach (var banksKey in loadedBanks.Keys) {
            if (banksKey != AudioBankScenario.ALWAYS && banksKey != scenario) {
                UnLoadBankImpl(banksKey);
            } else if (banksKey == scenario) {
                LoadBankImpl(banksKey);
            }
        }
    }

    // 检查event对应的bank是否已加载
    private bool IsSoundBankLoaded(string eventName) {
        if (!Inited)
        {
            return false;
        }
        if (string.IsNullOrEmpty(eventName))
            return false;

        if (_eventBankDic.TryGetValue(eventName, out string bank)) {
            bool loaded = false;
            foreach (var banks in loadedBanks.Values) {
                if (banks.Contains(bank)) {
                    loaded = true;
                    break;
                }
            }

            if (loaded) {
                return true;
            }

            if (!eventName.StartsWith("Stop_")) {
                Debug($"bank 未加载，无法播放声音! event 名：{eventName}, 所属bank：{bank}");
            }
        } else {
            Debug($"未找到event 所属bank，请检查! event 名：{eventName}");
        }

        return false;
    }

    private void LoadDefaultBank() {
        LoadBankImpl(AudioBankScenario.ALWAYS);
    }

    private void LoadBankImpl(AudioBankScenario scenario) {
        if (AudioConfig == null) {
            Debug("AudioConfig 为空！");
            return;
        }
        foreach (var bankInfo in AudioConfig.bankInfos) {
            if (bankInfo == null || bankInfo.scenario != scenario)
                continue;
            if (loadedBanks[scenario].Contains(bankInfo.name))
                continue;
            AkBankManager.LoadBank(bankInfo.name, false, false);
            loadedBanks[scenario].Add(bankInfo.name);
        }
    }

    private void UnLoadBankImpl(AudioBankScenario scenario) {
        var banks = loadedBanks[scenario];
        if (banks.Count == 0)
            return;

        foreach (var bank in banks) {
            AkBankManager.UnloadBank(bank);
        }

        banks.Clear();
    }

    IEnumerator LoadEventBankDict() {
        var xmlPath = Path.Combine(BasePath + "SoundbanksInfo.xml");
        string rawText = null;
#if UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_EDITOR
            // macOS上UnityWebRequest不好使
            if (!File.Exists(xmlPath)) {
                Debug($"找不到音频配置文件！路径：{xmlPath}");
                yield break;
            }

            rawText = File.ReadAllText(xmlPath);
#else
            UnityWebRequest webRequest = UnityWebRequest.Get(xmlPath);

            yield return webRequest.SendWebRequest();

            if (!webRequest.isDone || webRequest.result != UnityWebRequest.Result.Success) {
                Debug($"找不到音频配置文件！路径：{xmlPath}");
                yield break;
            }

            rawText = webRequest.downloadHandler.text;
#endif
        if (string.IsNullOrEmpty(rawText)) {
            Debug("音频配置文件为空");
            yield break;
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(rawText);

        //首先获取xml中所有的SoundBank
        _eventBankDic.Clear();
        XmlNodeList soundBankList = xmlDoc.GetElementsByTagName("SoundBank");
        foreach (XmlNode node in soundBankList) {
            XmlNode bankNameNode = node.SelectSingleNode("ShortName");
            if (bankNameNode == null)
                continue;

            string bankName = bankNameNode.InnerText;
            //判断SingleNode存在与否,比如Init.bak就没这个
            XmlNode eventNode = node.SelectSingleNode("IncludedEvents");
            //拿到其中所有的event做一个映射
            XmlNodeList eventList = eventNode?.SelectNodes("Event");
            if (eventList != null) {
                foreach (XmlElement element in eventList) {
                    var eventName = element.Attributes["Name"].Value;
                    if (!_eventBankDic.ContainsKey(eventName)) {
                        _eventBankDic.Add(eventName, bankName);
                    }
                }
            }
        }
    }

    #endregion

    private class AudioLayer
    {
        public AudioLayerEnum layerType;
        public bool isPlaying;
        public float volume;
        public GameObject emitter;
        public GameObject listener;
        public string lastPlay;

        public AudioLayer(AudioLayerEnum layer) {
            layerType = layer;
            isPlaying = true;
            volume = 1;
        }
    }
}