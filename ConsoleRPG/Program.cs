using System;
using BibliotekaRPG.map;

class Program
{
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

            Console.WriteLine("Komendy: W/A/S/D – ruch, G – idź do, M – atak wręcz, K – atak magiczny, I – przedmioty, U – użyj itemu, Q – wyjście");
            var key = Console.ReadKey(true).KeyChar;

            switch (char.ToLower(key))
            {
                case 'w':
                    session.TryMove(-1, 0);
                    break;
                case 's':
                    session.TryMove(1, 0);
                    break;
                case 'a':
                    session.TryMove(0, -1);
                    break;
                case 'd':
                    session.TryMove(0, 1);
                    break;
                case 'm':
                    PerformAttack(session, logger, true);
                    break;
                case 'k':
                    PerformAttack(session, logger, false);
                    break;
                case 'i':
                    session.Player.ListItems();
                    break;
                case 'u':
                    UseItem(session);
                    break;
                case 'g':
                    MoveToCoordinates(session);
                    break;
                case 'q':
                    return;
            }
        }
    }

    private static void MoveToCoordinates(MapSession session)
    {
        Console.Write("\nPodaj rząd (Row): ");
        if (int.TryParse(Console.ReadLine(), out int r))
        {
            Console.Write("Podaj kolumnę (Col): ");
            if (int.TryParse(Console.ReadLine(), out int c))
            {
                var path = session.PathFinder.FindPath(session.PlayerPosition, (r, c));
                if (path != null)
                {
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
                        
                        // Wait 0.2s
                        System.Threading.Thread.Sleep(200);
                        
                        // Redraw is handled by TryMove -> MapChanged -> PrintMap
                        // Check if we are in battle after move
                         if (session.HasActiveBattle)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Nie znaleziono drogi.");
                }
            }
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
        {
            session.Player.UseItem(index);
        }
    }

    private static void DisplayStatus(MapSession session)
    {
        var player = session.Player;

        Console.WriteLine($"\nPozycja: {session.PlayerPosition.Row},{session.PlayerPosition.Col} | HP: {player.Health}/{player.MaxHealth} | Lvl: {player.Level} | Złoto: {player.Gold}");
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
            _ => '?'
        };
    }
}