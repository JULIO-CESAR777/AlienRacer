using System.Text;
using UnityEngine;

public static class LevelProgressSave
{
    private const string SAVE_KEY = "LEVEL_PROGRESS";

    public static bool level1Completed;
    public static bool level2Completed;

    public static void Load()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
        {
            level1Completed = false;
            level2Completed = false;
            return;
        }

        string data = PlayerPrefs.GetString(SAVE_KEY);
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
        StringBuilder sb = new StringBuilder();

        sb.Append(level1Completed ? 1 : 0);
        sb.Append("/");
        sb.Append(level2Completed ? 1 : 0);

        PlayerPrefs.SetString(SAVE_KEY, sb.ToString());
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

    public static void ResetProgress()
    {
        level1Completed = false;
        level2Completed = false;
        Save();
    }
}