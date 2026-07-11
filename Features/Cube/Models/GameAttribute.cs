namespace WinFormsApp1
{
    public class GameAttribute
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string StringValue { get; set; }
        public string OriginalString { get; set; }
        public bool IsPercentage { get; set; }
        public bool IsNumeric { get; set; } = true;
        public bool IsInteger { get; set; }

        public override string ToString()
        {
            if (IsNumeric)
            {
                return $"{Name}: {Value}" + (IsPercentage ? "%" : "");
            }
            else
            {
                return $"{Name}: {StringValue}";
            }
        }
    }
}
