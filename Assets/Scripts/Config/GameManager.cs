using System;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public bool isWebGlBuild;
    public bool lockDevice = false;
    public bool _isPause;
    public bool isPause
    {
        get => _isPause;
        set
        {
            if (_isPause != value)
            {
                _isPause = value;
                Time.timeScale = _isPause ? 0 : 1;
            }
        }
    }
    public TypeDevice principalDevice;
    public TypeDevice _currentDevice;
    public event Action<TypeDevice, TypeDevice> OnDeviceChanged;
    public TypeDevice currentDevice
    {
        get => _currentDevice;
        set
        {
            if (_currentDevice != value)
            {
                _currentDevice = value;
                OnDeviceChanged?.Invoke(principalDevice, _currentDevice);
            }
        }
    }
    public bool _startGame;
    public Action<bool> OnStartGame;
    public InputAction pauseButton;
    string[] _excludedScenesForPause = { "CreditsScene", "HomeScene", "OptionsScene" };
    public int currentLevelId;
    public bool startGame
    {
        get => _startGame;
        set
        {
            if (_startGame != value)
            {
                _startGame = value;
                OnStartGame?.Invoke(_startGame);
            }
        }
    }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void OnEnable()
    {
        pauseButton.performed += PauseHandle;
        pauseButton.Enable();
    }
    void OnDisable()
    {
        pauseButton.performed -= PauseHandle;
        pauseButton.Disable();
    }
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        currentDevice = principalDevice;
        if (ManagementLoaderScene.Instance) ManagementLoaderScene.Instance.OnFinishOpenAnimation += () => { startGame = true; };
    }
    public async Awaitable LoadMenuScene(TypeScene typeScene)
    {
        {
            try
            {
                switch (typeScene)
                {
                    case TypeScene.HomeScene:

                        AdsManager.Instance.ShowBanner();
                        _ = AudioManager.Instance.FadeOut();
                        await ManagementLoaderScene.Instance.CloseLoader();
                        SceneManager.LoadScene("HomeScene", LoadSceneMode.Single);
                        AudioManager.Instance.bgmSource.clip = AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.BGM, "HomeScene");
                        AudioManager.Instance.bgmSource.Play();
                        _ = AudioManager.Instance.FadeIn();
                        await ManagementLoaderScene.Instance.OpenLoader();
                        break;
                    case TypeScene.LevelSelectorScene:
                    case TypeScene.CreditsScene:
                    case TypeScene.WinScene:
                    case TypeScene.OptionsScene:
                    case TypeScene.GameOverScene:
                    case TypeScene.LanguageSelectorScene:
                        if (!SceneManager.GetSceneByName(typeScene.ToString()).isLoaded) await SceneManager.LoadSceneAsync(typeScene.ToString(), LoadSceneMode.Additive);
                        break;
                    case TypeScene.Exit:
                        Application.Quit();
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
    public async Awaitable LoadGameScene(int id)
    {
        try
        {
            AdsManager.Instance.DestroyBanner();
            Physics2D.gravity = new Vector2(0, -9.81f);
            startGame = false;
            _ = AudioManager.Instance.FadeOut();
            await ManagementLoaderScene.Instance.CloseLoader();
            SceneManager.LoadScene(GameData.Instance.levelsDBSO.data[id].scene, LoadSceneMode.Single);
            currentLevelId = id;
            AudioManager.Instance.bgmSource.clip = AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.BGM, GameData.Instance.levelsDBSO.data[id].scene);
            AudioManager.Instance.bgmSource.Play();
            _ = AudioManager.Instance.FadeIn();
            await ManagementLoaderScene.Instance.OpenLoader();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async Awaitable LoadNextGameScene()
    {
        try
        {
            AdsManager.Instance.DestroyBanner();
            if (GameData.Instance.levelsDBSO.data.ContainsKey(currentLevelId + 1))
            {
                currentLevelId++;
            }
            else
            {
                _ = LoadMenuScene(TypeScene.HomeScene);
                return;
            }
            Physics2D.gravity = new Vector2(0, -9.81f);
            startGame = false;
            _ = AudioManager.Instance.FadeOut();
            await ManagementLoaderScene.Instance.CloseLoader();
            SceneManager.LoadScene(GameData.Instance.levelsDBSO.data[currentLevelId].scene, LoadSceneMode.Single);
            AudioManager.Instance.bgmSource.clip = AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.BGM, GameData.Instance.levelsDBSO.data[currentLevelId].scene);
            AudioManager.Instance.bgmSource.Play();
            _ = AudioManager.Instance.FadeIn();
            await ManagementLoaderScene.Instance.OpenLoader();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async Awaitable ReloadGameScene()
    {
        try
        {
            startGame = false;
            Physics2D.gravity = new Vector2(0, -9.81f);
            _ = AudioManager.Instance.FadeOut();
            await ManagementLoaderScene.Instance.CloseLoader();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
            AudioManager.Instance.bgmSource.Play();
            _ = AudioManager.Instance.FadeIn();
            await ManagementLoaderScene.Instance.OpenLoader();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async Awaitable UnloadMenuScene(TypeScene typeScene, GameManagerHelper.IMenuScene sceneData)
    {
        try
        {
            sceneData.PlayEndAnimation();
            while (!sceneData.AnimationEnded())
            {
                await Awaitable.NextFrameAsync();
            }
            if (typeScene == TypeScene.OptionsScene)
            {
                isPause = false;
            }
            else if (typeScene == TypeScene.DialogScene)
            {
                // WorldManager.Instance.ResumeWorldAfterCloseDialog();
            }
            EventSystem.current.SetSelectedGameObject(null);
            _ = SceneManager.UnloadSceneAsync(typeScene.ToString());
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public void PauseHandle(InputAction.CallbackContext context)
    {
        if (!_excludedScenesForPause.Contains(SceneManager.GetActiveScene().name) && startGame)
        {
            if (!SceneManager.GetSceneByName("OptionsScene").isLoaded)
            {
                _ = LoadMenuScene(TypeScene.OptionsScene);
            }
            else if (OptionsMenu.Instance && OptionsMenu.Instance.isMenuActive)
            {
                _ = UnloadMenuScene(TypeScene.OptionsScene, OptionsMenu.Instance);
            }
        }
    }
    public enum TypeScene
    {
        HomeScene = 0,
        OptionsScene = 1,
        CreditsScene = 2,
        Exit = 3,
        DialogScene = 4,
        Reload = 5,
        GameScene = 6,
        WinScene = 7,
        GameOverScene = 8,
        LevelSelectorScene = 9,
        LanguageSelectorScene = 10,
    }
    public enum TypeLoader
    {
        None = 0,
        WithProgressBar = 1,
        BlackOut = 2
    }
    public enum TypeDevice
    {
        None,
        PC,
        GAMEPAD,
        MOBILE,
    }
}
