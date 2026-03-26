using System;
using System.Collections.Generic;
using System.Text;

namespace BibliotekaRPG.Rewards
{
    public interface IRewardFactroy
    {
        IReward get();
    }
}
