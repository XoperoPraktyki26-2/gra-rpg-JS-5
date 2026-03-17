using System;
using System.Collections.Generic;
using System.Text;

namespace BibliotekaRPG.map
{
    public class Mountain : ITile
    {
        public ITile.TileType Type => ITile.TileType.Mountain;

        public bool isWalkable => false;

        
    }
}
