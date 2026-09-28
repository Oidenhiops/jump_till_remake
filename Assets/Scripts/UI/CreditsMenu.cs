using UnityEngine;
using UnityEngine.InputSystem;

public class CreditsMenu : MonoBehaviour, GameManagerHelper.IMenuScene
{
    public InputAction unloadCreditsMenuButton;
    public GameManagerHelper gameManagerHelper;
    public Animator animator;
    public CircleWipe circleWipe;
    bool finishEndAnimation;
    void Awake()
    {
        _ = InitMenu();
    }
    async Awaitable InitMenu()
    {
        gameManagerHelper.sceneData = this;
        unloadCreditsMenuButton.started += UnloadCreditsMenu;
        await Awaitable.NextFrameAsync();
        await circleWipe.Play(1);
    }
    public void UnloadCreditsMenu(InputAction.CallbackContext context) { HandleUnloadCreditsMenu(); }
    public void HandleUnloadCreditsMenu() { gameManagerHelper.UnloadMenuScene(2); }
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
