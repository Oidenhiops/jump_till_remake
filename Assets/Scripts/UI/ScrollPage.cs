using System;
using UnityEngine;

public class ScrollPage : MonoBehaviour
{
    public ButtonSelector leftButton;
    public ButtonSelector rigthButton;
    public ScrollPageData scrollPageData;
    public void InitScrollPageData(ScrollPageData data)
    {
        scrollPageData = data;
    }
    public void OnHandleChangeValue(int value)
    {
        scrollPageData.pageNum += value;
        ValidateEnaledButtonsByIndex();
    }
    void ValidateEnaledButtonsByIndex()
    {
        if (scrollPageData.pageNum <= 0)
        {
            scrollPageData.pageNum = 0;
            leftButton.button.interactable = false;
            leftButton.DisableButton();
            rigthButton.button.interactable = true;
        }
        else if (scrollPageData.pageNum >= scrollPageData.pageSize)
        {
            scrollPageData.pageNum = scrollPageData.pageSize;
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
    [Serializable]
    public class ScrollPageData
    {
        public int pageNum;
        public int pageSize;
        public float pageHorizontalSize;
    }
}
