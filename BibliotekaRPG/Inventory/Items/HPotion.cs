public class HPotion : IItem
{
    public string Name { get; }
    public int howMuchHeal { get;}
    public HPotion(string name, int howMuchHeal)
    {
        this.Name = name;
        this.howMuchHeal = howMuchHeal;
    }

    public void Use(Character player)
    {
        player.Health += howMuchHeal;
    }

    public IItem Clone()
    {
        return new HPotion(Name, howMuchHeal);
    }
}
