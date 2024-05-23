namespace SQLiteData
{
    public class Entry
    {
        public DateTime Time { get; set; } = DateTime.MinValue;
        public double Value { get; set; } = 0;
        public double Threshold { get; set; } = 0;
    }
}
