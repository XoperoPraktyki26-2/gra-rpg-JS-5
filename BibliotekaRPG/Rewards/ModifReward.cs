using System;
using System.Collections.Generic;
using System.Text;

namespace BibliotekaRPG.Rewards
{
    public class ModifReward : IReward
    {
        IStatModifier md;
        public ModifReward(IStatModifier md)
        {
            this.md = md;
        }

        public void Apply(Player player)
        {
            player.Equipment.PutOn(md);
        }
    }
}
