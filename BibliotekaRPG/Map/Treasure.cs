using System;
using System.Collections.Generic;
using System.Text;
using BibliotekaRPG.Rewards;

namespace BibliotekaRPG.map
{
    public class Treasure : ITile
    {
        public ITile.TileType Type => ITile.TileType.Treasure;

        public bool isWalkable => true;
        public IReward reward;
        public Treasure(IReward reward)
        {
            this.reward = reward;
        }
        
        public void Entered(Player player)
        {
            reward.Apply(player);
        }


    }
}
