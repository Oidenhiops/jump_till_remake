using System.Collections.Generic;
using UnityEngine;

public class GroundMap : MapBase
{
    public override void OnHandlePlayerEnter(CharacterBase characterBase, TypeDirection typeDirection)
    {
        switch (typeDirection)
        {
            case TypeDirection.Up:
            case TypeDirection.Down:
                break;
            case TypeDirection.Left:
                characterBase.SetMovement(1);
                break;
            case TypeDirection.Right:
                characterBase.SetMovement(-1);
                break;
        }
        AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, "GroundSound"));
    }
    public override void OnHandlePlayerExit(CharacterBase characterBase, TypeDirection typeDirection) { }
}
