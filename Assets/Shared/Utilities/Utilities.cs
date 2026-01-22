using Godot;
using System;
using System.Text.Json;

public partial class Utilities : GameData
{
    public static T Load<T>(string fileName)
    {
        string json_string;
        File file = new File();
        file.Open($"{LEVEL_DATA_PATH}{fileName}.json", File.ModeFlags.Read);

        if (file == null)
        {
            GD.Print("File is not exist!");
            return default(T);
        }
        json_string = file.GetAsText();
        file.Close();
        return JsonSerializer.Deserialize<T>(json_string);
    }

    public static void Save<T>(string fileName, T saveObject)
    {
        string jsonString = JsonSerializer.Serialize(saveObject);
        GD.Print(jsonString);
        File file = new File();
        file.Open($"{LEVEL_DATA_PATH}{fileName}.json", File.ModeFlags.Write);

        if (file == null)
        {
            GD.Print("File is not exist!");
            return;
        }
        file.StoreString(jsonString);
        file.Close();

    }
}
