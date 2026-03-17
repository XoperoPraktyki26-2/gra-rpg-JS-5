using System;
using System.Collections.Generic;
using System.Text;

namespace BibliotekaRPG.Rewards
{
    public interface IReward
    {
        void Apply(Player player);
    }
}
