using UnityEngine;
using UnityEngine.InputSystem;

public class CreditsMenu : MonoBehaviour, GameManagerHelper.IMenuScene
{
    public InputAction unloadCreditsMenuButton;
    public GameManagerHelper gameManagerHelper;
    public Animator animator;
    void Awake()
    {
        gameManagerHelper.sceneData = this;
        unloadCreditsMenuButton.started += UnloadCreditsMenu;
    }
    public void UnloadCreditsMenu(InputAction.CallbackContext context) { HandleUnloadCreditsMenu(); }
    public void HandleUnloadCreditsMenu() { gameManagerHelper.UnloadMenuScene(2); }
    public void PlayEndAnimation()
    {
        animator.SetTrigger("exit");
    }

    public bool AnimationEnded()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("MenuExit") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1;
    }
}
