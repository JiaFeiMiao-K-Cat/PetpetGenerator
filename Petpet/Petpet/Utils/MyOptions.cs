namespace Petpet.Utils
{
    public class MyOptions
    {
        public string BasePath { get; set; } = string.Empty;
        public int MaxSize { get; set; } = 32;
        public Dictionary<string, string> Petpets { get; set; } = new Dictionary<string, string>();
    }
}
