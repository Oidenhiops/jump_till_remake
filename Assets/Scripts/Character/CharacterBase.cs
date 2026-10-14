using System;
using UnityEngine;

public class CharacterBase : MonoBehaviour
{
    public bool isActive;
    public Action OnCharacterDie;
    public SpriteRenderer sprite;
    public Vector2 movementDirection;
    public virtual void InitializeCharacter()
    {
        OnCharacterDie += HandleDie;
    }
    public virtual void HandleStart()
    {
        isActive = true;
    }
    public virtual void HandleDie()
    {
        isActive = false;
    }
}
