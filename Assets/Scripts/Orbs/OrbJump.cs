using UnityEngine;

public class OrbJump : OrbBase
{
    public int amountJumpsToAppend;
    public override void OnPlayerEnter(CharacterPlayer characterPlayer)
    {
        if (!isActive) return;
        characterPlayer.jumpCount += amountJumpsToAppend;
        AfterPlayerEnter();
    }
}
