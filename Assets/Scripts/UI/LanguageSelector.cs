using System;
using System.Collections.Generic;
using UnityEngine;

public class LanguageSelector : MonoBehaviour, GameManagerHelper.IMenuScene
{
    public Animator animator;
    public GameManagerHelper gameManagerHelper;
    void Awake()
    {
        gameManagerHelper.sceneData = this;
        languageRowSelector.InitializeData(TransformLanguageRowSelectorData(out int currentKey), currentKey);
    }
    RowSelector.RowData[] TransformLanguageRowSelectorData(out int currentKey)
    {
        List<RowSelector.RowData> rowData = new List<RowSelector.RowData>();
        foreach (GameData.TypeLanguage row in Enum.GetValues(typeof(GameData.TypeLanguage)))
        {
            rowData.Add(new RowSelector.RowData((int)row, row.ToString()));
        }
        currentKey = (int)GameData.Instance.systemDataInfo.configurationsInfo.currentLanguage;
        return rowData.ToArray();
    }
    public void ChangeLanguage(RowSelector.RowData rowData)
    {
        GameData.Instance.ChangeLanguage((GameData.TypeLanguage)rowData.key);
    }
    public void HandleUnloadLanguageSelectorMenu() { gameManagerHelper.UnloadMenuScene(10); }
    public void PlayEndAnimation()
    {
        animator.SetTrigger("exit");
    }

    public bool AnimationEnded()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("MenuExit") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1;
    }

    public RowSelector languageRowSelector;
}
