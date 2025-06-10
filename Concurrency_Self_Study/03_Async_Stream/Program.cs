
namespace _03_Async_Stream
{
    internal class Program
    {
        public static async IAsyncEnumerable<string> GetValueAsync(HttpClient client)
        {
            int offset = 0;
            const int limit = 10;
            while (true)
            {
                // 전체 결과 중 현재 페이지를 가져와서 파싱
                string result = await client.GetStringAsync(
                    $"https://example.com/api/values?offset={offset}&limit={limit}");
                string[] valuesOnThisPage = result.Split('\n');

                // 현재 페이지의 결과 전달
                foreach (string value in valuesOnThisPage )
                {
                    yield return value;
                }

                // 마지막 페이지면 끝냄
                if (valuesOnThisPage.Length != limit)
                {
                    break;
                }
                // 아니면 다음 페이지로 넘어감
                offset += limit;
            }
        }
        public static async Task ProcessValueAsync(HttpClient client)
        {
            await foreach (string value in GetValueAsync(client).ConfigureAwait(false))
            {
                await Task.Delay(100).ConfigureAwait(false);  // 비동기 작업
                Console.WriteLine(value);
            }
        }
        

        public static async IAsyncEnumerable<string> SlowRange()
        {
            throw new NotImplementedException();
        }
    }
}
