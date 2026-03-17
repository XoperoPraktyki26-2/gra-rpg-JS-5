public interface IItem
{
    string Name { get; }
    void Use(Character player);
    IItem Clone();
}
