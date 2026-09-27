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
    public ButtonSelector displayButton;
    public GameObject homeButton;
    public InputAction unloadOptionsMenuButton;
    public bool isMenuActive;
    public GameManagerHelper gameManagerHelper;
    public Animator animator;
    void Awake()
    {
        Instance = this;
        InitializeVolumeMenu();
        InitializeDisplayMenu();
        gameManagerHelper.sceneData = this;
        unloadOptionsMenuButton.started += UnloadOptionsMenu;
        displayButton.InitializeDefaultButton();
        if (SceneManager.GetSceneByName("MainScene").isLoaded) homeButton.SetActive(false);
        isMenuActive = true;
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
        animator.SetTrigger("exit");
    }

    public bool AnimationEnded()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("MenuExit") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1;
    }
}
