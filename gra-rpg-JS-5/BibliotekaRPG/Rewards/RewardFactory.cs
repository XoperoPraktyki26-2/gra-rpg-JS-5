using System;
using System.Collections.Generic;
using System.Text;

namespace BibliotekaRPG.Rewards
{
    public class RewardFactory
    {


        private  Random rand = new Random();

        private  IRewardFactroy[] factories =
        {
        new GoldReardFactory(),
        new ItemRewardFactory(),
        new ModifRewardFactory()
        };

        public IReward Spawn()
        {
            return factories[rand.Next(factories.Length)].get();
        }

    }
    

    
}
