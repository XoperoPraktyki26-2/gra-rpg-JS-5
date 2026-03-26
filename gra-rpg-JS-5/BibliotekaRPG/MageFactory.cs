using System.Collections.Generic;
using BibliotekaRPG.Inventory;

public class MageFactory : IEnemyFactory
{
    public Enemy CreateEnemy()
        => new Enemy("Mage", 20, 20, 15, 1, 34, 12, BuildLoot(), new MagicAttack());

    private static IEnumerable<IItem> BuildLoot()
    {
        return new List<IItem>
        {
            new HPotion("Magiczna mikstura", 18),
            new EquipmentItem("Apprentice Staff", EquipmentSlot.Weapon, 6, 2),
            new EquipmentItem("Silken Robe", EquipmentSlot.Armor, 0, 6)
        };
    }
}
