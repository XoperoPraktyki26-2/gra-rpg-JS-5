namespace BibliotekaRPG.map
{
    public class Forest : ITile
    {
        public ITile.TileType Type => ITile.TileType.Forest;
        public bool isWalkable => true;
    }
}
