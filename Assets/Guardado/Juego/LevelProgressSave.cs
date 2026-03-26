using System.Text;
using UnityEngine;

public static class LevelProgressSave
{
    private const string ACTIVE_SLOT_KEY = "ACTIVE_SLOT";

    public static bool level1Completed;
    public static bool level2Completed;

    private static string GetSaveKey()
    {
        int activeSlot = PlayerPrefs.GetInt(ACTIVE_SLOT_KEY, -1);

        if (activeSlot < 0)
            activeSlot = 0;

        return "LEVEL_PROGRESS_SLOT_" + activeSlot;
    }

    public static void SetActiveSlot(int slotIndex)
    {
        PlayerPrefs.SetInt(ACTIVE_SLOT_KEY, slotIndex);
        PlayerPrefs.Save();
    }

    public static int GetActiveSlot()
    {
        return PlayerPrefs.GetInt(ACTIVE_SLOT_KEY, -1);
    }

    public static void Load()
    {
        string saveKey = GetSaveKey();

        if (!PlayerPrefs.HasKey(saveKey))
        {
            level1Completed = false;
            level2Completed = false;
            return;
        }

        string data = PlayerPrefs.GetString(saveKey);
        string[] splitData = data.Split('/');

        if (splitData.Length < 2)
        {
            level1Completed = false;
            level2Completed = false;
            return;
        }

        level1Completed = int.Parse(splitData[0]) == 1;
        level2Completed = int.Parse(splitData[1]) == 1;
    }

    public static void Save()
    {
        string saveKey = GetSaveKey();

        StringBuilder sb = new StringBuilder();
        sb.Append(level1Completed ? 1 : 0);
        sb.Append("/");
        sb.Append(level2Completed ? 1 : 0);

        PlayerPrefs.SetString(saveKey, sb.ToString());
        PlayerPrefs.Save();
    }

    public static void CompleteLevel(int level)
    {
        Load();

        switch (level)
        {
            case 1:
                level1Completed = true;
                break;
            case 2:
                level2Completed = true;
                break;
        }

        Save();
    }

    public static bool IsLevelUnlocked(int level)
    {
        Load();

        switch (level)
        {
            case 1:
                return true;
            case 2:
                return level1Completed;
        }

        return false;
    }

    public static int GetNextLevelToPlay()
    {
        Load();

        if (!level1Completed)
            return 1;

        if (!level2Completed)
            return 2;

        return 2;
    }

    public static void ResetProgress()
    {
        level1Completed = false;
        level2Completed = false;
        Save();
    }

    public static void ResetSlot(int slotIndex)
    {
        string key = "LEVEL_PROGRESS_SLOT_" + slotIndex;
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
    }
}