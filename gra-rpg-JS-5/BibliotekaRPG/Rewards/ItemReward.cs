using System;
using System.Collections.Generic;
using System.Text;

namespace BibliotekaRPG.Rewards
{
    public class ItemReward : IReward
    {
        IItem item;
        public ItemReward(IItem item)
        {
            this.item = item;
        }

        public void Apply(Player player)
        {
            player.AddItem(item);
        }
    }
}
