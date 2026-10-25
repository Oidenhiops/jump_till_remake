using UnityEngine;

public class OrbChangeDirection : OrbBase
{
    public override void OnPlayerEnter(CharacterPlayer characterPlayer)
    {
        if (!isActive) return;
        characterPlayer.movementDirection = new Vector2(characterPlayer.movementDirection.x * -1, characterPlayer.movementDirection.y);
        AfterPlayerEnter();
    }
}
