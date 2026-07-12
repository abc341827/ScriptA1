namespace WinFormsApp1
{
    public class MarketPurchaseTarget
    {
        public string Name { get; set; } = string.Empty;

        public int MaxPrice { get; set; }

        public override string ToString()
        {
            return $"{Name}|{MaxPrice}";
        }
    }
}
