using UnityEngine;
using UnityEngine.InputSystem;

public class LevelsManager : MonoBehaviour, GameManagerHelper.IMenuScene
{
    public ScrollPage scrollPage;
    public Transform container;
    public ScrollPage.ScrollPageData scrollPageData;
    public GameManagerHelper gameManagerHelper;
    public InputAction unloadLevelSelectorButton;
    public Animator animator;
    public CircleWipe circleWipe;
    bool finishEndAnimation;
    void Awake()
    {
        _ = InitMenu();
    }
    async Awaitable InitMenu()
    {
        InitLevelsData();
        gameManagerHelper.sceneData = this;
        unloadLevelSelectorButton.started += UnloadLevelSelectorMenu;
        await Awaitable.NextFrameAsync();
        await circleWipe.Play(1);
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
    public void UnloadLevelSelectorMenu(InputAction.CallbackContext context) { HandleUnloadLevelSelectorMenu(); }
    public void HandleUnloadLevelSelectorMenu() { gameManagerHelper.UnloadMenuScene(9); }
    public void PlayEndAnimation()
    {
        _ = EndAnimationHandle();
    }
    async Awaitable EndAnimationHandle()
    {
        animator.SetTrigger("exit");
        await circleWipe.Play(0);
        finishEndAnimation = true;
    }
    public bool AnimationEnded()
    {
        return finishEndAnimation;
    }
}
