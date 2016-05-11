using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shamsullin.Common
{
    /// <summary>
    /// Класс для постановки задач в очередь на асинхронное выполнение.
    /// </summary>
    public class AsyncManager
    {
		private readonly WaitCallback _callback;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncManager"/> class.
        /// </summary>
		public AsyncManager(WaitCallback callback)
        {
            _callback = callback;
        }

		public static void SetMinThreads(int cnt)
		{
			int workerThreads, complete;
			ThreadPool.GetMinThreads(out workerThreads, out complete);
			ThreadPool.SetMinThreads(cnt, complete);
        }

        public Task ExecuteAsync(object state = null)
        {
			return Task.Factory.StartNew(() => Method(state));
        }

		protected void Method(object state)
        {
            try
            {
                _callback(state);
            }
            catch (Exception ex)
            {
				Log.Instance.Error("Async execution error.", ex);
            }
        }
    }
}
