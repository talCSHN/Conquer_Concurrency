using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace _02_Async_Basic
{
    internal class Program
    {
        public static bool CanBehaveSynchronously { get; private set; }

        public static async Task<T> DelayResult<T>(T result, TimeSpan delay)
        {
            await Task.Delay(delay);
            return result;
        }
        public static async Task<string> DownloadStringWithRetries(HttpClient client, string uri)
        {
            // 1초 후 재시도, 다음에는 2초 후 재시도, 그 다음에는 4초 후 재시도 ...
            TimeSpan nextDelay = TimeSpan.FromSeconds(1);
            for (int i = 0; i != 3; ++i)
            {
                try
                {
                    return await client.GetStringAsync(uri);
                }
                catch (Exception)
                {
                }
                await Task.Delay(nextDelay);
                nextDelay = nextDelay * 2;
            }
            // 오류 전파를 위해 마지막으로 한 번 더 시도
            return await client.GetStringAsync(uri);
        }
        public static async Task<string> DownloadStringWithTimeout(HttpClient client, string uri)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            Task<string> downloadTask = client.GetStringAsync(uri);
            Task timeoutTask = Task.Delay(Timeout.InfiniteTimeSpan, cts.Token);

            Task completedTask = await Task.WhenAny(downloadTask, timeoutTask);
            if (completedTask == timeoutTask)
            {
                return null;
            }
            return await downloadTask;
        }
        interface IMyAsyncInterface
        {
            Task<int> GetValueAsync();
            Task DosomethingAsync();
            Task<T> NotImplementedAsync<T>();
            Task<int> GetValueAsync(CancellationToken cancellationToken);
            Task DosomethingAsync1();

        }
        class MySynchronousImplementation : IMyAsyncInterface
        {
            public Task<int> GetValueAsync()
            {
                return Task.FromResult(13);
            }
            public Task DosomethingAsync()
            {
                return Task.CompletedTask;
            }
            public Task<T> NotImplementedAsync<T>()
            {
                return Task.FromException<T>(new NotImplementedException());
            }
            public Task<int> GetValueAsync(CancellationToken cancellationToken)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.FromCanceled<int>(cancellationToken);
                }
                return Task.FromResult(13);
            }
            public Task DosomethingAsync1()
            {
                try
                {
                    DosomethingAsync();
                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    return Task.FromException(ex);
                }
            }
            private static readonly Task<int> zeroTask = Task.FromResult(0);
            Task<int> GetValueAsync1()
            {
                return zeroTask;
            }
        }
        public static async Task MyMethodAsync(IProgress<double> progress = null)
        {
            bool done = false;
            double percentComplete = 0;
            while (!done)
            {
                progress?.Report(percentComplete);
            }
        }

        public static async Task CallMyMethodAsync()
        {
            var progress = new Progress<double>();
            progress.ProgressChanged += (sender, args) =>
            {

            };
            await MyMethodAsync(progress);
        }
        public static async Task<string> DownloadAllAsync(HttpClient client, IEnumerable<string> urls)
        {
            // 각 URL에 수행할 동작 정의
            var downloads = urls.Select(url => client.GetStringAsync(url));
            // 시퀀스 평가하지 않았으므로
            // 아직 실제로 시작한 작업은 없음

            // 동시에 모든 URL에서 다운로드 시작
            Task<string>[] downloadTasks = downloads.ToArray();
            // 이제 모든 작업 시작함

            // 모든 다운로드 완료를 비동기적으로 대기
            string[] htmlPages = await Task.WhenAll(downloadTasks);

            return string.Concat(htmlPages);
        }
        public static async Task<int> FirstRespondingUrlAsync(HttpClient client, string urlA, string urlB)
        {
            // 동시에 두 URL에서 다운로드 시작
            Task<byte[]> downloadTaskA = client.GetByteArrayAsync(urlA);
            Task<byte[]> downloadTaskB = client.GetByteArrayAsync(urlB);

            // 두 작업 중 하나의 완료 기다림
            Task<byte[]> completedTask = await Task.WhenAny(downloadTaskA, downloadTaskB);

            // 해당 URL에서 받은 데이터의 길이 반환
            byte[] data = await completedTask;
            return data.Length;
        }

        public static async Task<int> DelayAndReturnAsync(int value)
        {
            await Task.Delay(TimeSpan.FromSeconds(value));
            return value;
        }
        public static async Task AwaitAndProcessAsync(Task<int> task)
        {
            int result = await task;
            Console.WriteLine(result);
        }
        
        // 이제 1, 2, 3 출력
        public static async Task ProcessTasksAsync()
        {
            // 일련의 작업 생성
            Task<int> taskA = DelayAndReturnAsync(2);
            Task<int> taskB = DelayAndReturnAsync(3);
            Task<int> taskC = DelayAndReturnAsync(1);
            Task<int>[] tasks = new[] { taskA, taskB, taskC };

            Task[] processingTasks = tasks.Select(async t =>
            {
                var result = await t;
                Console.WriteLine(result);
            }).ToArray();

            // 모든 처리가 끝날 때까지 대기
            await Task.WhenAll(processingTasks);
        }
        public static async Task ResumeOnContextAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            // 현재 이 메서드는 같은 컨텍스트 안에서 재개
        }
        public static async Task ResumeWithoutContextAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            // 현재 이 메서드는 재개할 때 자신의 컨텍스트 무시
        }
        // 예외 발생 함수
        public static async Task ThrowExceptionAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            throw new InvalidOperationException("TEST");
        }
        // 예외 잡는 함수
        public static async Task TestAsync()
        {
            // 메서드가 예외 발생시키고 이 예외를 반환할 작업에 추가
            Task task = ThrowExceptionAsync();
            try
            {
                await task;
            }
            catch (InvalidOperationException)
            {
                // 여기서 예외 잡힘
            }
        }
        public static ValueTask<int> MethodAsync()
        {
            if (CanBehaveSynchronously)
            {
                return new ValueTask<int>(13);
            }
            return new ValueTask<int>(SlowMethodAsync());
        }

        private static async Task<int> SlowMethodAsync()
        {
            throw new NotImplementedException();
        }

        private Func<Task> _disposeLogic;
        public ValueTask DisposeAsync()
        {
            if (_disposeLogic == null)
            {
                return default;
            }
            // 주의: 지금 이 코드는 thread-unsafe임
            // 여러 스레드가 DisposeAsync 호출 시
            // 삭제 로직이 두 번 이상 실행 가능성 존재
            Func<Task> logic = _disposeLogic;
            _disposeLogic = null;
            return new ValueTask(logic());
        }
        public static async Task ConsumingMethodAsync()
        {
            Task<int> task = MethodAsync().AsTask();
            int value = await task;
            int anotherValue = await task;
        }

        static async Task Main(string[] args)
        {
            Task<int> task1 = Task.FromResult(3);
            Task<int> task2 = Task.FromResult(5);
            Task<int> task3 = Task.FromResult(7);
            List<int> results = (await Task.WhenAll(task1, task2, task3)).ToList();
            results.ForEach(n => Console.Write(n + ", "));

        }
    }
}
