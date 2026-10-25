using UnityEngine;

public class OrbBase : MonoBehaviour
{
    public bool isActive;
    public GameObject orbVisual;
    public void Start()
    {
        isActive = true;
        CharacterPlayer.Instance.OnCharacterDie += OnPlayerDie;
    }
    void OnDisable()
    {
        CharacterPlayer.Instance.OnCharacterDie -= OnPlayerDie;
    }
    public virtual void OnPlayerEnter(CharacterPlayer characterPlayer) { Debug.LogError("On Player Enter Not Implementer"); }
    public virtual void AfterPlayerEnter()
    {
        orbVisual.gameObject.SetActive(false);
        isActive = false;
    }
    public virtual void OnPlayerDie()
    {
        orbVisual.gameObject.SetActive(true);
        isActive = true;
    }
}
