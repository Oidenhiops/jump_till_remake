using UnityEngine;

public class DeathZoneMap : MapBase
{
    public float cd;
    public override void OnHandlePlayerEnter(CharacterBase characterBase, TypeDirection typeDirection)
    {
        if (cd > 0) return;
        _ = MakeCd();
        AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, "DeathSound"));
        Destroy(Instantiate(GameData.Instance.utils.prefabs["DieEffect"], characterBase.transform.position, Quaternion.identity), 1f);
        characterBase.HandleDie();
    }
    public override void OnHandlePlayerExit(CharacterBase characterBase, TypeDirection typeDirection) { }
    public async Awaitable MakeCd()
    {
        cd = 1;
        while (cd > 0)
        {
            await Awaitable.NextFrameAsync();
            cd -= Time.deltaTime;
        }
    }
}
