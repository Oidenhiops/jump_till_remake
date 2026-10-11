using System.Collections.Generic;
using UnityEngine;

public class GroundMap : MapBase
{
    public override void OnHandlePlayerEnter(CharacterBase characterBase, TypeDirection typeDirection)
    {
        switch (typeDirection)
        {
            case TypeDirection.Down:
                break;
            case TypeDirection.Left:
                characterBase.movementDirection.x = 1;
                characterBase.sprite.flipX = false;
                break;
            case TypeDirection.Right:
                characterBase.movementDirection.x = -1;
                characterBase.sprite.flipX = true;
                break;
        }
        AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, "GroundSound"));
    }
    public override void OnHandlePlayerExit(CharacterBase characterBase, TypeDirection typeDirection) { }
}
