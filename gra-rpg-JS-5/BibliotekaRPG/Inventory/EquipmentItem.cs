using BibliotekaRPG.Inventory.Decorators;

namespace BibliotekaRPG.Inventory;

public enum EquipmentSlot
{
    Weapon,
    Armor
}

public class EquipmentItem : IItem, IStatModifier
{
    public string Name { get; }
    public EquipmentSlot Slot { get; }

    private readonly int attackBonus;
    private readonly int healthBonus;

    public EquipmentItem(string name, EquipmentSlot slot, int attackBonus, int healthBonus)
    {
        Name = name;
        Slot = slot;
        this.attackBonus = attackBonus;
        this.healthBonus = healthBonus;
    }

    public void Use(Character player)
    {
        if (player is Player hero)
            hero.Equip(this);
    }

    public int ModifyAttack() => attackBonus;
    public int ModifyHealth() => healthBonus;

    public IItem Clone()
    {
        return new EquipmentItem(Name, Slot, attackBonus, healthBonus);
    }
}
