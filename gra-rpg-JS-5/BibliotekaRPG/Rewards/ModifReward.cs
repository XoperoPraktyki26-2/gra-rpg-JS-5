using System;
using System.Collections.Generic;
using System.Text;
using BibliotekaRPG.Inventory;

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
            if (md is EquipmentItem equipmentItem)
            {
                player.Equip(equipmentItem);
            }
            else
            {
                player.Equipment.PutOn(md);
            }
        }
    }
}
