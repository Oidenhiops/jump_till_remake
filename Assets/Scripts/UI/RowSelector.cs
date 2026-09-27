using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class RowSelector : MonoBehaviour
{
    public ButtonSelector leftButton;
    public ButtonSelector rigthButton;
    public TMP_Text textField;
    public int index;
    public RowData[] options;
    public UnityEvent<RowData> unityEvent;
    public void InitializeData(RowData[] data, int key)
    {
        index = key;
        options = data;
        textField.text = data[index].value;
        ValidateEnaledButtonsByIndex();
    }
    public void OnHandleChangeValue(int value)
    {
        index += value;
        ValidateEnaledButtonsByIndex();
        ChangeValueAction(index);
    }
    void ValidateEnaledButtonsByIndex()
    {
        if (index <= 0)
        {
            index = 0;
            leftButton.button.interactable = false;
            leftButton.DisableButton();
            rigthButton.button.interactable = true;
        }
        else if (index >= options.Length - 1)
        {
            index = options.Length - 1;
            leftButton.button.interactable = true;
            rigthButton.button.interactable = false;
            rigthButton.DisableButton();
        }
        else
        {
            leftButton.button.interactable = true;
            rigthButton.button.interactable = true;
        }
    }
    void ChangeValueAction(int index)
    {
        textField.text = options[index].value;
        unityEvent?.Invoke(options[index]);
    }
    [Serializable]
    public class RowData
    {
        public int key;
        public string value;
        public RowData(int key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }
}
