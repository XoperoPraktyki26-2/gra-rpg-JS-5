using System;

namespace BibliotekaRPG.Inventory.Items;

public class ItemFactory
{
    IItemFactory[] itemFactory = new  IItemFactory[]{
        new HPotionFactory()
    };
    Random rng;

    public IItem CreateItem()
    {
        return itemFactory[0].CreateItem();
    }
    
}