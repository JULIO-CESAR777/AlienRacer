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

    private void Awake()
    {
        if (label == null)
            label = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        languageManager = LanguageManager.GetInstance();

        if (languageManager != null)
            languageManager.OnLanguageChanged += OnLanguageChanged;

        Refresh();
    }

    private void OnDisable()
    {
        if (languageManager != null)
            languageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(LANGUAGES newLanguage)
    {
        Refresh();
    }

    public void SetSlotIndex(int newSlotIndex)
    {
        slotIndex = newSlotIndex;
        Refresh();
    }

    public void Refresh()
    {
        print("refrescando");
        if (label == null) return;

        SlotSaveData data = SlotSaveSystem.LoadSlot(slotIndex);

        int slotNumber = slotIndex + 1;

        if (!data.used)
        {
            label.text = GetText(
                emptySlotTexts,
                $"Slot {slotNumber} - Vacío",
                slotNumber
            );

            return;
        }

        if (data.lastCompletedLevel >= 2)
        {
            label.text = GetText(
                lastRaceTexts,
                $"Slot {slotNumber} - Última carrera",
                slotNumber
            );

            return;
        }

        label.text = GetText(
            raceTexts,
            $"Slot {slotNumber} - Carrera {data.nextLevelToPlay}",
            slotNumber,
            data.nextLevelToPlay
        );
    }

    private string GetText(string[] texts, string fallback, params object[] values)
    {
        if (languageManager == null)
            languageManager = LanguageManager.GetInstance();

        int languageIndex = 0;

        if (languageManager != null)
            languageIndex = languageManager.GetCurrentLanguageByte();

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