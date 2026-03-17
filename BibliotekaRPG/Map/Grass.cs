using System;
using System.Collections.Generic;
using System.Text;

namespace BibliotekaRPG.map
{
    public class Grass : ITile
    {
        public ITile.TileType Type => ITile.TileType.Grass;

        public bool isWalkable => true;

        
    }
}
