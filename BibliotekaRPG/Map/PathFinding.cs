using System;
using System.Collections.Generic;
using System.Linq;

namespace BibliotekaRPG.map;

public class PathFinding
{
    private readonly WorldMap _map;

    public PathFinding(WorldMap map)
    {
        _map = map;
    }

    public List<(int Row, int Col)>? FindPath((int Row, int Col) start, (int Row, int Col) end)
    {
        
        
        if (!_map.IsWalkable(end.Row, end.Col))
            return null;

        var openSet = new PriorityQueue<(int Row, int Col), int>();
        openSet.Enqueue(start, 0);

        var cameFrom = new Dictionary<(int Row, int Col), (int Row, int Col)>();
        var gScore = new Dictionary<(int Row, int Col), int>();
        gScore[start] = 0;

        var fScore = new Dictionary<(int Row, int Col), int>();
        fScore[start] = Heuristic(start, end);

        var openSetHash = new HashSet<(int Row, int Col)> { start };

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();
            openSetHash.Remove(current);

            if (current == end)
                return ReconstructPath(cameFrom, current);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (!_map.IsWalkable(neighbor.Row, neighbor.Col))
                    continue;

                int stepCost = 1;
                var tile = _map.GetTile(neighbor.Row, neighbor.Col);
                
                
                if (tile.Type == ITile.TileType.EnemySpawn && neighbor != end)
                {
                    stepCost = 20; 
                }

                int tentativeGScore = gScore[current] + stepCost;

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + Heuristic(neighbor, end);

                    if (!openSetHash.Contains(neighbor))
                    {
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                        openSetHash.Add(neighbor);
                    }
                }
            }
        }

        return null;
    }

    private int Heuristic((int Row, int Col) a, (int Row, int Col) b)
    {
        return Math.Abs(a.Row - b.Row) + Math.Abs(a.Col - b.Col);
    }

    private IEnumerable<(int Row, int Col)> GetNeighbors((int Row, int Col) pos)
    {
        yield return (pos.Row - 1, pos.Col);
        yield return (pos.Row + 1, pos.Col);
        yield return (pos.Row, pos.Col - 1);
        yield return (pos.Row, pos.Col + 1);
    }

    private List<(int Row, int Col)> ReconstructPath(Dictionary<(int Row, int Col), (int Row, int Col)> cameFrom, (int Row, int Col) current)
    {
        var totalPath = new List<(int Row, int Col)> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Add(current);
        }
        totalPath.Reverse();
        return totalPath.Skip(1).ToList();
    }
}
