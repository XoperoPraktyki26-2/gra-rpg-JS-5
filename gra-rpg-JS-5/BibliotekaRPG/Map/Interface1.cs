using System;
using System.Collections.Generic;
using System.Text;

namespace BibliotekaRPG.map
{
    public interface ITile
    {
        enum TileType
        {
            Grass, Forest, Mountain, EnemySpawn, Treasure, Empty, Merchant, Npc
        }
        TileType Type { get; }
        bool isWalkable { get; }

        
        
    }
}
