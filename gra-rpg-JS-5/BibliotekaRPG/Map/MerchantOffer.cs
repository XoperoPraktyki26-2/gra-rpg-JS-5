public class MerchantOffer
{
    public IItem Item { get; }
    public int Price { get; }

    public MerchantOffer(IItem item, int price)
    {
        Item = item;
        Price = price;
    }

    public MerchantOffer Clone()
    {
        return new MerchantOffer(Item.Clone(), Price);
    }
}
