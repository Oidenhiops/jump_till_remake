using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SliderManager : MonoBehaviour
{
    public Slider slider;
    public UnityEvent<float> unityEvent;
    public bool makeSound;
    public bool awaitSecondsToMakeSound;
    public float awaitTime = 0.125f;
    public bool isAwaitingTime;
    public bool isInitialice;
    public string soundId = "ChangeSliderValue";
    public void OnSliderValueChange()
    {
        if (!isInitialice) return;
        if (makeSound)
        {
            if (awaitSecondsToMakeSound && !isAwaitingTime)
            {
                _ = AwaitTime();
                AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, soundId), true);
            }
            else if (!awaitSecondsToMakeSound)
            {
                AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, soundId), true);
            }
        }
        unityEvent?.Invoke(slider.value);
    }
    public async Awaitable AwaitTime()
    {
        isAwaitingTime = true;
        await Awaitable.WaitForSecondsAsync(awaitTime);
        isAwaitingTime = false;
    }
}
