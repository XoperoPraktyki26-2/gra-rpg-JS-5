public class MagicAttack : IAttackInterface
{
    public void Attack(Character player, Character target)
    {
        int damage = player.AttackPower + 5;
        target.Health -= damage;
    }
}