using System;
using System.Collections.Generic;
using BibliotekaRPG.Inventory.Decorators;
using BibliotekaRPG.map;

public class ConsoleLogger : IGameEventListener
{
    public void OnAttack(Character attacker, Character target, int damage)
    {
        Console.WriteLine($"{attacker.Name} atakuje {target.Name} za {damage}. HP celu: {target.Health}");
    }

    public void OnLevelUp(Player player)
    {
        Console.WriteLine($"LEVEL UP! {player.Name} osiągnął poziom {player.Level}!");
    }

    public void OnBattleStart(Enemy enemy)
    {
        Console.WriteLine($"Walka z {enemy.Name}!");
    }

    public void OnEnemyDefeated(Enemy enemy)
    {
        Console.WriteLine($"Pokonałeś {enemy.Name}!");
    }

    public void OnPlayerDefeated(Player player)
    {
        Console.WriteLine($"{player.Name} poległ. Git Gut.");
    }

    public void OnItemUsed(Character user, IItem item)
    {
        Console.WriteLine($"{user.Name} używa {item.Name}");
    }

    public void OnEquipmentPutOn(IStatModifier item)
    {
        Console.WriteLine("Założono: " + item.Name);
    }

    public void OnShowStats(Player player)
    {
        Console.WriteLine($"HP: {player.Health}/{player.MaxHealth} | Atak: {player.AttackPower} | Level: {player.Level}");
    }

    public void OnShowInventory(List<IItem> items)
    {
        Console.WriteLine("\n--- Przedmioty ---");
        if (items.Count == 0)
        {
            Console.WriteLine("Brak przedmiotów.");
            return;
        }

        for (int i = 0; i < items.Count; i++)
            Console.WriteLine($"{i}. {items[i].Name}");
    }

    public void OnShowEquipment(IStatModifier[] items)
    {
        Console.WriteLine("\n--- Założone przedmioty ---");

        bool empty = true;
        foreach (var m in items)
        {
            if (m != null)
            {
                Console.WriteLine("- " + m.Name);
                empty = false;
            }
        }

        if (empty)
            Console.WriteLine("Brak założonych przedmiotów.");
    }

    public void OnEquipmentSlotsFull()
    {
        Console.WriteLine("Brak Wolnego slotu"); ;
    }

    public void ShowMap(WorldMap map)
    {
        for (int i = 0; i < map.grid.GetLength(0); i++)
        {
            for (int j = 0; j < map.grid.GetLength(1); j++)
            {
                Console.Write(map.grid[i, j].Type.ToString() + " ");
            }
            Console.WriteLine();
        }
    }
}
