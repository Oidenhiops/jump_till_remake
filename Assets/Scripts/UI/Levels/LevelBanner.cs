using System;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LevelBanner : MonoBehaviour
{
    public GameObject levelData;
    public GameObject levelLock;
    public Button button;
    public TMP_Text levelName;
    public TMP_Text bestTime;
    public Image[] stars;
    public void InitializeBanner(LevelsDBSO.LevelData levelInfo)
    {
        if (levelInfo.isUnlock)
        {
            levelLock.SetActive(false);
            levelData.SetActive(true);
            button.interactable = true;
            levelName.text = levelInfo.levelName;
            if (levelInfo.time == 0)
            {
                bestTime.text = "N/A";
            }
            else
            {
                bool[] starsInfo = levelInfo.GetStarsStatus();
                for (int i = 0; i < starsInfo.Length; i++)
                {
                    stars[i].enabled = starsInfo[i];
                }
                bestTime.text = levelInfo.time.ToString("F2");
            }
        }
    }
}
