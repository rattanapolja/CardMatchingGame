using UnityEngine;
using System.IO;

public static class SaveSystem
{
    private static string m_SavePath = Application.persistentDataPath + "/savefile.json";

    public static void SaveGame(GameData data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(m_SavePath, json);
    }

    public static GameData LoadGame()
    {
        if (File.Exists(m_SavePath))
        {
            string json = File.ReadAllText(m_SavePath);
            return JsonUtility.FromJson<GameData>(json);
        }

        return null;
    }
}

[System.Serializable]
public class GameData
{
    public int score;
    public int rows;
    public int columns;
}