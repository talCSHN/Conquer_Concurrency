using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Reactive.Linq;
using System.Threading.Tasks.Dataflow;

namespace Concurrency_Overview
{
    
    internal class Program
    {
        public static async Task DoSomethingAsync()
        {
            int value = 13;

            // 비동기로 1초 대기
            await Task.Delay(TimeSpan.FromSeconds(1));

            value *= 2;

            // 비동기로 1초 대기
            await Task.Delay(TimeSpan.FromSeconds(1));

            Console.WriteLine(value);
        }
        public static async Task DoSomthingAsync1()
        {
            int value = 13;

            // 비동기로 1초 대기
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

            value *= 2;

            // 비동기로 1초 대기
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

            Console.WriteLine(value);
        }
        public static async Task TrySomethingAsync()
        {
            try
            {
                await PossibleExceptionAsync();
            }
            catch (NotSupportedException ex)
            {
                LogException(ex);
                throw;
            }
        }
        public static async Task TrySomethingAsync1()
        {
            // 예외가 바로 일어나지 않고 Task에서 발생
            Task task = PossibleExceptionAsync();
            try
            {
                // 여기 await에서 Task의 예외 발생
                await task;
            }
            catch (NotSupportedException ex)
            {
                LogException(ex);
                throw;
            }
        }
        public static async Task WaitAsync()
        {
            // 이 await는 현재 컨텍스트를 저장하고
            await Task.Delay(TimeSpan.FromSeconds(1));
            // 여기서 지정한 컨텍스트 안에서 메서드 재개하려고 시도
        }
        static void Deadlock()
        {
            // 지연 시작
            Task task = WaitAsync();

            // 동기적으로 차단하고 async 메서드의 완료 기다림
            task.Wait();
        }

        static void LogException(NotSupportedException ex)
        {
            throw new NotImplementedException();
        }

        static async Task PossibleExceptionAsync()
        {
            throw new NotImplementedException();
        }
        static void RotateMatrices(IEnumerable<Matrix> matrices, float degrees)
        {
            Parallel.ForEach(matrices, matrix => matrix.Rotate(degrees));
        }
        static IEnumerable<bool> PrimalityTest(IEnumerable<int> values)
        {
            return values.AsParallel().Select(value => IsPrime(value));
        }
        void ProcessArray(double[] array)
        {
            Parallel.Invoke(
            
                () => ProcessPartialArray(array, 0, array.Length / 2),
                () => ProcessPartialArray(array, array.Length / 2, array.Length)
            );
        }
        // 클로저 개념 익히기
        void ExampleFunc()
        {
            int a = 1;
            Parallel.Invoke(
                // int a 가 캡쳐되어 클로저 객체 안에 담김
                () => Console.WriteLine(a),
                () => Console.WriteLine(a + 1),
                () => Console.WriteLine(a + 2)
            );

        }
        void ExampleFunc1()
        {
            try
            {
                Parallel.Invoke(() => { throw new Exception(); },
                () => { throw new Exception(); });

            }
            catch (AggregateException ex)
            {
                ex.Handle(exception =>
                {
                    Trace.WriteLine(exception);
                    return true;
                });
            }
        }

        void ProcessPartialArray(double[] array, int begin, int end)
        {
            // CPU 집약적인 처리
        }

        private static bool IsPrime(int value)
        {
            throw new NotImplementedException();
        }

        static void Main(string[] args)
        {
            DoSomethingAsync().Wait();
            DoSomthingAsync1().Wait();

            Observable.Interval(TimeSpan.FromSeconds(1))
                .Timestamp()
                .Where(x => x.Value % 2 == 0)
                .Select(x => x.Timestamp)
                .Subscribe(x => Trace.WriteLine(x));

            IObservable<DateTimeOffset> timestamps =
                Observable.Interval(TimeSpan.FromSeconds(1))
                .Timestamp()
                .Where(x => x.Value % 2 == 0)
                .Select(x => x.Timestamp);
            timestamps.Subscribe(x => Trace.WriteLine(x), ex => Trace.WriteLine(ex));

            try
            {
                var multiplyBlock = new TransformBlock<int, int>(item =>
                {
                    if (item == 1)
                    {
                        throw new InvalidOperationException("Blech.");
                    }
                    return item * 2;
                });
                var substractBlock = new TransformBlock<int, int>(item => item - 2);
                multiplyBlock.LinkTo(substractBlock, new DataflowLinkOptions { PropagateCompletion = true });
                multiplyBlock.Post(1);
                substractBlock.Completion.Wait();
            }
            catch (AggregateException exception)
            {
                AggregateException ex = exception.Flatten();
                Trace.WriteLine(ex.InnerException);
            }
        }
    }
}
