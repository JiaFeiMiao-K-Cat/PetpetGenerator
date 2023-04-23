namespace Petpet.Utils
{
    public class QQHelper
    {
        public static async Task<byte[]> GetAvatar(string qqid)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36 Edg/112.0.1722.48");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            HttpResponseMessage response = await client.GetAsync($"http://q1.qlogo.cn/g?b=qq&nk={qqid}&s=640");
            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
