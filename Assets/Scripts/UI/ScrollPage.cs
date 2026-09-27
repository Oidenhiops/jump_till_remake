using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScrollPage : MonoBehaviour
{
    public ButtonSelector leftButton;
    public ButtonSelector rigthButton;
    public ScrollRect scrollRect;
    public float scrollDuration = 0.35f;
    public ScrollPageData scrollPageData;

    Coroutine scrollRoutine;

    public void InitScrollPageData(ScrollPageData data)
    {
        scrollPageData = data;
        ValidateEnaledButtonsByIndex();
    }

    public void OnHandleChangeValue(int value)
    {
        scrollPageData.pageNum += value;
        ValidateEnaledButtonsByIndex();
        ScrollToCurrentPage();
    }

    void ScrollToCurrentPage()
    {
        if (scrollRoutine != null) StopCoroutine(scrollRoutine);

        RectTransform content = scrollRect.content;
        Vector2 target = new Vector2(-scrollPageData.pageNum * scrollPageData.pageHorizontalSize, content.anchoredPosition.y);
        scrollRoutine = StartCoroutine(SmoothScroll(content, target));
    }

    IEnumerator SmoothScroll(RectTransform content, Vector2 target)
    {
        scrollRect.velocity = Vector2.zero; // corta la inercia del ScrollRect
        Vector2 start = content.anchoredPosition;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / scrollDuration;
            float eased = Mathf.SmoothStep(0f, 1f, t);
            content.anchoredPosition = Vector2.Lerp(start, target, eased);
            yield return null;
        }

        content.anchoredPosition = target;
        scrollRoutine = null;
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
        else if (scrollPageData.pageNum >= scrollPageData.pageSize - 1)
        {
            scrollPageData.pageNum = scrollPageData.pageSize - 1;
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