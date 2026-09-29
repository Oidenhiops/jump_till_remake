using UnityEngine;

public class CharacterPlayer : CharacterBase
{
    public Rigidbody2D rigidbody2D;
    public int direction = 1;
    public override void HandleStart()
    {
        isSctive = true;
        rigidbody2D.constraints = RigidbodyConstraints2D.None;
        rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
    }
    public override void HandleDie()
    {
        base.HandleDie();
        rigidbody2D.constraints = RigidbodyConstraints2D.FreezePositionX;
        rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
    }
    void Update()
    {
        if (isSctive)
        {
            
        }
    }
}
