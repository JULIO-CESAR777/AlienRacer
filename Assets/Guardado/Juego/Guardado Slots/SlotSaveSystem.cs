using UnityEngine;

public static class SlotSaveSystem
{
    private const int TOTAL_SLOTS = 3;
    private const string ACTIVE_SLOT_KEY = "ACTIVE_SLOT";

    private const int MAX_LEVEL = 2;

    private static string GetSlotKey(int slotIndex)
    {
        return "SAVE_SLOT_" + slotIndex;
    }

    public static bool IsValidSlot(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < TOTAL_SLOTS;
    }

    public static void SetActiveSlot(int slotIndex)
    {
        if (!IsValidSlot(slotIndex))
        {
            Debug.LogError("Slot inválido: " + slotIndex);
            return;
        }

        PlayerPrefs.SetInt(ACTIVE_SLOT_KEY, slotIndex);
        PlayerPrefs.Save();
    }

    public static int GetActiveSlot()
    {
        return PlayerPrefs.GetInt(ACTIVE_SLOT_KEY, -1);
    }

    public static SlotSaveData LoadSlot(int slotIndex)
    {
        SlotSaveData data = new SlotSaveData();

        if (!IsValidSlot(slotIndex))
        {
            Debug.LogError("Slot inválido: " + slotIndex);
            return data;
        }

        string raw = PlayerPrefs.GetString(GetSlotKey(slotIndex), "");
        data.LoadFromString(raw);
        return data;
    }

    public static void SaveSlot(int slotIndex, SlotSaveData data)
    {
        if (!IsValidSlot(slotIndex))
        {
            Debug.LogError("Slot inválido: " + slotIndex);
            return;
        }

        PlayerPrefs.SetString(GetSlotKey(slotIndex), data.ToSaveString());
        PlayerPrefs.Save();
    }

    public static void CreateNewSlot(int slotIndex)
    {
        if (!IsValidSlot(slotIndex)) return;

        SlotSaveData data = new SlotSaveData();
        data.used = true;
        data.lastCompletedLevel = 0;
        data.nextLevelToPlay = 1;

        SaveSlot(slotIndex, data);
    }

    public static void DeleteSlot(int slotIndex)
    {
        if (!IsValidSlot(slotIndex)) return;

        PlayerPrefs.DeleteKey(GetSlotKey(slotIndex));
        PlayerPrefs.Save();
    }

    public static bool SlotHasData(int slotIndex)
    {
        SlotSaveData data = LoadSlot(slotIndex);
        return data.used;
    }

    public static int GetLevelToPlay(int slotIndex)
    {
        SlotSaveData data = LoadSlot(slotIndex);

        if (!data.used)
            return 1;

        return data.nextLevelToPlay;
    }

    public static void CompleteLevelInActiveSlot(int completedLevel)
    {
        int activeSlot = GetActiveSlot();

        if (!IsValidSlot(activeSlot))
        {
            Debug.LogWarning("No hay slot activo.");
            return;
        }

        SlotSaveData data = LoadSlot(activeSlot);

        if (!data.used)
            data.used = true;

        if (completedLevel > data.lastCompletedLevel)
            data.lastCompletedLevel = completedLevel;

        int nextLevel = completedLevel + 1;

        if (nextLevel > MAX_LEVEL)
            nextLevel = MAX_LEVEL;

        if (data.nextLevelToPlay <= completedLevel)
            data.nextLevelToPlay = nextLevel;

        SaveSlot(activeSlot, data);

        Debug.Log($"Slot {activeSlot} guardado. Último completado: {data.lastCompletedLevel}, siguiente: {data.nextLevelToPlay}");
    }

    public static void ResetAllSlots()
    {
        for (int i = 0; i < TOTAL_SLOTS; i++)
        {
            DeleteSlot(i);
        }

        PlayerPrefs.DeleteKey(ACTIVE_SLOT_KEY);
        PlayerPrefs.Save();
    }
}