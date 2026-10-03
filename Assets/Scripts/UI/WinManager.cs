using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WinManager : MonoBehaviour, GameManagerHelper.IMenuScene
{
    public GameManagerHelper gameManagerHelper;
    public Animator animator;
    public CircleWipe circleWipe;
    bool finishEndAnimation;
    public TMP_Text levelName;
    void Awake()
    {
        _ = InitMenu();
    }
    async Awaitable InitMenu()
    {
        levelName.text = GameData.Instance.levelsDBSO.data[GameManager.Instance.currentLevelId].levelName;
        gameManagerHelper.sceneData = this;
        await Awaitable.NextFrameAsync();
        await circleWipe.Play(1);
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