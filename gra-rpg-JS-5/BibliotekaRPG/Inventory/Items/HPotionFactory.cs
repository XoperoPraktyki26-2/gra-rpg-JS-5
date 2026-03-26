using System;

namespace BibliotekaRPG.Inventory.Items;

public class HPotionFactory : IItemFactory
{
    Random rng = new Random();

    private string[] name =
    {
        "duża potka",
        "mała potka"
    };

    private int[] amount =
    {
        24,
        49
    };
    public IItem CreateItem()
    {
        int index = rng.Next(0,name.Length);
        return new HPotion(name[index], amount[index]);
    }
}