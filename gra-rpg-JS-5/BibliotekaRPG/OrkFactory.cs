using System.Collections.Generic;
using BibliotekaRPG.Inventory;

public class OrkFactory : IEnemyFactory
{
    public Enemy CreateEnemy()
        => new Enemy("Ork", 80, 80, 30, 1, 34, 20, BuildLoot(), new MeleeAttack());

    private static IEnumerable<IItem> BuildLoot()
    {
        return new List<IItem>
        {
            new HPotion("Wojenny eliksir", 35),
            new EquipmentItem("Great Axe", EquipmentSlot.Weapon, 9, 0),
            new EquipmentItem("Scale Mail", EquipmentSlot.Armor, 0, 8)
        };
    }
}
