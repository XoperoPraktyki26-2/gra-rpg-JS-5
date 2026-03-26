public interface IStatModifier
{
    string Name { get; }
    int ModifyAttack();
    int ModifyHealth();
}