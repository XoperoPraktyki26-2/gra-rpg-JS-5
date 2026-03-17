using System;
using System.Collections.Generic;
using System.Text;

namespace BibliotekaRPG.Rewards
{
    public class ModifRewardFactory: IRewardFactroy
    {
        Random rng = new Random();
        public IReward get()
        {
            return new ModifReward(new ArmorPiece("jakiś mieczyk",5,1));
        }
    }
}
