using System;
using UnityEngine;

public class CharacterBase : MonoBehaviour
{
    public bool isActive;
    public Transform spawnPos;
    public Action OnCharacterDie;
    public SpriteRenderer sprite;
    public Vector2 _movementDirection;
    public Vector2 movementDirection
    {
        get => _movementDirection;
        set
        {
            if (value.x != _movementDirection.x)
            {
                sprite.flipX = value.x > 0 ? false : true;
            }
            _movementDirection = value;
        }
    }
    public bool _isGravityChange;
    public bool isGravityChange
    {
        get => _isGravityChange;
        set
        {
            sprite.flipY = value;
            _isGravityChange = value;
        }
    }
    public void SetMovement(float? x = null, float? y = null)
    {
        movementDirection = new Vector2(x ?? _movementDirection.x, y ?? _movementDirection.y);
    }
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
        transform.position = spawnPos.position;
    }
}
