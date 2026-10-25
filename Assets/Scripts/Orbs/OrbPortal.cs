using System.Collections;
using UnityEngine;

public class OrbPortal : OrbBase
{
    public OrbPortal otherPortal;
    public int exitDirection;
    public float cd;
    public override void OnPlayerEnter(CharacterPlayer characterPlayer)
    {
        if (!isActive || cd > 0) return;
        if (otherPortal)
        {
            otherPortal.OnPlayerTp(characterPlayer);
        }
    }
    public void OnPlayerTp(CharacterPlayer characterPlayer)
    {
        _ = MakeCd(this);
        _ = MakeCd(otherPortal);
        characterPlayer.transform.localPosition = transform.position;
        characterPlayer.movementDirection = new Vector2(exitDirection, characterPlayer.movementDirection.y);
    }
    public async Awaitable MakeCd(OrbPortal orbPortal)
    {
        orbPortal.cd = 1;
        while (orbPortal.cd > 0)
        {
            await Awaitable.NextFrameAsync();
            orbPortal.cd -= Time.deltaTime;
        }
    }
}
