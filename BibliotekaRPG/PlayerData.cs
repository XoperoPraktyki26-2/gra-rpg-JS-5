using System.Collections.Generic;

public class PlayerData
{
    public string Name { get; set; }
    public int Health { get; set; }
    public int BaseHealth { get; set; }
    public int BaseAttack { get; set; }
    public int Level { get; set; }
    public int Exp { get; set; }
    public int ExpToNext { get; set; }
    public int Gold { get; set; }

    public List<ItemData> Inventory { get; set; }
    public List<ItemData> Equipment { get; set; }
}