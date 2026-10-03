using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
[CreateAssetMenu(fileName = "LevelsDB", menuName = "ScriptableObjects/DB/LevelsDB", order = 1)]
public class LevelsDBSO : ScriptableObject
{
    [SerializeField]
    public Dictionary<int, LevelData> data = new Dictionary<int, LevelData>();
    public LevelData CloneLevelData(int id)
    {
        if (data.ContainsKey(id))
        {
            LevelData original = data[id];
            LevelData clone = new LevelData
            {
                id = original.id,
                scene = original.scene,
                levelName = original.levelName,
                isUnlock = original.isUnlock,
                isPass = original.isPass,
                time = original.time,
                starsTime = (float[])original.starsTime.Clone()
            };
            return clone;
        }
        else
        {
            Debug.LogError($"Level with ID {id} does not exist in the database.");
            return null;
        }
    }
    [Serializable]
    public class LevelData
    {
        public int id;
        public string scene;
        public string levelName;
        public bool isUnlock = false;
        public bool isPass = false;
        public float time = 0;
        public float[] starsTime = new float[3];
        public bool[] GetStarsStatus()
        {
            bool[] starsStatus = new bool[3];
            for (int i = 0; i < starsTime.Length; i++)
            {
                if (time <= starsTime[i])
                {
                    starsStatus[i] = true;
                }                
            }
            return starsStatus;
        }
    }
    [NaughtyAttributes.Button]
    public void SetLevelsId()
    {
        foreach (KeyValuePair<int, LevelData> levelData in data)
        {
            levelData.Value.id = levelData.Key;
        }
    }
    [NaughtyAttributes.Button]
    public void SetLevelsNameById()
    {
        foreach (KeyValuePair<int, LevelData> levelData in data)
        {
            levelData.Value.levelName = levelData.Key.ToString();
        }
    }
}
