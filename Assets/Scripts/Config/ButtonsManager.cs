using UnityEngine;

public class ButtonsManager : MonoBehaviour
{
    public static ButtonsManager Instance { get; private set; }
    public ButtonSelector lastButton;
    public ButtonSelector lastButtonPermanent;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
