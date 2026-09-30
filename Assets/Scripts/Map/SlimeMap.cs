using UnityEngine;

public class SlimeMap : MapBase
{
    public override void OnHandlePlayerEnter(CharacterBase characterBase, TypeDirection typeDirection)
    {
        CharacterPlayer.Instance.rb.linearVelocity = new Vector2(0, CharacterPlayer.Instance.rb.linearVelocity.y);
    }
}
