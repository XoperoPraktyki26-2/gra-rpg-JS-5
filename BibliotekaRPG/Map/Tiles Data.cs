using System.Collections.Generic;

public class TileData
{
    public string Type { get; set; }
    public bool IsWalkable { get; set; }
    public RewardData Reward { get; set; }
    public List<MerchantOfferData> MerchantOffers { get; set; }
}
