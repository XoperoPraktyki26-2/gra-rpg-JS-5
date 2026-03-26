using BibliotekaRPG.map;
using System;

namespace BibliotekaRPG.Npcs
{
    public class NpcTile : ITile
    {
        public ITile.TileType Type => ITile.TileType.Npc;
        public bool isWalkable => true;
        public NpcData Npc { get; }

        public NpcTile(NpcData npc)
        {
            Npc = npc ?? throw new ArgumentNullException(nameof(npc));
        }
    }
}
