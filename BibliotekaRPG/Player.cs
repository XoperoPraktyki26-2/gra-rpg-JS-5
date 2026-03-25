using System.Collections.Generic;
using BibliotekaRPG.Inventory;
using BibliotekaRPG.Inventory.Decorators;

public class Player : Character
{
    public const int MaxWeaponSlots = 2;
    public const int MaxArmorSlots = 4;

    private int experience;
    private int experienceToNextLevel;
    private int gold = 0;

    public int Experience => experience;
    public int ExperienceToNextLevel => experienceToNextLevel;
    public int Gold => gold;
    public List<IItem> Inventory => items;
    private readonly List<EquipmentItem> equippedWeapons = new();
    private readonly List<EquipmentItem> equippedArmors = new();
    public IReadOnlyList<EquipmentItem> EquippedWeapons => equippedWeapons.AsReadOnly();
    public IReadOnlyList<EquipmentItem> EquippedArmors => equippedArmors.AsReadOnly();

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

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0 || gold < amount)
            return false;

        gold -= amount;
        return true;
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

        Inventory.Remove(item);

        switch (item.Slot)
        {
            case EquipmentSlot.Weapon:
                EquipToSlot(equippedWeapons, MaxWeaponSlots, item);
                break;
            case EquipmentSlot.Armor:
                EquipToSlot(equippedArmors, MaxArmorSlots, item);
                break;
        }
    }

    private void EquipToSlot(List<EquipmentItem> slotList, int slotLimit, EquipmentItem newItem)
    {
        if (slotList.Count >= slotLimit)
        {
            var replaced = slotList[0];
            slotList.RemoveAt(0);
            Equipment.Remove(replaced);
            Inventory.Add(replaced);
        }

        slotList.Add(newItem);
        Equipment.PutOn(newItem);
    }

    public void ResetEquippedItems()
    {
        equippedWeapons.Clear();
        equippedArmors.Clear();
    }

    public void RegisterEquippedItem(EquipmentItem item)
    {
        if (item == null)
            return;

        switch (item.Slot)
        {
            case EquipmentSlot.Weapon:
                equippedWeapons.Add(item);
                break;
            case EquipmentSlot.Armor:
                equippedArmors.Add(item);
                break;
        }
    }

    public bool Unequip(EquipmentItem item)
    {
        if (item == null || Equipment == null)
            return false;

        bool removed = false;
        switch (item.Slot)
        {
            case EquipmentSlot.Weapon:
                removed = equippedWeapons.Remove(item);
                break;
            case EquipmentSlot.Armor:
                removed = equippedArmors.Remove(item);
                break;
        }

        if (!removed)
            return false;

        Equipment.Remove(item);
        Inventory.Add(item);
        return true;
    }
}
