using System.Collections.Generic;
using BibliotekaRPG.Inventory;

public class GoblinFactory : IEnemyFactory
{
    public Enemy CreateEnemy()
        => new Enemy("Goblin", 45, 45, 5, 1, 15, 8, BuildLoot(), new MeleeAttack());

    private static IEnumerable<IItem> BuildLoot()
    {
        return new List<IItem>
        {
            new HPotion("Mała mikstura", 24),
            new EquipmentItem("Gnoll Dagger", EquipmentSlot.Weapon, 3, 1),
            new EquipmentItem("Leather Vest", EquipmentSlot.Armor, 0, 4)
        };
    }
}
