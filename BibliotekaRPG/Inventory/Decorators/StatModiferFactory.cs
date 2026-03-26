using System;

namespace BibliotekaRPG.Inventory.Decorators;

public class StatModiferFactory
{
    private readonly IStatmodierFactory[] itemFactory = {
        new ArmorFactory(),
        new WeaponFactory()
    };
    private readonly Random rng = new Random();

    public IStatModifier CreateItem()
    {
        return itemFactory[rng.Next(0, itemFactory.Length)].CreateStatModifier();
    }
}
