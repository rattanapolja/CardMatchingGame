using UnityEngine;
using System.IO;
using System.Collections.Generic;

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
    public int Score;
    public int Rows;
    public int Columns;
    public int ComboCount;
    public int MaxCombo;
    public int MisMatchStack;
    public List<CardData> cards;
    public List<int> QueueList;
}

[System.Serializable]
public class CardData
{
    public int CardIndex;
    public int SpriteIndex;
    public bool IsFaceUp;
}