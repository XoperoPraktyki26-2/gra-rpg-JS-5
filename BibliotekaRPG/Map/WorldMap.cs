using System;
using System.Collections.Generic;
using BibliotekaRPG.Rewards;

namespace BibliotekaRPG.map
{
    public class WorldMap
    {
        private const int MapDimension = 9;
        public ITile[,] grid = new ITile[MapDimension, MapDimension];
        private readonly Random rng = new Random();
        public RewardFactory factory = new RewardFactory();

        public (int Row, int Col) PlayerStart { get; private set; }
        public int Size => grid.GetLength(0);

        public WorldMap()
        {
            PlayerStart = (MapDimension / 2, MapDimension / 2);
            GenerateValidMap();
        }

        private void GenerateValidMap()
        {
            bool ok = false;

            while (!ok)
            {
                grid = new ITile[MapDimension, MapDimension];

                int treasureCount = 0;
                int bossCount = 0;

                for (int i = 0; i < MapDimension; i++)
                {
                    for (int j = 0; j < MapDimension; j++)
                    {
                        ITile tile = GetRandomTile();

                        if (tile.Type == ITile.TileType.Treasure)
                        {
                            if (treasureCount < 5)
                            {
                                treasureCount++;
                                grid[i, j] = tile;
                            }
                            else
                            {
                                grid[i, j] = GetRandomTerrain();
                            }
                        }
                        else if (tile.Type == ITile.TileType.EnemySpawn)
                        {
                            if (bossCount < 3 && IsEdge(i, j) && DistanceToNearestBoss(i, j) >= 2)
                            {
                                bossCount++;
                                grid[i, j] = tile;
                            }
                            else
                            {
                                grid[i, j] = GetRandomTerrain();
                            }
                        }
                        else
                        {
                            grid[i, j] = tile;
                        }
                    }
                }

                EnsureStartIsWalkable();
                ok = treasureCount == 5 && bossCount == 3 && AreSpecialTilesReachable();
            }
        }

        private void EnsureStartIsWalkable()
        {
            var startTile = grid[PlayerStart.Row, PlayerStart.Col];
            if (startTile == null || !startTile.isWalkable || startTile.Type == ITile.TileType.EnemySpawn || startTile.Type == ITile.TileType.Treasure)
                grid[PlayerStart.Row, PlayerStart.Col] = new EmptyTile();
        }

        private ITile GetRandomTile()
        {
            int roll = rng.Next(6);

            return roll switch
            {
                0 => new Grass(),
                1 => new Forest(),
                2 => new Mountain(),
                3 => new Treasure(factory.Spawn()),
                4 => new EnemySpawn(),
                5 => new EmptyTile(),
                _ => new Grass()
            };
        }

        private ITile GetRandomTerrain()
        {
            int roll = rng.Next(4);

            return roll switch
            {
                0 => new Grass(),
                1 => new Forest(),
                2 => new Mountain(),
                3 => new EmptyTile(),
                _ => new Grass()
            };
        }

        private bool IsEdge(int x, int y)
        {
            return x == 0 || y == 0 || x == MapDimension - 1 || y == MapDimension - 1;
        }

        private int DistanceToNearestBoss(int x, int y)
        {
            int min = int.MaxValue;

            for (int i = 0; i < MapDimension; i++)
            {
                for (int j = 0; j < MapDimension; j++)
                {
                    if (grid[i, j] != null && grid[i, j].Type == ITile.TileType.EnemySpawn)
                    {
                        int d = Math.Abs(x - i) + Math.Abs(y - j);
                        min = Math.Min(min, d);
                    }
                }
            }

            return min == int.MaxValue ? 99 : min;
        }

        public bool IsWithinBounds(int row, int col)
        {
            return row >= 0 && col >= 0 && row < Size && col < Size;
        }

        public bool IsWalkable(int row, int col)
        {
            return IsWithinBounds(row, col) && grid[row, col].isWalkable;
        }

        public ITile GetTile(int row, int col)
        {
            return grid[row, col];
        }

        public void ReplaceTile(int row, int col, ITile tile)
        {
            grid[row, col] = tile;
        }

        public void ShowMap(ConsoleLogger logger)
        {
            logger.ShowMap(this);
        }

        private bool AreSpecialTilesReachable()
        {
            var visited = new bool[MapDimension, MapDimension];
            var queue = new Queue<(int Row, int Col)>();

            if (!grid[PlayerStart.Row, PlayerStart.Col].isWalkable)
                return false;

            queue.Enqueue(PlayerStart);
            visited[PlayerStart.Row, PlayerStart.Col] = true;

            while (queue.Count > 0)
            {
                var (row, col) = queue.Dequeue();

                foreach (var neighbor in GetNeighbors(row, col))
                {
                    if (!IsWithinBounds(neighbor.Row, neighbor.Col))
                        continue;

                    if (visited[neighbor.Row, neighbor.Col])
                        continue;

                    var tile = grid[neighbor.Row, neighbor.Col];
                    if (!tile.isWalkable)
                        continue;

                    visited[neighbor.Row, neighbor.Col] = true;
                    queue.Enqueue(neighbor);
                }
            }

            for (int i = 0; i < MapDimension; i++)
            {
                for (int j = 0; j < MapDimension; j++)
                {
                    var tile = grid[i, j];
                    if ((tile.Type == ITile.TileType.Treasure || tile.Type == ITile.TileType.EnemySpawn) && !visited[i, j])
                        return false;
                }
            }

            return true;
        }

        private IEnumerable<(int Row, int Col)> GetNeighbors(int row, int col)
        {
            yield return (row - 1, col);
            yield return (row + 1, col);
            yield return (row, col - 1);
            yield return (row, col + 1);
        }
    }
}
