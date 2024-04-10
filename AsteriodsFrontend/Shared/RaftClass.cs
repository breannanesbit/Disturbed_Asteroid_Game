namespace Shared
{
    public class KeyValue
    {
        public string key { get; set; }
        public string value { get; set; }
    }

    public class SwapInfo
    {
        public string Key { get; set; }
        public int ExpectedIndex { get; set; }
        public string NewValue { get; set; }
    }
}
