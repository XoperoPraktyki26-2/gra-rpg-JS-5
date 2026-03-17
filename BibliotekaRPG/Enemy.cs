using System.Collections.Generic;
using BibliotekaRPG.Inventory;

public class Enemy : Character
{
    public int ExperienceReward { get; set; }
    public List<IItem> LootTable { get; }
    public int GoldReward { get; }

    public Enemy(string name, int health, int maxHealth,
        int attackPower, int level, int expReward,
        int goldReward, IEnumerable<IItem> lootTable, IAttackInterface atack)
        : base(name, health, maxHealth, attackPower, level, atack)
    {
        ExperienceReward = expReward;
        GoldReward = goldReward;
        LootTable = new List<IItem>(lootTable ?? new IItem[0]);
    }
}
