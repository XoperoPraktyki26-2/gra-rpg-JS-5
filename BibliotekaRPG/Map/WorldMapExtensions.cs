using BibliotekaRPG.map;
using System;
using System.Collections.Generic;

namespace BibliotekaRPG
{
    public static class WorldMapExtensions
    {
        public static TileData[] ToData(this WorldMap map)
        {
            int size = map.Size;
            var result = new TileData[size * size];

            int index = 0;

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    var tile = map.grid[i, j];

                    var data = new TileData
                    {
                        Type = tile.Type.ToString(),
                        IsWalkable = tile.isWalkable,
                        Reward = null,
                        MerchantOffers = null
                    };

                    if (tile is Treasure)
                    {
                        data.Reward = new RewardData
                        {
                            RewardType = "Treasure"
                        };
                    }
                    else if (tile is Merchant merchant)
                    {
                        data.MerchantOffers = merchant.Offers.ToData();
                    }

                    result[index++] = data;
                }
            }

            return result;
        }

        public static void LoadFromData(this WorldMap map, TileData[] data)
        {
            int size = (int)Math.Sqrt(data.Length);
            map.grid = new ITile[size, size];

            int index = 0;

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    var td = data[index++];

                    ITile tile = td.Type switch
                    {
                        "Grass" => new Grass(),
                        "Forest" => new Forest(),
                        "Mountain" => new Mountain(),
                        "Treasure" => new Treasure(map.factory.Spawn()),
                        "EnemySpawn" => new EnemySpawn(),
                        "Empty" => new EmptyTile(),
                        "Merchant" => new Merchant(td.MerchantOffers.ToOffers()),
                        _ => new Grass()
                    };

                    map.grid[i, j] = tile;
                }
            }
        }
    }
}
