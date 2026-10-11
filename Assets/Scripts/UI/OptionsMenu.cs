using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour, GameManagerHelper.IMenuScene
{
    public static OptionsMenu Instance { get; private set; }
    [SerializeField]
    public Dictionary<SliderManager, AudioManager.TypeSound> volumeSliders = new Dictionary<SliderManager, AudioManager.TypeSound>();
    public RowSelector languageRowSelector;
    public Toggle muteToggle;
    public ButtonSelector defaultButton;
    public GameObject homeButton;
    public GameObject resetButton;
    public InputAction unloadOptionsMenuButton;
    public bool isMenuActive;
    public GameManagerHelper gameManagerHelper;
    public Animator animator;
    public CircleWipe circleWipe;
    bool finishEndAnimation;
    void Awake()
    {
        _ = InitMenu();
    }
    public async Awaitable InitMenu()
    {
        try
        {
            Instance = this;
            GameManager.Instance.isPause = true;
            InitializeVolumeMenu();
            InitializeDisplayMenu();
            gameManagerHelper.sceneData = this;
            unloadOptionsMenuButton.started += UnloadOptionsMenu;
            defaultButton.InitializeDefaultButton();
            if (SceneManager.GetSceneByName("HomeScene").isLoaded)
            {
                homeButton.SetActive(false);
                resetButton.SetActive(false);
            }
            await Awaitable.NextFrameAsync();
            await circleWipe.Play(1);
            isMenuActive = true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public void InitializeDisplayMenu()
    {
        languageRowSelector.InitializeData(TransformLanguageRowSelectorData(out int currentKey), currentKey);
    }
    RowSelector.RowData[] TransformLanguageRowSelectorData(out int currentKey)
    {
        List<RowSelector.RowData> rowData = new List<RowSelector.RowData>();
        foreach (GameData.TypeLanguage row in Enum.GetValues(typeof(GameData.TypeLanguage)))
        {
            rowData.Add(new RowSelector.RowData((int)row, row.ToString()));
        }
        currentKey = (int)GameData.Instance.systemDataInfo.configurationsInfo.currentLanguage;
        return rowData.ToArray();
    }
    public void ChangeLanguage(RowSelector.RowData rowData)
    {
        GameData.Instance.ChangeLanguage((GameData.TypeLanguage)rowData.key);
    }
    public void InitializeVolumeMenu()
    {
        foreach (KeyValuePair<SliderManager, AudioManager.TypeSound> slider in volumeSliders)
        {
            slider.Key.slider.value = GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.volumes[slider.Value];
            slider.Key.isInitialice = true;
        }
        muteToggle.isOn = GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.isMute;
    }
    public void SetSoundValue(SliderManager sliderManager)
    {
        GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.volumes[volumeSliders[sliderManager]] = sliderManager.slider.value;
        AudioManager.Instance.SetAudioMixerData();
    }
    public void SetMute()
    {
        if (!isMenuActive) return;
        GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.isMute = !GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.isMute;
        muteToggle.isOn = GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.isMute;
        AudioManager.Instance.SetAudioMixerData();
    }
    public void UnloadOptionsMenu(InputAction.CallbackContext context) { HandleUnloadOptionsMenu(); }
    public void HandleUnloadOptionsMenu() { gameManagerHelper.UnloadMenuScene(1); }
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
