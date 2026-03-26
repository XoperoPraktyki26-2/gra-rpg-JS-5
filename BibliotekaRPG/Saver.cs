using System.IO;
using System.Text.Json;

namespace BibliotekaRPG
{
    public class Saver
    {
        private const string SavePath = "save.save";

        public void Save(GameState state)
        {
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(SavePath, json);
        }

        public GameState Load()
        {
            if (!File.Exists(SavePath))
                return null;

            var json = File.ReadAllText(SavePath);
            return JsonSerializer.Deserialize<GameState>(json);
        }
    }
}