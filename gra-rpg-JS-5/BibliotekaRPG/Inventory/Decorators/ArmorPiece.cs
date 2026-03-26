using BibliotekaRPG.Inventory.Decorators;

public class ArmorPiece : IStatModifier
{
    public string Name { get; }
    private int attackBonus;
    private int healthBonus;

    public ArmorPiece(string name, int attackBonus, int healthBonus)
    {
        this.Name = name;
        this.attackBonus = attackBonus;
        this.healthBonus = healthBonus;
    }

    

    public int ModifyAttack() => attackBonus;
    public int ModifyHealth() => healthBonus;
}