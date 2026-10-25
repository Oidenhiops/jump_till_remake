using UnityEngine;

public class OrbChangeGravity : OrbBase
{
    public override void OnPlayerEnter(CharacterPlayer characterPlayer)
    {
        if (!isActive) return;
        Physics2D.gravity = new Vector2(Physics2D.gravity.x, Physics2D.gravity.y * -1);
        characterPlayer.isGravityChange = Physics2D.gravity.y < 0 ? false : true;
        AfterPlayerEnter();
    }
    public override void OnPlayerDie()
    {
        base.OnPlayerDie();
        if (Physics2D.gravity.y > 0) Physics2D.gravity = new Vector2(Physics2D.gravity.x, Physics2D.gravity.y * -1);
    }
}
