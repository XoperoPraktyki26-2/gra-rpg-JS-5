using System.Collections.Generic;
using BibliotekaRPG.Inventory;
using BibliotekaRPG.Inventory.Decorators;

public class Player : Character
{
    private int experience;
    private int experienceToNextLevel;
    private int gold = 0;

    public int Experience => experience;
    public int ExperienceToNextLevel => experienceToNextLevel;
    public int Gold => gold;
    public List<IItem> Inventory => items;
    private EquipmentItem equippedWeapon;
    private EquipmentItem equippedArmor;
    public IItem EquippedWeapon => equippedWeapon;
    public IItem EquippedArmor => equippedArmor;

    public Player(string name, int health, int maxHealth,
        int attackPower, int level, int exp,
        int expToNext, IAttackInterface atack)
        : base(name, health, maxHealth, attackPower, level, atack)
    {
        experience = exp;
        experienceToNextLevel = expToNext;
    }

    public void ReciveGold(int amm)
    {
        gold += amm;
    }

    public void GetExp(int amount)
    {
        experience += amount;
        while (experience >= experienceToNextLevel)
            LevelUp();
    }

    private void LevelUp()
    {
        experience -= experienceToNextLevel;
        Level++;
        BaseHealth += 5;
        BaseAttack += 2;
        Health = MaxHealth;
        experienceToNextLevel = (int)(experienceToNextLevel * 1.4);

        foreach (var listener in listeners)
            listener.OnLevelUp(this);
    }

    public void Equip(EquipmentItem item)
    {
        if (item == null)
            return;

        if (Equipment == null)
            Equipment = new Decorator();

        switch (item.Slot)
        {
            case EquipmentSlot.Weapon:
                SwapEquipment(ref equippedWeapon, item);
                break;
            case EquipmentSlot.Armor:
                SwapEquipment(ref equippedArmor, item);
                break;
        }
    }

    private void SwapEquipment(ref EquipmentItem slot, EquipmentItem newItem)
    {
        if (slot != null)
        {
            Equipment.Remove(slot);
            Inventory.Add(slot);
        }

        slot = newItem;
        Equipment.PutOn(newItem);
    }
}
