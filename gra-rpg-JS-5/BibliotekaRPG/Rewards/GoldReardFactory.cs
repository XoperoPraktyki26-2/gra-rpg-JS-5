using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace BibliotekaRPG.Rewards
{
    public class GoldReardFactory : IRewardFactroy
    {
        Random rng = new Random();
        public IReward get()
        {
            return new GoldReward(rng.Next(1000));
        }
    }
}
