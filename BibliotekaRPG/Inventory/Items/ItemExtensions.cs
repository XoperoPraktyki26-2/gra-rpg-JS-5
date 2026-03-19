using BibliotekaRPG.Inventory;
using BibliotekaRPG.Inventory.Decorators;

namespace BibliotekaRPG
{
    public static class ItemExtensions
    {
        public static ItemData ToData(this IItem item)
        {
            if (item is HPotion pot)
            {
                return new ItemData
                {
                    ItemType = "HPotion",
                    Name = pot.Name,
                    HealAmount = pot.howMuchHeal
                };
            }

            if (item is EquipmentItem eq)
            {
                return new ItemData
                {
                    ItemType = "Equipment",
                    Name = eq.Name,
                    Slot = eq.Slot.ToString(),
                    AttackBonus = eq.ModifyAttack(),
                    HealthBonus = eq.ModifyHealth()
                };
            }

            return new ItemData
            {
                ItemType = "Unknown",
                Name = item.Name
            };
        }

        public static IItem ToItem(this ItemData data)
        {
            switch (data.ItemType)
            {
                case "HPotion":
                    return new HPotion(data.Name, data.HealAmount);

                case "Equipment":
                    var slot = data.Slot == "Weapon" ? EquipmentSlot.Weapon : EquipmentSlot.Armor;
                    return new EquipmentItem(data.Name, slot, data.AttackBonus, data.HealthBonus);

                default:
                    return new HPotion(data.Name, 10);
            }
        }
    }
}