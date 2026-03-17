using System;
using System.Collections.Generic;
using BibliotekaRPG.Inventory;
using BibliotekaRPG.Inventory.Decorators;

namespace BibliotekaRPG.map
{
    public class MapSession
    {
        private readonly Player player;
        private readonly Decorator decorator;
        private readonly Factory factory;
        private readonly WorldMap worldMap;
        private readonly Random encounterRng;
        private readonly Random lootRng;
        private readonly Dictionary<ITile.TileType, double> encounterChances;
        private readonly List<IGameEventListener> gameEventListeners = new();

        public event Action<string>? Log;
        public event Action? MapChanged;
        public event Action<Enemy>? BattleStarted;
        public event Action<Enemy>? EnemyRewardsProcessed;

        public Player Player => player;
        public Decorator Equipment => decorator;
        public WorldMap Map => worldMap;
        public (int Row, int Col) PlayerPosition { get; private set; }
        public Enemy CurrentEnemy { get; private set; }
        public bool HasActiveBattle => CurrentEnemy != null && CurrentEnemy.IsAlive();

        public MapSession(Random? encounterRng = null, Random? lootRng = null)
        {
            worldMap = new WorldMap();
            factory = new Factory();
            decorator = new Decorator();
            player = new Player("Bohater", 35, 35, 10, 1, 0, 20, new MeleeAttack());
            player.Equipment = decorator;

            PlayerPosition = worldMap.PlayerStart;

            encounterChances = new Dictionary<ITile.TileType, double>
            {
                [ITile.TileType.Grass] = 0.10,
                [ITile.TileType.Forest] = 0.25,
                [ITile.TileType.Empty] = 0.0
            };

            this.encounterRng = encounterRng ?? new Random();
            this.lootRng = lootRng ?? new Random();

            SeedStartingInventory();
        }

        public void RegisterListener(IGameEventListener listener)
        {
            if (listener == null || gameEventListeners.Contains(listener))
                return;

            gameEventListeners.Add(listener);
            player.AddListener(listener);
            decorator.AddListener(listener);
        }

        public bool TryMove(int deltaRow, int deltaCol)
        {
            if (HasActiveBattle)
            {
                SendLog("Najpierw zakończ obecną walkę.");
                return false;
            }

            int targetRow = PlayerPosition.Row + deltaRow;
            int targetCol = PlayerPosition.Col + deltaCol;

            if (!worldMap.IsWithinBounds(targetRow, targetCol))
            {
                SendLog("Nie możesz wyjść poza mapę.");
                return false;
            }

            if (!worldMap.IsWalkable(targetRow, targetCol))
            {
                SendLog("To pole jest niedostępne.");
                return false;
            }

            PlayerPosition = (targetRow, targetCol);
            MapChanged?.Invoke();

            var tile = worldMap.GetTile(targetRow, targetCol);
            return HandleTileEnter(tile, targetRow, targetCol);
        }

        public Enemy SpawnEnemy()
        {
            StartBattleAt(PlayerPosition.Row, PlayerPosition.Col, clearTile: false);
            return CurrentEnemy;
        }

        public void ResolveEnemyDefeat(Enemy enemy)
        {
            if (enemy == null)
                return;

            if (enemy.GoldReward > 0)
            {
                player.ReciveGold(enemy.GoldReward);
                SendLog($"Zdobyto {enemy.GoldReward} złota.");
            }

            if (enemy.LootTable.Count > 0)
            {
                int drops = lootRng.Next(enemy.LootTable.Count + 1);

                for (int i = 0; i < drops; i++)
                {
                    var template = enemy.LootTable[lootRng.Next(enemy.LootTable.Count)];
                    var loot = template.Clone();
                    player.AddItem(loot);
                    SendLog($"Zdobyto przedmiot: {loot.Name}");
                }
            }

            player.GetExp(enemy.ExperienceReward);
            CurrentEnemy = null;
            NotifyEnemyDefeated(enemy);
            MapChanged?.Invoke();
            EnemyRewardsProcessed?.Invoke(enemy);
        }

        private void SeedStartingInventory()
        {
            for (int i = 0; i < 3; i++)
            {
                player.AddItem(new HPotion("Mała mikstura", 24));
                player.AddItem(new HPotion("Duża mikstura", 56));
            }

            decorator.PutOn(new ArmorPiece("Sword", 11, 3));
            decorator.PutOn(new ArmorPiece("Chestplate", 0, 15));
            decorator.PutOn(new ArmorPiece("Sword", 11, 3));
            decorator.PutOn(new ArmorPiece("Chestplate", 0, 15));
            decorator.PutOn(new ArmorPiece("Sword", 11, 3));
            decorator.PutOn(new ArmorPiece("Chestplate", 0, 15));
        }

        private bool HandleTileEnter(ITile tile, int row, int col)
        {
            switch (tile.Type)
            {
                case ITile.TileType.EnemySpawn:
                    SendLog("Natrafiono na przeciwnika!");
                    StartBattleAt(row, col, clearTile: true);
                    return true;

                case ITile.TileType.Treasure:
                    SendLog("Natrafiono na skrzynię!");
                    if (tile is Treasure treasure)
                    {
                        treasure.Entered(player);
                        SendLog("Znaleziono nagrodę.");
                        MapChanged?.Invoke();
                    }

                    worldMap.ReplaceTile(row, col, new Grass());
                    MapChanged?.Invoke();
                    return false;

                default:
                    return TryStartRandomEncounter(tile, row, col);
            }
        }

        private void StartBattleAt(int row, int col, bool clearTile)
        {
            if (clearTile)
            {
                worldMap.ReplaceTile(row, col, new Grass());
                MapChanged?.Invoke();
            }

            var enemy = factory.Spawn();
            CurrentEnemy = enemy;

            foreach (var listener in gameEventListeners)
                enemy.AddListener(listener);

            SendLog($"Walka z {enemy.Name}!");
            NotifyBattleStart(enemy);
            MapChanged?.Invoke();
        }

        private bool TryStartRandomEncounter(ITile tile, int row, int col)
        {
            if (!encounterChances.TryGetValue(tile.Type, out var chance) || chance <= 0)
                return false;

            if (encounterRng.NextDouble() < chance)
            {
                SendLog($"Spotkałeś przeciwnika ({tile.Type})! Nie możesz ruszyć się dopóki go nie pokonasz.");
                StartBattleAt(row, col, clearTile: false);
                return true;
            }

            return false;
        }

        private void NotifyBattleStart(Enemy enemy)
        {
            foreach (var listener in gameEventListeners)
                listener.OnBattleStart(enemy);
            BattleStarted?.Invoke(enemy);
        }

        private void NotifyEnemyDefeated(Enemy enemy)
        {
            foreach (var listener in gameEventListeners)
                listener.OnEnemyDefeated(enemy);
        }

        private void SendLog(string message)
        {
            Log?.Invoke(message);
        }

    }
}
