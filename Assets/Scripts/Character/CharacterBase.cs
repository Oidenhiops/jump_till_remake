using System;
using UnityEngine;

public class CharacterBase : MonoBehaviour
{
    public bool isSctive;
    public Action OnCharacterDie;
    public SpriteRenderer sprite;
    public Vector2 movementDirection;
    public virtual void InitializeCharacter()
    {
        OnCharacterDie += HandleDie;
    }
    public virtual void HandleStart()
    {
        isSctive = true;
    }
    public virtual void HandleDie()
    {
        isSctive = false;
    }
}
