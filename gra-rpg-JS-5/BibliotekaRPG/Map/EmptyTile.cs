namespace BibliotekaRPG.map
{
    public class EmptyTile : ITile
    {
        public ITile.TileType Type => ITile.TileType.Empty;
        public bool isWalkable => true;
    }
}
