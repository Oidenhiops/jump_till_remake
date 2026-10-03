using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.UI;

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }
    public GameDataInfo gameDataInfo = new GameDataInfo();
    public SystemDataInfo systemDataInfo = new SystemDataInfo();
    public Utils utils = new Utils();
    [SerializeField]
    public Dictionary<TypeLOCS, Dictionary<TypeLanguage, Dictionary<string, DialogData>>> locs = new Dictionary<TypeLOCS, Dictionary<TypeLanguage, Dictionary<string, DialogData>>>();
    public LevelsDBSO levelsDBSO;
    public SoundsDBSO soundsDBSO;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _ = LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public async Awaitable LoadData()
    {
        try
        {
            CheckFileExistance();
            LoadGameDataInfo();
            LoadSystemDataInfo();
            LoadLOCS();
            await Awaitable.NextFrameAsync();
            AudioManager.Instance.SetAudioMixerData(true);
            GameManager.Instance.StartCoroutine(AudioManager.Instance.FadeIn());
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public void LoadGameDataInfo()
    {
        ReadGameDataFromJson();
    }
    public void LoadSystemDataInfo()
    {
        systemDataInfo = ReadSystemDataFromJson();
    }
    void LoadLOCS()
    {
        foreach (TypeLOCS typeLoc in Enum.GetValues(typeof(TypeLOCS)))
        {
            foreach (TypeLanguage typeLanguage in Enum.GetValues(typeof(TypeLanguage)))
            {
                try
                {
                    string path = $"LOCS/{typeLanguage}/{typeLoc}";
                    TextAsset loc = Resources.Load<TextAsset>(path);
                    if (!locs.ContainsKey(typeLoc)) locs.Add(typeLoc, new Dictionary<TypeLanguage, Dictionary<string, DialogData>>());
                    locs[typeLoc].Add(typeLanguage, TransformCSV(loc));
                }
                catch
                {
                    Debug.LogError($"Not found {typeLoc} in {typeLanguage}");
                }
            }
        }
    }
    Dictionary<string, DialogData> TransformCSV(TextAsset textAsset)
    {
        string[] lines = textAsset.text.Split('\n');
        List<string[]> textData = new List<string[]>();
        foreach (string line in lines)
        {
            List<string> columns = new List<string>(line.Split(';'));
            columns.RemoveAt(columns.Count - 1);
            textData.Add(columns.ToArray());
        }
        Dictionary<string, DialogData> data = new Dictionary<string, DialogData>();
        foreach (string[] text in textData)
        {
            data.Add(text[0], new DialogData { dialog = text[1], description = text[2] });
        }
        return data;
    }
    public DialogData GetDialog(int id, TypeLOCS typeLOCS)
    {
        if (locs.ContainsKey(typeLOCS) && locs[typeLOCS].ContainsKey(systemDataInfo.configurationsInfo.currentLanguage) && locs[typeLOCS][systemDataInfo.configurationsInfo.currentLanguage].ContainsKey(id.ToString()))
        {
            return new DialogData
            {
                dialog = locs[typeLOCS][systemDataInfo.configurationsInfo.currentLanguage][id.ToString()].dialog,
                description = locs[typeLOCS][systemDataInfo.configurationsInfo.currentLanguage][id.ToString()].description
            };
        }
        return new DialogData
        {
            description = $"NTF {typeLOCS}: {id}",
            dialog = $"NTF {typeLOCS}: {id}"
        };
    }
    public void ChangeLanguage(TypeLanguage language)
    {
        systemDataInfo.configurationsInfo.currentLanguage = language;
        SaveSystemData();
    }
    public void SetStartingData()
    {
        systemDataInfo = new SystemDataInfo();
        systemDataInfo.configurationsInfo.currentLanguage = TypeLanguage.English;
        SetStartingDataSound();
        gameDataInfo = new GameDataInfo();
        gameDataInfo.levels.Add(0, levelsDBSO.CloneLevelData(0));
        gameDataInfo.levels[0].isUnlock = true;
        SaveGameData();
        _ = GameManager.Instance.LoadMenuScene(GameManager.TypeScene.LanguageSelectorScene);
    }
    void SetStartingDataSound()
    {
        systemDataInfo.configurationsInfo.soundConfiguration.volumes.Add(AudioManager.TypeSound.MASTER, 50);
        systemDataInfo.configurationsInfo.soundConfiguration.volumes.Add(AudioManager.TypeSound.BGM, 50);
        systemDataInfo.configurationsInfo.soundConfiguration.volumes.Add(AudioManager.TypeSound.SFX, 50);
    }
    [NaughtyAttributes.Button]
    public void SaveGameData()
    {
        WriteGameDataToJson();
    }
    [NaughtyAttributes.Button]
    public void SaveSystemData()
    {
        WriteSystemDataToJson();
    }
    void CheckFileExistance()
    {
        if (!File.Exists(DataPath(TypeSaveData.SystemDataInfo)))
        {
            File.Create(DataPath(TypeSaveData.SystemDataInfo)).Close();
            SetStartingData();
            string gameDataString = JsonUtility.ToJson(gameDataInfo);
            string systemDataString = JsonUtility.ToJson(systemDataInfo);
            File.WriteAllText(DataPath(TypeSaveData.SystemDataInfo), gameDataString);
            File.WriteAllText(DataPath(TypeSaveData.SystemDataInfo), systemDataString);
        }
    }
    GameDataInfo ReadGameDataFromJson()
    {
        string dataString;
        string jsonFilePath = DataPath(TypeSaveData.GameDataInfo);
        dataString = File.ReadAllText(jsonFilePath);
        gameDataInfo = JsonUtility.FromJson<GameDataInfo>(dataString);
        return gameDataInfo;
    }
    SystemDataInfo ReadSystemDataFromJson()
    {
        string dataString;
        string jsonFilePath = DataPath(TypeSaveData.SystemDataInfo);
        dataString = File.ReadAllText(jsonFilePath);
        systemDataInfo = JsonUtility.FromJson<SystemDataInfo>(dataString);
        return systemDataInfo;
    }
    public void WriteGameDataToJson()
    {
        string jsonFilePath = DataPath(TypeSaveData.GameDataInfo);
        string dataString = JsonUtility.ToJson(gameDataInfo);
        File.WriteAllText(jsonFilePath, dataString);
    }
    public void WriteSystemDataToJson()
    {
        string jsonFilePath = DataPath(TypeSaveData.SystemDataInfo);
        string dataString = JsonUtility.ToJson(systemDataInfo);
        File.WriteAllText(jsonFilePath, dataString);
    }
    string DataPath(TypeSaveData typeSaveData)
    {
        return Path.Combine(Application.persistentDataPath, typeSaveData + ".json");
    }
    [Serializable]
    public class GameDataInfo
    {
        [SerializeField]
        public Dictionary<int, LevelsDBSO.LevelData> levels = new Dictionary<int, LevelsDBSO.LevelData>();
    }
    [Serializable]
    public class ConfigurationsInfo
    {
        public TypeLanguage _currentLanguage;
        public Action<TypeLanguage> OnLanguageChange;
        public TypeLanguage currentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    OnLanguageChange?.Invoke(_currentLanguage);
                }
            }
        }
        public SoundConfiguration soundConfiguration = new SoundConfiguration();
    }
    [Serializable]
    public class SoundConfiguration
    {
        public bool isMute = false;
        [SerializeField]
        public Dictionary<AudioManager.TypeSound, float> volumes = new Dictionary<AudioManager.TypeSound, float>();
    }
    [Serializable]
    public class SystemDataInfo
    {
        public int currentGameDataIndex = 0;
        public ConfigurationsInfo configurationsInfo = new ConfigurationsInfo();
    }
    [Serializable]
    public class DialogData
    {
        public string dialog;
        public string description;
    }
    [Serializable]
    public class Utils
    {
        [SerializeField]
        public Dictionary<string, Sprite> systemImages = new Dictionary<string, Sprite>();
        [SerializeField]
        public Dictionary<string, Color> systemColors = new Dictionary<string, Color>();
        public Color GetSystemColor(string key)
        {
            return systemColors.ContainsKey(key) ? systemColors[key] : Color.magenta;
        }
        [SerializeField]
        public Dictionary<string, ColorBlock> buttonsStyle = new Dictionary<string, ColorBlock>();
        [SerializeField]
        public Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
    }
    public enum TypeLanguage
    {
        English = 0,
        Español = 1,
    }
    public enum TypeLOCS
    {
        System = 0,
        Dialogs = 1,
        Items = 2,
        Skills = 3,
        Chars = 4
    }
    public enum TypeSaveData
    {
        None = 0,
        GameDataInfo = 1,
        SystemDataInfo = 2
    }
}