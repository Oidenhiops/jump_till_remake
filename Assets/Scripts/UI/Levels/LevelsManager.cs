using UnityEngine;

public class LevelsManager : MonoBehaviour
{
    public ScrollPage scrollPage;
    public Transform container;
    public ScrollPage.ScrollPageData scrollPageData;
    void Awake()
    {
        InitLevelsData();
    }
    void InitLevelsData()
    {
        const int levelsPerContainer = 10;
        int totalLevels = GameData.Instance.levelsDBSO.data.Count;
        int amountContainers = Mathf.CeilToInt(totalLevels / (float)levelsPerContainer);
        scrollPageData.pageSize = amountContainers;
        scrollPage.InitScrollPageData(scrollPageData);
        for (int c = 0; c < amountContainers; c++)
        {
            Transform levelContainer = Instantiate(GameData.Instance.utils.prefabs["LevelsContainer"], container).transform;

            for (int i = 0; i < levelsPerContainer; i++)
            {
                int levelIndex = c * levelsPerContainer + i;
                if (levelIndex >= totalLevels) break;

                LevelBanner levelBanner = Instantiate(GameData.Instance.utils.prefabs["LevelBanner"], levelContainer).GetComponent<LevelBanner>();
                levelBanner.InitializeBanner(GameData.Instance.levelsDBSO.data[levelIndex]);
            }
        }
    }
}
