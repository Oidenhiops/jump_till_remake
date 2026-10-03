using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WinManager : MonoBehaviour, GameManagerHelper.IMenuScene
{
    public GameManagerHelper gameManagerHelper;
    public Animator animator;
    public CircleWipe circleWipe;
    bool finishEndAnimation;
    public TMP_Text levelName;
    public Image[] stars;
    void Awake()
    {
        _ = InitMenu();
    }
    async Awaitable InitMenu()
    {
        levelName.text = "Level " + GameData.Instance.levelsDBSO.data[GameManager.Instance.currentLevelId].levelName;
        gameManagerHelper.sceneData = this;
        await Awaitable.NextFrameAsync();
        await circleWipe.Play(1);
        await InitStars();
    }
    async Awaitable InitStars()
    {
        bool[] starsInfo = GameData.Instance.gameDataInfo.levels[GameManager.Instance.currentLevelId].GetStarsStatus();
        for (int i = 0; i < starsInfo.Length; i++)
        {
            if (starsInfo[i]) await HandleStarsAnimation(stars[i]);
        }
    }
    async Awaitable HandleStarsAnimation(Image starImage)
    {
        float elapsedTime = 0f;
        float duration = 0.5f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            starImage.fillAmount = Mathf.Clamp01(elapsedTime / duration);
            await Awaitable.NextFrameAsync();
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