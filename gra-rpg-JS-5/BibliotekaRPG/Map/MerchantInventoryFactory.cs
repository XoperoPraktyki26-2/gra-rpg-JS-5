using System;
using System.Collections.Generic;
using BibliotekaRPG.Inventory;

namespace BibliotekaRPG.map
{
    public class MerchantInventoryFactory
    {
        private readonly Random rng;

        public MerchantInventoryFactory(Random? rng = null)
        {
            this.rng = rng ?? new Random();
        }

        public List<MerchantOffer> CreateOffers()
        {
            var offers = new List<MerchantOffer>();

            for (int i = 0; i < 2; i++)
            {
                offers.Add(CreatePotionOffer());
            }

            for (int i = 0; i < 3; i++)
            {
                offers.Add(CreateEquipmentOffer());
            }

            return offers;
        }

        private MerchantOffer CreatePotionOffer()
        {
            var heal = rng.Next(18, 61);
            var potion = new HPotion($"Potka leczenia {heal}", heal);
            var price = 10 + heal * 2;
            return new MerchantOffer(potion, price);
        }

        private MerchantOffer CreateEquipmentOffer()
        {
            var isWeapon = rng.NextDouble() < 0.5;
            var slot = isWeapon ? EquipmentSlot.Weapon : EquipmentSlot.Armor;
            var attack = isWeapon ? rng.Next(3, 13) : rng.Next(0, 5);
            var health = isWeapon ? rng.Next(0, 7) : rng.Next(6, 19);
            var namePrefix = isWeapon ? "Broń kupca" : "Zbroja kupca";
            var item = new EquipmentItem($"{namePrefix} {rng.Next(100, 999)}", slot, attack, health);
            var price = 40 + (attack * 9) + (health * 4);

            return new MerchantOffer(item, price);
        }
    }
}
