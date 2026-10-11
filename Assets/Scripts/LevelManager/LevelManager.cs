using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    public float timer;
    PlayerInputs playerInputs;
    public bool startTimer;
    public bool finishGame;
    public TMP_Text timerText;
    public GameManagerHelper gameManagerHelper;
    public Transform portalPosition;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            playerInputs = new PlayerInputs();
            playerInputs.Enable();
            playerInputs.Player.ScreenPress.performed += OnHandleTouch;
            GameManager.Instance.OnStartGame += OnHandleEndChangeScene;
        }
    }
    void OnDisable()
    {
        playerInputs.Player.ScreenPress.performed -= OnHandleTouch;
        GameManager.Instance.OnStartGame -= OnHandleEndChangeScene;
        playerInputs.Disable();
    }
    private void OnHandleEndChangeScene(bool value)
    {
        if (value) CharacterPlayer.Instance.InitializeCharacter();
    }

    void OnHandleTouch(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.startGame && !startTimer)
        {
            startTimer = true;
            _ = HandleTimer();
        }
    }
    async Awaitable HandleTimer()
    {
        while (!finishGame)
        {
            timer += Time.deltaTime;
            timerText.text = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(timer / 60f), Mathf.FloorToInt(timer % 60f));
            await Awaitable.NextFrameAsync();
        }
    }
    public void OnHandlePlayerDied()
    {
        CharacterPlayer.Instance.obtainedGear = false;
        Gear.Instance.gearSprite.enabled = true;
        CharacterPlayer.Instance.transform.position = portalPosition.position;
    }
    public void OnHandleLevelFinished()
    {
        finishGame = true;
        if (GameData.Instance.gameDataInfo.levels[GameManager.Instance.currentLevelId].time > timer || GameData.Instance.gameDataInfo.levels[GameManager.Instance.currentLevelId].time == 0f)
        {
            GameData.Instance.gameDataInfo.levels[GameManager.Instance.currentLevelId].time = timer;
            GameData.Instance.SaveGameData();
        }
        if (!GameData.Instance.gameDataInfo.levels.ContainsKey(GameManager.Instance.currentLevelId + 1))
        {
            GameData.Instance.gameDataInfo.levels.Add(GameManager.Instance.currentLevelId + 1, GameData.Instance.levelsDBSO.CloneLevelData(GameManager.Instance.currentLevelId + 1));
            GameData.Instance.gameDataInfo.levels[GameManager.Instance.currentLevelId + 1].isUnlock = true;
            GameData.Instance.SaveGameData();
        }
        CharacterPlayer.Instance.rb.linearVelocity = Vector2.zero;
        CharacterPlayer.Instance.rb.constraints = RigidbodyConstraints2D.FreezeAll;
        _ = GameManager.Instance.LoadMenuScene(GameManager.TypeScene.WinScene);
        StartCoroutine(EnterPortal());
    }
    IEnumerator EnterPortal()
    {
        AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, "EnterPortal"));
        float elapsedTime = 0f;
        float duration = 1f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            CharacterPlayer.Instance.sprite.color = Color.Lerp(Color.white, Color.clear, t);
            CharacterPlayer.Instance.transform.position = Vector3.Lerp(CharacterPlayer.Instance.transform.position, portalPosition.position, t);
            CharacterPlayer.Instance.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            yield return null;
        }
        CharacterPlayer.Instance.transform.localScale = Vector3.zero;
    }
}