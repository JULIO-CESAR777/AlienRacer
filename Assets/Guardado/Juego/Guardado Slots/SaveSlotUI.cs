using System.Collections;
using TMPro;
using UnityEngine;

public class SaveSlotUI : MonoBehaviour
{
    [Header("Slot")]
    [SerializeField] private int slotIndex;
    [SerializeField] private TextMeshProUGUI label;

    [Header("Textos por idioma - Mismo orden que LanguageManager")]
    [TextArea]
    [SerializeField] private string[] emptySlotTexts;

    [TextArea]
    [SerializeField] private string[] lastRaceTexts;

    [TextArea]
    [SerializeField] private string[] raceTexts;

    private LanguageManager languageManager;
    private Coroutine setupCoroutine;

    private void Awake()
    {
        if (label == null)
            label = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    private void OnEnable()
    {
        setupCoroutine = StartCoroutine(SetupLanguage());
    }

    private void OnDisable()
    {
        if (setupCoroutine != null)
        {
            StopCoroutine(setupCoroutine);
            setupCoroutine = null;
        }

        if (languageManager != null)
            languageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    private IEnumerator SetupLanguage()
    {
        // Espera hasta que LanguageManager exista
        while (LanguageManager.GetInstance() == null)
        {
            yield return null;
        }

        languageManager = LanguageManager.GetInstance();

        // Evita suscribirte doble
        languageManager.OnLanguageChanged -= OnLanguageChanged;
        languageManager.OnLanguageChanged += OnLanguageChanged;

        // Espera un frame extra para que todo termine de inicializar
        yield return null;

        Refresh();

       
    }

    private void OnLanguageChanged(LANGUAGES newLanguage)
    {
        Refresh((byte)newLanguage);
    }

    public void SetSlotIndex(int newSlotIndex)
    {
        slotIndex = newSlotIndex;
        Refresh();
    }

    public void Refresh()
    {
        int languageIndex = 0;

        if (languageManager == null)
            languageManager = LanguageManager.GetInstance();

        if (languageManager != null)
            languageIndex = languageManager.GetCurrentLanguageByte();

        Refresh(languageIndex);
    }

    private void Refresh(int languageIndex)
    {
        if (label == null)
        {
          
            return;
        }

        SlotSaveData data = SlotSaveSystem.LoadSlot(slotIndex);

        int slotNumber = slotIndex + 1;

        if (!data.used)
        {
            label.text = GetText(
                emptySlotTexts,
                languageIndex,
                $"Slot {slotNumber} - Vacío",
                slotNumber
            );

            return;
        }

        if (data.lastCompletedLevel >= 2)
        {
            label.text = GetText(
                lastRaceTexts,
                languageIndex,
                $"Slot {slotNumber} - Última carrera",
                slotNumber
            );

            return;
        }

        label.text = GetText(
            raceTexts,
            languageIndex,
            $"Slot {slotNumber} - Carrera {data.nextLevelToPlay}",
            slotNumber,
            data.nextLevelToPlay
        );
    }

    private string GetText(string[] texts, int languageIndex, string fallback, params object[] values)
    {
        if (texts == null || texts.Length == 0)
            return fallback;

        if (languageIndex < 0 || languageIndex >= texts.Length)
            return fallback;

        string selectedText = texts[languageIndex];

        if (string.IsNullOrWhiteSpace(selectedText))
            return fallback;

        return string.Format(selectedText, values);
    }
}