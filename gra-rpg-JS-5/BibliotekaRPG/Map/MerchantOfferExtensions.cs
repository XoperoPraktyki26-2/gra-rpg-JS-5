using System.Collections.Generic;
using BibliotekaRPG;

namespace BibliotekaRPG.map
{
    public static class MerchantOfferExtensions
    {
        public static List<MerchantOfferData> ToData(this List<MerchantOffer> offers)
        {
            var result = new List<MerchantOfferData>();
            foreach (var offer in offers)
            {
                result.Add(new MerchantOfferData
                {
                    Item = offer.Item.ToData(),
                    Price = offer.Price
                });
            }

            return result;
        }

        public static List<MerchantOffer> ToOffers(this List<MerchantOfferData> data)
        {
            var result = new List<MerchantOffer>();
            if (data == null)
                return result;

            foreach (var offer in data)
            {
                if (offer?.Item == null)
                    continue;

                result.Add(new MerchantOffer(offer.Item.ToItem(), offer.Price));
            }

            return result;
        }
    }
}
