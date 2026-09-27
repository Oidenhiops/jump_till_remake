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
    public bool isPause;
    public bool isWebGlBuild;
    public bool lockDevice = false;
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
    public int currentLevel;
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
    }
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        Physics2D.simulationMode = SimulationMode2D.Script;
        Physics2D.autoSyncTransforms = false;
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
                    case TypeScene.OptionsScene:
                        if (!SceneManager.GetSceneByName("OptionsScene").isLoaded) await SceneManager.LoadSceneAsync("OptionsScene", LoadSceneMode.Additive);
                        break;
                    case TypeScene.CreditsScene:
                        if (!SceneManager.GetSceneByName("CreditsScene").isLoaded) await SceneManager.LoadSceneAsync("CreditsScene", LoadSceneMode.Additive);
                        break;
                    case TypeScene.GameOverScene:
                        if (!SceneManager.GetSceneByName("GameOverScene").isLoaded) await SceneManager.LoadSceneAsync("GameOverScene", LoadSceneMode.Additive);
                        break;
                    case TypeScene.DialogScene:
                        if (!SceneManager.GetSceneByName("DialogScene").isLoaded) await SceneManager.LoadSceneAsync("DialogScene", LoadSceneMode.Additive);
                        break;
                    case TypeScene.LevelSelectorScene:
                        if (!SceneManager.GetSceneByName("LevelSelectorScene").isLoaded) await SceneManager.LoadSceneAsync("LevelSelectorScene", LoadSceneMode.Additive);
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
                Time.timeScale = 1;
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
