using System;
using UnityEngine;

public class Gear : MonoBehaviour
{
    public static Gear Instance { get; private set; }
    public GameObject gearSprite;
    public Action OnHandleObtainGear;
    public Action OnHandleLoseGear;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
}