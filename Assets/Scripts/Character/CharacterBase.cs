using UnityEngine;

public class CharacterBase : MonoBehaviour
{
    public bool isSctive;
    public virtual void HandleStart()
    {
        isSctive = true;
    }
    public virtual void HandleDie()
    {
        isSctive = false;
    }
}
