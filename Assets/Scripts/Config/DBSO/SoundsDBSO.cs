using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundsDB", menuName = "ScriptableObjects/DB/SoundsDB", order = 1)]
public class SoundsDBSO : ScriptableObject
{
    [SerializeField]
    public Dictionary<TypeSound, Dictionary<string, AudioClip[]>> sounds;
    public enum TypeSound
    {
        None = 0,
        BGM = 1,
        SFX = 2,
    }
}