using System.Collections.Generic;
using BibliotekaRPG.Inventory;
using BibliotekaRPG.Inventory.Decorators;

namespace BibliotekaRPG
{
    public static class PlayerExtensions
    {
        public static PlayerData ToData(this Player player)
        {
            var data = new PlayerData
            {
                Name = player.Name,
                Health = player.Health,
                BaseHealth = player.BaseHealth,
                BaseAttack = player.BaseAttack,
                Level = player.Level,
                Exp = player.Experience,
                ExpToNext = player.ExperienceToNextLevel,
                Gold = player.Gold,
                Inventory = new List<ItemData>(),
                Equipment = new List<ItemData>()
            };

            foreach (var item in player.Inventory)
                data.Inventory.Add(item.ToData());

            if (player.Equipment is Decorator decorator)
            {
                foreach (var mod in decorator.modifiers)
                {
                    if (mod is EquipmentItem eq)
                        data.Equipment.Add(eq.ToData());
                }
            }

            return data;
        }

        public static void LoadFromData(this Player player, PlayerData data)
        {
            player.BaseHealth = data.BaseHealth;
            player.BaseAttack = data.BaseAttack;
            player.Health = data.Health;
            player.Level = data.Level;

            
            var expField = typeof(Player).GetField("experience", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var expToNextField = typeof(Player).GetField("experienceToNextLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var goldField = typeof(Player).GetField("gold", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            expField?.SetValue(player, data.Exp);
            expToNextField?.SetValue(player, data.ExpToNext);
            goldField?.SetValue(player, data.Gold);

            player.Inventory.Clear();
            foreach (var itemData in data.Inventory)
                player.AddItem(itemData.ToItem());

            if (player.Equipment is Decorator decorator)
            {
                player.ResetEquippedItems();

                for (int i = 0; i < decorator.modifiers.Length; i++)
                    decorator.modifiers[i] = null;

                foreach (var eqData in data.Equipment)
                {
                    var item = eqData.ToItem();
                    if (item is IStatModifier mod)
                    {
                        decorator.PutOn(mod);
                    }

                    if (item is EquipmentItem equippedItem)
                        player.RegisterEquippedItem(equippedItem);
                }
            }
        }
    }
}
