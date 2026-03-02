using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemRouletteUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private Image iconPrefab;

    [Header("Spin")]
    [SerializeField] private float spinSpeed = 900f;
    [SerializeField] private float spinSeconds = 1.2f;
    [SerializeField] private float slowSeconds = 0.6f;
    [SerializeField] private int ribbonCount = 30;

    
 
    
    
    private List<ItemBase> ribbon = new();
    private bool spinning;

    public bool IsSpinning => spinning;

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);
     
    }

    public void Spin(ItemBase finalResult, List<ItemBase> visualPool, System.Action<ItemBase> onDone)
    {
        if (spinning) return;
        if (finalResult == null) return;

        StartCoroutine(SpinCR(finalResult, visualPool, onDone));
    }

    private IEnumerator SpinCR(ItemBase finalResult, List<ItemBase> visualPool, System.Action<ItemBase> onDone)
    {
        spinning = true;
        if (panel != null) panel.SetActive(true);

        BuildRibbon(finalResult, visualPool);
        content.anchoredPosition = Vector2.zero;

        float t = 0f;
        while (t < spinSeconds)
        {
            MoveRibbon(spinSpeed);
            t += Time.deltaTime;
            yield return null;
        }

        // Snap al finalResult (última aparición para que se sienta natural)
        int targetIndex = FindLastIndexOf(finalResult);
        float targetX = GetContentXToCenterIndex(targetIndex);

        float startX = content.anchoredPosition.x;
        t = 0f;
        while (t < slowSeconds)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / slowSeconds);
            float eased = 1f - Mathf.Pow(1f - a, 3f);
            float x = Mathf.Lerp(startX, targetX, eased);
            content.anchoredPosition = new Vector2(x, content.anchoredPosition.y);
            yield return null;
        }

        content.anchoredPosition = new Vector2(targetX, content.anchoredPosition.y);

        onDone?.Invoke(finalResult);

        yield return new WaitForSeconds(0.2f);
        if (panel != null) panel.SetActive(false);

        spinning = false;
    }

    private void BuildRibbon(ItemBase finalResult, List<ItemBase> visualPool)
    {
        // limpia
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);

        ribbon.Clear();

        // llena con pool visual aleatorio
        if (visualPool == null || visualPool.Count == 0)
        {
            visualPool = new List<ItemBase>() { finalResult };
        }

        for (int i = 0; i < ribbonCount; i++)
        {
            ItemBase it = visualPool[Random.Range(0, visualPool.Count)];
            ribbon.Add(it);

            var img = Instantiate(iconPrefab, content);
            img.sprite = it.icon;
            img.enabled = it.icon != null;
        }

        // fuerza que el finalResult aparezca al final para poder “snappear”
        int insertIndex = ribbonCount - 3;
        insertIndex = Mathf.Clamp(insertIndex, 0, ribbon.Count - 1);
        ribbon[insertIndex] = finalResult;
        (content.GetChild(insertIndex) as RectTransform).GetComponent<Image>().sprite = finalResult.icon;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    private void MoveRibbon(float pxPerSec)
    {
        Vector2 p = content.anchoredPosition;
        p.x -= pxPerSec * Time.deltaTime;
        content.anchoredPosition = p;
    }

    private int FindLastIndexOf(ItemBase item)
    {
        for (int i = ribbon.Count - 1; i >= 0; i--)
            if (ribbon[i] == item) return i;
        return ribbon.Count / 2;
    }

    private float GetContentXToCenterIndex(int index)
    {
        RectTransform itemRect = content.GetChild(index) as RectTransform;
        float viewportCenterX = viewport.rect.width * 0.5f;
        float itemCenterInContent = itemRect.anchoredPosition.x + itemRect.rect.width * 0.5f;
        return viewportCenterX - itemCenterInContent;
    }
}