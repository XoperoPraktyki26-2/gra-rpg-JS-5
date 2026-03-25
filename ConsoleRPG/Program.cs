using System;
using BibliotekaRPG;
using BibliotekaRPG.map;
using BibliotekaRPG.Inventory;
using System.Collections.Generic;

class Program
{
    private static Saver saver = new Saver();

    public static void Main(string[] args)
    {
        var session = new MapSession();
        var logger = new ConsoleLogger();

        session.RegisterListener(logger);
        session.Log += message => Console.WriteLine(message);
        session.MapChanged += () => PrintMap(session);

        PrintMap(session);
        RunConsoleLoop(session, logger);
    }

    private static void RunConsoleLoop(MapSession session, ConsoleLogger logger)
    {
        while (true)
        {
            DisplayStatus(session);

            Console.WriteLine("Komendy:");
            Console.WriteLine("W/A/S/D – ruch");
            Console.WriteLine("G – idź do");
            Console.WriteLine("M – atak wręcz");
            Console.WriteLine("K – atak magiczny");
            Console.WriteLine("I – przedmioty");
            Console.WriteLine("U – użyj itemu");
            Console.WriteLine("E – załóż ekwipunek");
            Console.WriteLine("N – zdejmij ekwipunek");
            Console.WriteLine("P – panel statystyk");
            Console.WriteLine("H – handel z kupcem");
            Console.WriteLine("Z – zapisz grę");
            Console.WriteLine("L – wczytaj grę");
            Console.WriteLine("R – cofnij turę");
            Console.WriteLine("Q – wyjście");

            var keyInfo = Console.ReadKey(true);
            var keyChar = char.ToLower(keyInfo.KeyChar);

            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                    session.TryMove(-1, 0);
                    continue;
                case ConsoleKey.DownArrow:
                    session.TryMove(1, 0);
                    continue;
                case ConsoleKey.LeftArrow:
                    session.TryMove(0, -1);
                    continue;
                case ConsoleKey.RightArrow:
                    session.TryMove(0, 1);
                    continue;
            }

            switch (keyChar)
            {
                case 'w': session.TryMove(-1, 0); break;
                case 's': session.TryMove(1, 0); break;
                case 'a': session.TryMove(0, -1); break;
                case 'd': session.TryMove(0, 1); break;

                case 'm': PerformAttack(session, logger, true); break;
                case 'k': PerformAttack(session, logger, false); break;

                case 'i': session.Player.ListItems(); break;
                case 'u': UseItem(session); break;
                case 'e': EquipItem(session); break;
                case 'n': UnequipItem(session); break;
                case 'p': ShowDetailedStats(session); break;
                case 'h': TradeWithMerchant(session); break;

                case 'g': MoveToCoordinates(session); break;

                case 'z': SaveGame(session); break;
                case 'l': LoadGame(session); break;
                case 'r': UndoTurn(session); break;

                case 'q': return;
            }
        }
    }

    private static void SaveGame(MapSession session)
    {
        var snapshot = session.CreateSnapshot();
        saver.Save(snapshot);
        Console.WriteLine("Gra zapisana.");
    }

    private static void LoadGame(MapSession session)
    {
        var loaded = saver.Load();
        if (loaded != null)
        {
            session.LoadSnapshot(loaded);
            Console.WriteLine("Wczytano zapis gry.");
            PrintMap(session);
        }
        else
        {
            Console.WriteLine("Brak zapisu gry.");
        }
    }

    private static void UndoTurn(MapSession session)
    {
        if (session.UndoTurn())
        {
            Console.WriteLine("Cofnięto turę.");
            PrintMap(session);
        }
        else
        {
            Console.WriteLine("Nie udało się cofnąć tury.");
        }
    }

    private static void MoveToCoordinates(MapSession session)
    {
        Console.Write("\nPodaj rząd (Row): ");
        if (!int.TryParse(Console.ReadLine(), out int r))
            return;

        Console.Write("Podaj kolumnę (Col): ");
        if (!int.TryParse(Console.ReadLine(), out int c))
            return;

        var path = session.PathFinder.FindPath(session.PlayerPosition, (r, c));
        if (path == null)
        {
            Console.WriteLine("Nie znaleziono drogi.");
            return;
        }

        foreach (var step in path)
        {
            if (session.HasActiveBattle)
            {
                Console.WriteLine("Zatrzymano: walka!");
                break;
            }

            int dr = step.Row - session.PlayerPosition.Row;
            int dc = step.Col - session.PlayerPosition.Col;

            if (!session.TryMove(dr, dc))
                break;

            System.Threading.Thread.Sleep(200);

            if (session.HasActiveBattle)
                break;
        }
    }

    private static void PerformAttack(MapSession session, ConsoleLogger logger, bool melee)
    {
        if (session.CurrentEnemy == null || !session.CurrentEnemy.IsAlive())
            session.SpawnEnemy();

        var enemy = session.CurrentEnemy;
        if (enemy == null)
            return;

        session.Player.ChooseAttack(melee ? new MeleeAttack() : new MagicAttack());
        session.Player.PerformAttack(enemy);

        if (!enemy.IsAlive())
        {
            session.ResolveEnemyDefeat(enemy);
            return;
        }

        enemy.PerformAttack(session.Player);

        if (!session.Player.IsAlive())
        {
            logger.OnPlayerDefeated(session.Player);
            Environment.Exit(0);
        }
    }

    private static void UseItem(MapSession session)
    {
        session.Player.ListItems();
        Console.Write("Wybierz indeks przedmiotu do użycia: ");

        if (int.TryParse(Console.ReadLine(), out var index))
            session.Player.UseItem(index);
    }

    private static void EquipItem(MapSession session)
    {
        var inventory = session.Player.Inventory;
        var equipmentChoices = new List<(int InventoryIndex, EquipmentItem Item)>();

        Console.WriteLine("\n--- Ekwipunek w plecaku ---");

        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] is not EquipmentItem eq)
                continue;

            equipmentChoices.Add((i, eq));
            var slotName = eq.Slot == EquipmentSlot.Weapon ? "Broń" : "Zbroja";
            Console.WriteLine(
                $"{equipmentChoices.Count - 1}. {eq.Name} ({slotName}, ATK +{eq.ModifyAttack()}, HP +{eq.ModifyHealth()})"
            );
        }

        if (equipmentChoices.Count == 0)
        {
            Console.WriteLine("Brak elementów ekwipunku w plecaku.");
            return;
        }

        Console.Write("Wybierz indeks do założenia: ");
        if (!int.TryParse(Console.ReadLine(), out var choiceIndex))
            return;

        if (choiceIndex < 0 || choiceIndex >= equipmentChoices.Count)
        {
            Console.WriteLine("Niepoprawny indeks.");
            return;
        }

        var selected = equipmentChoices[choiceIndex].Item;
        session.Player.Equip(selected);
        Console.WriteLine($"Założono: {selected.Name}");
    }

    private static void UnequipItem(MapSession session)
    {
        var equippedChoices = new List<EquipmentItem>();

        Console.WriteLine("\n--- Założony ekwipunek ---");

        foreach (var eq in session.Player.EquippedWeapons)
            equippedChoices.Add(eq);

        foreach (var eq in session.Player.EquippedArmors)
            equippedChoices.Add(eq);

        for (int i = 0; i < equippedChoices.Count; i++)
        {
            var eq = equippedChoices[i];
            var slotName = eq.Slot == EquipmentSlot.Weapon ? "Broń" : "Zbroja";
            Console.WriteLine($"{i}. {eq.Name} ({slotName}, ATK +{eq.ModifyAttack()}, HP +{eq.ModifyHealth()})");
        }

        if (equippedChoices.Count == 0)
        {
            Console.WriteLine("Brak założonych przedmiotów.");
            return;
        }

        Console.Write("Wybierz indeks do zdjęcia: ");
        if (!int.TryParse(Console.ReadLine(), out var choiceIndex))
            return;

        if (choiceIndex < 0 || choiceIndex >= equippedChoices.Count)
        {
            Console.WriteLine("Niepoprawny indeks.");
            return;
        }

        var selected = equippedChoices[choiceIndex];
        if (session.Player.Unequip(selected))
            Console.WriteLine($"Zdjęto: {selected.Name}");
        else
            Console.WriteLine("Nie udało się zdjąć przedmiotu.");
    }

    private static void ShowDetailedStats(MapSession session)
    {
        var player = session.Player;

        Console.WriteLine("\n--- Panel statystyk ---");
        Console.WriteLine($"HP: {player.Health}/{player.MaxHealth}");
        Console.WriteLine($"Atak: {player.AttackPower}");
        Console.WriteLine($"Level: {player.Level}");
        Console.WriteLine($"XP: {player.Experience}/{player.ExperienceToNextLevel}");
        Console.WriteLine($"Złoto: {player.Gold}");
        Console.WriteLine($"Tokeny cofania: {session.RewindTokens}");
        Console.WriteLine($"Tura: {session.TurnCount}");

        Console.WriteLine("\nBroń:");
        if (player.EquippedWeapons.Count == 0)
            Console.WriteLine("- brak");
        else
        {
            foreach (var eq in player.EquippedWeapons)
                Console.WriteLine($"- {eq.Name} (ATK +{eq.ModifyAttack()}, HP +{eq.ModifyHealth()})");
        }

        Console.WriteLine("\nZbroja:");
        if (player.EquippedArmors.Count == 0)
            Console.WriteLine("- brak");
        else
        {
            foreach (var eq in player.EquippedArmors)
                Console.WriteLine($"- {eq.Name} (ATK +{eq.ModifyAttack()}, HP +{eq.ModifyHealth()})");
        }
    }

    private static void DisplayStatus(MapSession session)
    {
        var player = session.Player;
        var enemy = session.CurrentEnemy;

        Console.WriteLine(
            $"\nPozycja: {session.PlayerPosition.Row},{session.PlayerPosition.Col} | " +
            $"HP: {player.Health}/{player.MaxHealth} | " +
            $"Lvl: {player.Level} | " +
            $"Złoto: {player.Gold} | " +
            $"Tokeny cofania: {session.RewindTokens} | " +
            $"Tura: {session.TurnCount}"
        );

        if (enemy != null && enemy.IsAlive())
        {
            Console.WriteLine(
                $"Przeciwnik: {enemy.Name} | HP: {enemy.Health}/{enemy.MaxHealth} | Lvl: {enemy.Level}"
            );
        }
        else
        {
            Console.WriteLine("Przeciwnik: brak");
        }
    }

    private static void PrintMap(MapSession session)
    {
        var map = session.Map;

        Console.WriteLine("\nMapa:");
        PrintColumnHeaders(map.Size);
        Console.WriteLine("   +" + new string('-', map.Size * 3) + "+");

        for (int row = 0; row < map.Size; row++)
        {
            Console.Write($"{row,2} |");

            for (int col = 0; col < map.Size; col++)
            {
                var tile = map.GetTile(row, col);
                char symbol = GetSymbolForTile(tile);

                if (session.PlayerPosition.Row == row && session.PlayerPosition.Col == col)
                    symbol = 'P';

                Console.Write($" {symbol} ");
            }

            Console.WriteLine("|");
        }

        Console.WriteLine("   +" + new string('-', map.Size * 3) + "+");
        PrintColumnHeaders(map.Size);
    }

    private static void PrintColumnHeaders(int size)
    {
        Console.Write("    ");
        for (int col = 0; col < size; col++)
            Console.Write($"{col,3}");
        Console.WriteLine();
    }

    private static char GetSymbolForTile(ITile tile)
    {
        return tile.Type switch
        {
            ITile.TileType.Grass => 'G',
            ITile.TileType.Forest => 'F',
            ITile.TileType.Mountain => 'M',
            ITile.TileType.EnemySpawn => 'E',
            ITile.TileType.Treasure => 'T',
            ITile.TileType.Empty => '.',
            ITile.TileType.Merchant => 'K',
            _ => '?'
        };
    }

    private static void TradeWithMerchant(MapSession session)
    {
        var merchant = session.GetMerchantAtPlayerPosition();
        if (merchant == null)
        {
            Console.WriteLine("Nie stoisz na polu kupca.");
            return;
        }

        if (merchant.Offers.Count == 0)
        {
            Console.WriteLine("Kupiec nie ma już towaru.");
            return;
        }

        Console.WriteLine("\n--- Oferty kupca ---");
        for (int i = 0; i < merchant.Offers.Count; i++)
        {
            var offer = merchant.Offers[i];
            Console.WriteLine($"{i}. {offer.Item.Name} - {offer.Price} zł");
        }

        Console.Write("Podaj indeks oferty do kupienia: ");
        if (!int.TryParse(Console.ReadLine(), out var index))
            return;

        if (session.BuyFromMerchant(index, out var message))
            Console.WriteLine(message);
        else
            Console.WriteLine(message);
    }
}
