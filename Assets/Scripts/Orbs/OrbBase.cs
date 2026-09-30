using UnityEngine;

public class OrbBase : MonoBehaviour
{
    public bool isActive;
    public GameObject orbVisual;
    public void Start()
    {
        isActive = true;
        CharacterPlayer.Instance.OnPlayerDie += OnHandlePlayerDie;
    }
    void OnDisable()
    {
        CharacterPlayer.Instance.OnPlayerDie -= OnHandlePlayerDie;
    }
    public virtual void OnPlayerEnter(CharacterPlayer characterPlayer) { Debug.LogError("On Player Enter Not Implementer"); }

    void OnHandlePlayerDie()
    {
        OnPlayerDie();
    }
    public virtual void AfterPlayerEnter()
    {
        orbVisual.gameObject.SetActive(false);
        isActive = false;
    }
    public void OnPlayerDie()
    {
        orbVisual.gameObject.SetActive(true);
        isActive = true;
    }
}
