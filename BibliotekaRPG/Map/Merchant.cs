using System.Collections.Generic;

namespace BibliotekaRPG.map
{
    public class Merchant : ITile
    {
        public ITile.TileType Type => ITile.TileType.Merchant;
        public bool isWalkable => true;
        public List<MerchantOffer> Offers { get; } = new();

        public Merchant(IEnumerable<MerchantOffer> offers)
        {
            if (offers == null)
                return;

            Offers.AddRange(offers);
        }
    }
}
