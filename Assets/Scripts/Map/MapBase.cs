using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapBase : MonoBehaviour
{
    public virtual void OnHandlePlayerEnter(CharacterBase characterBase, TypeDirection typeDirection) { Debug.Log("OnHandlePlayerEnter not implemented"); }
    public virtual void OnHandlePlayerExit(CharacterBase characterBase, TypeDirection typeDirection) { Debug.Log("OnHandlePlayerExit not implemented"); }
    public enum TypeDirection
    {
        Up,
        Down,
        Left,
        Right
    }
}
