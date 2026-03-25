public class GameState
{
    public PlayerData Player { get; set; }

    public int PlayerRow { get; set; }
    public int PlayerCol { get; set; }

    public TileData[] Map { get; set; }
    public int MapSize { get; set; }

    public int RewindTokens { get; set; }
    public int TurnCount { get; set; }
}
