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
        private readonly Random merchantRng;
        private readonly MerchantInventoryFactory merchantInventoryFactory;
        private readonly Dictionary<ITile.TileType, double> encounterChances;
        private readonly List<IGameEventListener> gameEventListeners = new();

        private readonly Stack<GameState> history = new();
        private int rewindTokens = 3;
        private int turnCount;

        public int RewindTokens => rewindTokens;
        public int TurnCount => turnCount;

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
        public PathFinding PathFinder { get; private set; }

        public MapSession(Random? encounterRng = null, Random? lootRng = null)
        {
            worldMap = new WorldMap();
            PathFinder = new PathFinding(worldMap);
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
            merchantRng = new Random();
            merchantInventoryFactory = new MerchantInventoryFactory(merchantRng);

            SeedStartingInventory();

            SaveTurnState();
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

            SaveTurnState();

            PlayerPosition = (targetRow, targetCol);
            turnCount++;
            TrySpawnMerchantsByTurn();
            MapChanged?.Invoke();

            var tile = worldMap.GetTile(targetRow, targetCol);
            HandleTileEnter(tile, targetRow, targetCol);
            
            return true;
        }

        public Enemy SpawnEnemy()
        {
            SaveTurnState();
            StartBattleAt(PlayerPosition.Row, PlayerPosition.Col, clearTile: false);
            return CurrentEnemy;
        }

        public void ResolveEnemyDefeat(Enemy enemy)
        {
            if (enemy == null)
                return;

            if (enemy.GoldReward > 0)
                player.ReciveGold(enemy.GoldReward);

            if (enemy.LootTable.Count > 0)
            {
                int drops = lootRng.Next(enemy.LootTable.Count + 1);

                for (int i = 0; i < drops; i++)
                {
                    var template = enemy.LootTable[lootRng.Next(enemy.LootTable.Count)];
                    var loot = template.Clone();
                    player.AddItem(loot);
                }
            }

            if (lootRng.NextDouble() < 0.25)
                rewindTokens++;

            player.GetExp(enemy.ExperienceReward);
            CurrentEnemy = null;

            NotifyEnemyDefeated(enemy);
            MapChanged?.Invoke();
            EnemyRewardsProcessed?.Invoke(enemy);
        }

        public bool UndoTurn()
        {
            if (rewindTokens <= 0)
            {
                SendLog("Brak tokenów cofania tury!");
                return false;
            }

            if (history.Count <= 1)
            {
                SendLog("Brak stanu do cofnięcia.");
                return false;
            }

            history.Pop();
            var previous = history.Peek();

            LoadSnapshot(previous);

            CurrentEnemy = null;

            rewindTokens--;
            SendLog("Cofnięto turę.");
            MapChanged?.Invoke();
            return true;
        }

        private void SaveTurnState()
        {
            var snapshot = CreateSnapshot();
            history.Push(snapshot);
        }

        public GameState CreateSnapshot()
        {
            return new GameState
            {
                Player = player.ToData(),
                PlayerRow = PlayerPosition.Row,
                PlayerCol = PlayerPosition.Col,
                Map = worldMap.ToData(),
                MapSize = worldMap.Size,
                RewindTokens = rewindTokens
                ,
                TurnCount = turnCount
            };
        }


        public void LoadSnapshot(GameState state)
        {
            player.LoadFromData(state.Player);
            PlayerPosition = (state.PlayerRow, state.PlayerCol);

            worldMap.LoadFromData(state.Map);

            rewindTokens = state.RewindTokens;
            turnCount = state.TurnCount;
            CurrentEnemy = null;
        }

        private void SeedStartingInventory()
        {
            for (int i = 0; i < 3; i++)
            {
                player.AddItem(new HPotion("Mała mikstura", 24));
                player.AddItem(new HPotion("Duża mikstura", 56));
            }

            player.Equip(new EquipmentItem("Startowy miecz", EquipmentSlot.Weapon, 7, 1));
            player.Equip(new EquipmentItem("Startowy napierśnik", EquipmentSlot.Armor, 0, 10));
        }

        private void SendLog(string message)
        {
            Log?.Invoke(message);
        }

        private void HandleTileEnter(ITile tile, int row, int col)
        {
            switch (tile.Type)
            {
                case ITile.TileType.EnemySpawn:
                    StartBattleAt(row, col, clearTile: true);
                    break;

                case ITile.TileType.Treasure:
                    if (tile is Treasure treasure)
                        treasure.Entered(player);

                    worldMap.ReplaceTile(row, col, new Grass());
                    MapChanged?.Invoke();
                    break;

                default:
                    TryStartRandomEncounter(tile, row, col);
                    break;
            }
        }

        public Merchant GetMerchantAtPlayerPosition()
        {
            var tile = worldMap.GetTile(PlayerPosition.Row, PlayerPosition.Col);
            return tile as Merchant;
        }

        public bool BuyFromMerchant(int offerIndex, out string message)
        {
            message = string.Empty;
            var merchant = GetMerchantAtPlayerPosition();
            if (merchant == null)
            {
                message = "Tu nie ma kupca.";
                return false;
            }

            if (offerIndex < 0 || offerIndex >= merchant.Offers.Count)
            {
                message = "Niepoprawny indeks oferty.";
                return false;
            }

            var offer = merchant.Offers[offerIndex];
            if (!player.TrySpendGold(offer.Price))
            {
                message = "Nie masz wystarczająco złota.";
                return false;
            }

            SaveTurnState();
            player.AddItem(offer.Item.Clone());
            merchant.Offers.RemoveAt(offerIndex);
            message = $"Kupiono: {offer.Item.Name} za {offer.Price} złota.";

            if (merchant.Offers.Count == 0)
            {
                worldMap.ReplaceTile(PlayerPosition.Row, PlayerPosition.Col, new Grass());
                message += " Kupiec odszedł.";
            }

            MapChanged?.Invoke();
            return true;
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

            NotifyBattleStart(enemy);
            MapChanged?.Invoke();
        }

        private bool TryStartRandomEncounter(ITile tile, int row, int col)
        {
            if (!encounterChances.TryGetValue(tile.Type, out var chance) || chance <= 0)
                return false;

            if (encounterRng.NextDouble() < chance)
            {
                StartBattleAt(row, col, clearTile: false);
                return true;
            }

            return false;
        }

        private void TrySpawnMerchantsByTurn()
        {
            if (turnCount == 0 || turnCount % 5 != 0)
                return;

            var activeMerchants = worldMap.GetMerchantPositions();
            var freeSlots = Math.Max(0, 2 - activeMerchants.Count);
            if (freeSlots == 0)
                return;

            var spawnCapForWindow = Math.Min(2, freeSlots);
            var candidates = worldMap.GetEligibleMerchantSpawnPositions(PlayerPosition);

            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int swap = merchantRng.Next(i + 1);
                (candidates[i], candidates[swap]) = (candidates[swap], candidates[i]);
            }

            var spawnCount = Math.Min(spawnCapForWindow, candidates.Count);
            for (int i = 0; i < spawnCount; i++)
            {
                var target = candidates[i];
                var offers = merchantInventoryFactory.CreateOffers();
                worldMap.ReplaceTile(target.Row, target.Col, new Merchant(offers));
            }

            if (spawnCount > 0)
            {
                SendLog($"Na mapie pojawiło się {spawnCount} kupców.");
                MapChanged?.Invoke();
            }
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
    }
}
