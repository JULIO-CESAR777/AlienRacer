using System;
using System.Text;

[Serializable]
public class SlotSaveData
{
    public bool used;
    public int lastCompletedLevel;
    public int nextLevelToPlay;

    public SlotSaveData()
    {
        used = false;
        lastCompletedLevel = 0;
        nextLevelToPlay = 1;
    }

    public string ToSaveString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(used ? 1 : 0);
        sb.Append("|");
        sb.Append(lastCompletedLevel);
        sb.Append("|");
        sb.Append(nextLevelToPlay);
        return sb.ToString();
    }

    public void LoadFromString(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            used = false;
            lastCompletedLevel = 0;
            nextLevelToPlay = 1;
            return;
        }

        string[] parts = data.Split('|');

        if (parts.Length < 3)
        {
            used = false;
            lastCompletedLevel = 0;
            nextLevelToPlay = 1;
            return;
        }

        used = parts[0] == "1";
        int.TryParse(parts[1], out lastCompletedLevel);
        int.TryParse(parts[2], out nextLevelToPlay);

        if (nextLevelToPlay <= 0)
            nextLevelToPlay = 1;
    }
}