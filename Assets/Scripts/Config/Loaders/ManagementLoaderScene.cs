using System;
using Unity.VisualScripting;
using UnityEngine;

public class ManagementLoaderScene : MonoBehaviour
{
    public static ManagementLoaderScene Instance { get; private set; }
    public Animator animator;
    public bool _finishLoad;
    public Action OnFinishOpenAnimation;
    public CanvasGroup canvasGroup;
    public CircleWipe circleWipe;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _ = OpenLoader();
    }
    public bool ValidateLoaderIsOnIdle()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("LoaderIdle");
    }
    public virtual async Awaitable ValidateChargeIsComplete()
    {
        await Awaitable.NextFrameAsync();
    }
    public async Awaitable CloseLoader()
    {
        SetBlockers(true);
        await circleWipe.Play(0);
        animator.Play("LoaderExit", 0, 0f);
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("LoaderExit"))
        {
            await Awaitable.NextFrameAsync();
        }
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
            await Awaitable.NextFrameAsync();
        }
    }
    public void SetBlockers(bool state)
    {
        canvasGroup.interactable = state;
        canvasGroup.blocksRaycasts = state;
    }
    public async Awaitable OpenLoader()
    {
        animator.Play("LoaderEnter", 0, 0f);
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("LoaderEnter"))
        {
            await Awaitable.NextFrameAsync();
        }
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
            await Awaitable.NextFrameAsync();
        }
        await circleWipe.Play(1);
        SetBlockers(false);
    }
}