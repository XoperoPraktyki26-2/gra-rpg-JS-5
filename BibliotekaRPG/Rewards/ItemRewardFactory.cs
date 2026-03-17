using System;
using System.Collections.Generic;
using System.Text;
using BibliotekaRPG.Inventory.Items;

namespace BibliotekaRPG.Rewards
{
    public class ItemRewardFactory : IRewardFactroy
    {
        Random rng = new Random();
        ItemFactory itemFactory = new ItemFactory();
        public IReward get()
        {
            return new ItemReward(itemFactory.CreateItem());
        }
    }
}
