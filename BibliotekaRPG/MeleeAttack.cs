public class MeleeAttack : IAttackInterface
{
    public void Attack(Character player, Character target)
    {
        int damage = player.AttackPower;
        target.Health -= damage;
        
    }
}