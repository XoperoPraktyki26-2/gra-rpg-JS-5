using System.Collections.Generic;
using BibliotekaRPG.Inventory;
using BibliotekaRPG.Inventory.Decorators;

public class Character
{
    public string Name { get; }

    public int BaseHealth { get; set; }
    public int BaseAttack { get; set; }

    public int Health { get; set; }
    public int Level { get; set; }

    public Decorator Equipment { get; set; }

    public int MaxHealth => BaseHealth + (Equipment?.GetBonusHealth() ?? 0);
    public int AttackPower => BaseAttack + (Equipment?.GetBonusAttack() ?? 0);

    private IAttackInterface attackStrategy;
    protected readonly List<IItem> items = new List<IItem>();

    public IReadOnlyList<IItem> Items => items.AsReadOnly();

    protected readonly List<IGameEventListener> listeners = new List<IGameEventListener>();

    public Character(string name, int health, int maxHealth,
        int attackPower, int level, IAttackInterface atack)
    {
        Name = name;
        BaseHealth = maxHealth;
        BaseAttack = attackPower;
        Health = health;
        Level = level;
        attackStrategy = atack;
    }

    public void AddListener(IGameEventListener listener)
    {
        listeners.Add(listener);
    }

    protected void NotifyAttack(Character target, int damage)
    {
        foreach (var listener in listeners)
            listener.OnAttack(this, target, damage);
    }

    public void ChooseAttack(IAttackInterface attack)
    {
        attackStrategy = attack;
    }

    public void PerformAttack(Character target)
    {
        int initialHP = target.Health;
        attackStrategy.Attack(this, target);
        int damageDealt = initialHP - target.Health;
        NotifyAttack(target, damageDealt);
    }

    public void AddItem(IItem item)
    {
        items.Add(item);
    }

    public void ListItems()
    {
        foreach (var listener in listeners)
            listener.OnShowInventory(items);
    }

    public void UseItem(int index)
    {
        if (index >= 0 && index < items.Count)
        {
            var item = items[index];
            item.Use(this);

            foreach (var listener in listeners)
                listener.OnItemUsed(this, item);

            if (index < items.Count && ReferenceEquals(items[index], item))
            {
                items.RemoveAt(index);
            }
            else
            {
                items.Remove(item);
            }
        }
    }

    public bool IsAlive() => Health > 0;
}
