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
    [SerializeField] private float slowSeconds = 0.8f;
    [SerializeField] private int ribbonCount = 15;

    [Header("Result")]
    [Tooltip("2 = penúltimo, 3 = antepenúltimo")]
    [SerializeField] private int resultFromEnd = 2;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip tickSound;
    [SerializeField] private AudioClip winSound;
    [Tooltip("Distancia en píxeles que debe moverse para sonar un tick (Ancho del item + espaciado)")]
    [SerializeField] private float distanceBetweenTicks = 100f;

    private readonly List<ItemBase> ribbon = new();
    private bool spinning;
    private int forcedResultIndex;

    private MainManager gm;
    private bool isPaused = false;

    public bool IsSpinning => spinning;

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void Start()
    {
        gm = MainManager.GetInstance();
        if (gm != null)
        {
            gm.onChangeGameState += OnChangeGameStateCallback;
            isPaused = gm.gameState != GameState.Play;
        }
        else
        {
            Debug.LogError("MainManager is NULL in ItemRouletteUI");
        }
    }

    private void OnDestroy()
    {
        if (gm != null)
            gm.onChangeGameState -= OnChangeGameStateCallback;
    }

    public void OnChangeGameStateCallback(GameState newState)
    {
        isPaused = newState != GameState.Play;
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

        if (panel != null)
            panel.SetActive(true);

        yield return StartCoroutine(ClearContentCR());

        BuildRibbon(finalResult, visualPool);

        yield return null;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        yield return null;

        content.anchoredPosition = Vector2.zero;

        float lastTickX = content.anchoredPosition.x;

        float t = 0f;

        while (t < spinSeconds)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            MoveRibbon(spinSpeed);

            float currentX = content.anchoredPosition.x;
            if (Mathf.Abs(currentX - lastTickX) >= distanceBetweenTicks)
            {
                PlayTickSound();
                lastTickX = currentX;
            }

            t += Time.deltaTime;
            yield return null;
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        yield return null;

        float startX = content.anchoredPosition.x;
        float targetX = GetContentXToCenterIndex(forcedResultIndex);

        t = 0f;

        while (t < slowSeconds)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / slowSeconds);
            float eased = 1f - Mathf.Pow(1f - a, 3f);

            float x = Mathf.Lerp(startX, targetX, eased);
            content.anchoredPosition = new Vector2(x, content.anchoredPosition.y);

            float currentX = content.anchoredPosition.x;
            if (Mathf.Abs(currentX - lastTickX) >= distanceBetweenTicks)
            {
                PlayTickSound();
                lastTickX = currentX;
            }

            yield return null;
        }

        content.anchoredPosition = new Vector2(targetX, content.anchoredPosition.y);

        if (audioSource != null && winSound != null)
        {
            audioSource.PlayOneShot(winSound);
        }

        onDone?.Invoke(finalResult);

        float wait = 0f;
        while (wait < 0.2f)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            wait += Time.deltaTime;
            yield return null;
        }

        if (panel != null)
            panel.SetActive(false);

        spinning = false;
    }

    private void PlayTickSound()
    {
        if (audioSource != null && tickSound != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(tickSound);
        }
    }

    private IEnumerator ClearContentCR()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        yield return null;

        ribbon.Clear();
        content.anchoredPosition = Vector2.zero;
    }

    private void BuildRibbon(ItemBase finalResult, List<ItemBase> visualPool)
    {
        ribbon.Clear();

        if (visualPool == null || visualPool.Count == 0)
            visualPool = new List<ItemBase> { finalResult };

        for (int i = 0; i < ribbonCount; i++)
        {
            ItemBase it = GetRandomItemExcluding(finalResult, visualPool);
            ribbon.Add(it);

            Image img = Instantiate(iconPrefab, content);
            img.sprite = it != null ? it.icon : null;
            img.enabled = it != null && it.icon != null;
        }

        forcedResultIndex = Mathf.Clamp(ribbonCount - resultFromEnd, 0, ribbon.Count - 1);

        ribbon[forcedResultIndex] = finalResult;

        Image forcedImg = content.GetChild(forcedResultIndex).GetComponent<Image>();
        forcedImg.sprite = finalResult.icon;
        forcedImg.enabled = finalResult.icon != null;
    }

    private ItemBase GetRandomItemExcluding(ItemBase excluded, List<ItemBase> pool)
    {
        if (pool == null || pool.Count == 0)
            return excluded;

        if (pool.Count == 1)
            return pool[0];

        ItemBase selected = null;
        int safety = 0;

        do
        {
            selected = pool[Random.Range(0, pool.Count)];
            safety++;
        }
        while (selected == excluded && safety < 50);

        return selected;
    }

    private void MoveRibbon(float pxPerSec)
    {
        Vector2 p = content.anchoredPosition;
        p.x -= pxPerSec * Time.deltaTime;
        content.anchoredPosition = p;
    }

    private float GetContentXToCenterIndex(int index)
    {
        if (index < 0 || index >= content.childCount)
            return content.anchoredPosition.x;

        RectTransform itemRect = content.GetChild(index) as RectTransform;

        Vector3 itemWorldCenter = itemRect.TransformPoint(itemRect.rect.center);
        Vector3 viewportWorldCenter = viewport.TransformPoint(viewport.rect.center);

        float deltaX = viewportWorldCenter.x - itemWorldCenter.x;

        return content.anchoredPosition.x + deltaX;
    }
}