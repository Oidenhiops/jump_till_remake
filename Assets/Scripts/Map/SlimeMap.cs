using UnityEngine;

public class SlimeMap : MapBase
{
    public override void OnHandlePlayerEnter(CharacterBase characterBase, TypeDirection typeDirection)
    {
        CharacterPlayer.Instance.currentSpeed = 0;
        AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, "SlimeGroundSound"));
    }
    public override void OnHandlePlayerExit(CharacterBase characterBase, TypeDirection typeDirection)
    {
        CharacterPlayer.Instance.currentSpeed = CharacterPlayer.Instance.speed;
    }
}
