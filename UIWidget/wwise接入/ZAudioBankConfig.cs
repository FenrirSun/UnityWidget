using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = FileName, menuName = "ScriptableObjects/ZAudioBankConfig", order = 3)]
public class ZAudioBankConfig: ScriptableObject
{
    public const string FileName = "ZAudioBankConfig";

    // [Title("独奏层")]
    // public List<AudioLayerEnum> soloLayer = new List<AudioLayerEnum>();
    
    [ListDrawerSettings(CustomAddFunction = "CreateNewBankInfo")]
    public List<BankInfo> bankInfos = new List<BankInfo>();
    
    private BankInfo CreateNewBankInfo()
    {
        BankInfo result = new BankInfo();
        result.name = string.Empty;
        result.scenario = AudioBankScenario.ALWAYS;
        
        return result;
    }

    [Serializable]
    public class BankInfo
    {
        [Title("bank 名称")]
        [HideLabel]
        public string name;
        [Title("bank 使用场景")]
        [EnumToggleButtons, HideLabel]
        public AudioBankScenario scenario;
    }
}

public enum AudioBankScenario
{
    ALWAYS,
    IN_MAIN,
    IN_LEVEL
}

public enum AudioLayerEnum
{
    BGM = 1,
    SOUND
}
