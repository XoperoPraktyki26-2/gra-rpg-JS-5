using System;
using System.Collections.Generic;
using System.Text;

namespace BibliotekaRPG.Rewards
{
    public class GoldReward : IReward
    {
        int amm;
        public GoldReward(int amm)
        {
            this.amm = amm;
        }

        public void Apply(Player player)
        {
            player.ReciveGold(amm);
        }
    }
}
