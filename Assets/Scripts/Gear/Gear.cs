using System;
using UnityEngine;
using UnityEngine.UI;

public class Gear : MonoBehaviour
{
    public static Gear Instance { get; private set; }
    public SpriteRenderer gearSprite;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    public void OnPlayerEnter()
    {
        CharacterPlayer.Instance.obtainedGear = true;
        gearSprite.enabled = false;
        AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, "Gear"));
    }
}