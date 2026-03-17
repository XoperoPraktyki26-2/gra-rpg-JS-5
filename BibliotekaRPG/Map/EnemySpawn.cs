namespace BibliotekaRPG.map
{
    public class EnemySpawn : ITile
    {
        public ITile.TileType Type => ITile.TileType.EnemySpawn;
        public bool isWalkable => true;
    }
}
