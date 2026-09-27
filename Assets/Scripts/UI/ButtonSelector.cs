using System;
using System.Collections;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public bool isPermanent;
    public Button button;
    public Transform popTarget;
    public TMP_Text textField;
    public Image buttonImage;
    public GameObject selector;
    public UnityEvent unityEvent;
    public bool makeSound;
    public string soundId = "PressButton";
    public string buttonStyleId = "GeneralButton";
    public float popDuration = 0.1f;
    public float popUpScale = 1.25f;
    public float popDownScale = 1f;
    Coroutine popCoroutine;
    void Awake()
    {
        button.colors = GameData.Instance.utils.buttonsStyle[buttonStyleId];
    }
    public void InitializeDefaultButton()
    {
        ButtonsManager.Instance.lastButton = this;
        ButtonsManager.Instance.lastButtonPermanent = this;
        ChangeButtonColor(true);
        if (selector) selector.SetActive(true);
        button.transform.localScale = Vector3.one * 1.25f;
    }
    public void EnabledButton()
    {
        if (isPermanent)
        {
            if (ButtonsManager.Instance.lastButtonPermanent && ButtonsManager.Instance.lastButtonPermanent != this)
            {
                ButtonsManager.Instance.lastButtonPermanent.DisableButton();
            }
            ButtonsManager.Instance.lastButtonPermanent = this;
            ChangeButtonColor(true);
            if (selector) selector.SetActive(true);
        }
        if (ButtonsManager.Instance.lastButton && !ButtonsManager.Instance.lastButton.isPermanent) ButtonsManager.Instance.lastButton.DisableButton();
        ButtonsManager.Instance.lastButton = this;
    }
    public void DisableButton()
    {
        ChangeButtonColor(false);
        if (selector) selector.SetActive(false);
        if (popCoroutine != null) StopCoroutine(popCoroutine);
        popCoroutine = StartCoroutine(PopButtonRoutine(false));
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;
        if (popCoroutine != null) StopCoroutine(popCoroutine);
        popCoroutine = StartCoroutine(PopButtonRoutine(true));
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (ButtonsManager.Instance.lastButtonPermanent == this) return;
        if (popCoroutine != null) StopCoroutine(popCoroutine);
        popCoroutine = StartCoroutine(PopButtonRoutine(false));
    }

    public void ChangeButtonColor(bool isSelect)
    {
        if (textField) textField.color = GameData.Instance.utils.systemColors[isSelect ? "SelectButton" : "UnSelectButton"];
        if (buttonImage) buttonImage.color = GameData.Instance.utils.systemColors[isSelect ? "SelectButton" : "UnSelectButton"];
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (ButtonsManager.Instance.lastButton && ButtonsManager.Instance.lastButton == this) return;
        if (makeSound) AudioManager.Instance.PlayASound(AudioManager.Instance.GetAudioClip(SoundsDBSO.TypeSound.SFX, soundId), true);
        EnabledButton();
        unityEvent?.Invoke();
    }

    IEnumerator PopButtonRoutine(bool isPopUp)
    {
        Vector3 startScale = popTarget.localScale;
        Vector3 endScale = Vector3.one * (isPopUp ? popUpScale : popDownScale);
        float elapsedTime = 0f;

        while (elapsedTime < popDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / popDuration);
            popTarget.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        popTarget.localScale = endScale;
        popCoroutine = null;
    }
}
