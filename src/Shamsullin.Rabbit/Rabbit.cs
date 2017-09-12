using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shamsullin.Common;

namespace Shamsullin.Rabbit
{
    public static class Rabbit
    {
        private class ThreadContext
        {
            public IConnection Connection { get; set; }
            public IModel Model { get; set; }
            public QueueingBasicConsumer Consumer { get; set; }
            public int BatchSize { get; set; }
            public string InputQueue { get; set; }
            public Thread Thread { get; set; }
        }

        private static readonly ThreadContext SendOnly = new ThreadContext();

        private static readonly ConnectionFactory ConnectionFactory;

        private static readonly Dictionary<int, ThreadContext> WorkerThreads = new Dictionary<int, ThreadContext>();

        private static readonly int NumberOfWorkerThreads = 1;

        private static readonly int MaxRetries = 1;

        private static readonly int PrefetchCount = 8;

        private static readonly int SleepOnError = 1000;

        private static ThreadContext CurrentContext
        {
            get
            {
                ThreadContext context;
                if (WorkerThreads.TryGetValue(Thread.CurrentThread.ManagedThreadId, out context))
                {
                    return context;
                }
                return SendOnly;
            }
        }

        private static IConnection Connection
        {
            get { return CurrentContext.Connection; }
            set { CurrentContext.Connection = value; }
        }

        private static IModel Model
        {
            get { return CurrentContext.Model; }
            set { CurrentContext.Model = value; }
        }

        private static QueueingBasicConsumer Consumer
        {
            get { return CurrentContext.Consumer; }
            set { CurrentContext.Consumer = value; }
        }

        private static int BatchSize => CurrentContext.BatchSize;

        private static string InputQueue => CurrentContext.InputQueue;

        public static void Publish<T>(T message)
        {
            Publish(message, string.Empty, ConfigurationManager.AppSettings["RabbitExchange"],
                new[] {new KeyValuePair<string, object>("message", message.GetType().FullName)});
        }

        public static void Publish<T>(T message, string routingKey, string exchange,
            KeyValuePair<string, object>[] headers)
        {
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, message);
                stream.Seek(0, SeekOrigin.Begin);
                bytes = stream.ToArray();
            }

            ConnectIfNot();
            var properties = Model.CreateBasicProperties();
            properties.SetPersistent(true);
            properties.Headers = new Dictionary<string, object>();
            foreach (var header in headers) properties.Headers.Add(header);
            lock (Model)
                Model.BasicPublish(exchange, routingKey, properties, bytes);
        }

        private static bool IsConnected => Model != null && Model.IsOpen && Connection != null && Connection.IsOpen;

        private static readonly object SyncObj = new object();

        private static bool _isTerminating;

        private static void Cleanup()
        {
            if (Model != null)
            {
                try
                {
                    Model.Dispose();
                }
                catch
                {
                }
                finally
                {
                    Model = null;
                }
            }

            if (Connection != null)
            {
                try
                {
                    Connection.Dispose();
                }
                catch
                {
                }
                finally
                {
                    Connection = null;
                }
            }
        }

        private static void ConnectIfNot()
        {
            if (!IsConnected)
            {
                lock (SyncObj)
                {
                    if (!IsConnected)
                    {
                        Cleanup();

                        Connection = ConnectionFactory.CreateConnection();

                        Model = Connection.CreateModel();

                        if (!string.IsNullOrEmpty(InputQueue))
                        {
                            int prefetchCount = ushort.MaxValue;
                            if (PrefetchCount*BatchSize < ushort.MaxValue)
                            {
                                prefetchCount = PrefetchCount*BatchSize;
                            }
                            Model.BasicQos(0, (ushort) prefetchCount, false);
                            //model.BasicQos(0, (ushort)1, false);

                            Consumer = new QueueingBasicConsumer(Model);
                            Model.BasicConsume(InputQueue, false, "", new Dictionary<String, Object>(), Consumer);
                        }
                    }
                }
            }
        }

        private static TM ReadMessage<TM>(byte[] bytes)
        {
            try
            {
                using (var stream = new MemoryStream(bytes))
                {
                    var result = (TM) new BinaryFormatter().Deserialize(stream);
                    return result;
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Can't deserialize object", ex);
            }


            return default(TM);
        }

        private static void Work(Action strategy)
        {
            while (!_isTerminating)
            {
                try
                {
                    ConnectIfNot();
                    strategy();
                }
                catch (ThreadInterruptedException ex)
                {
                    Log.Instance.Warn("Thread interrupted", ex);
                }
                catch (ThreadAbortException ex)
                {
                    Log.Instance.Warn("Thread aborted", ex);
                }
                catch (Exception ex)
                {
                    Log.Instance.ErrorFormat("Unable to process message. Sleeping for {0}ms {1}", SleepOnError, ex);
                    Thread.Sleep(SleepOnError);
                }
            }
        }

        static Rabbit()
        {
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["RabbitMaxRetries"]))
                MaxRetries = int.Parse(ConfigurationManager.AppSettings["RabbitMaxRetries"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["RabbitPrefetchCount"]))
                PrefetchCount = int.Parse(ConfigurationManager.AppSettings["RabbitPrefetchCount"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["RabbitNumberOfWorkerThreads"]))
                NumberOfWorkerThreads = int.Parse(ConfigurationManager.AppSettings["RabbitNumberOfWorkerThreads"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["RabbitSleepOnError"]))
                SleepOnError = int.Parse(ConfigurationManager.AppSettings["RabbitSleepOnError"]);

            const string rabbitUriSettingName = "RabbitUri";
            var rabbitUri = ConfigurationManager.AppSettings[rabbitUriSettingName];
            if (string.IsNullOrEmpty(rabbitUri))
            {
                throw new ConfigurationErrorsException(
                    "Rabbit configuration '" + rabbitUriSettingName + "' is empty or missing");
            }

            ConnectionFactory = new ConnectionFactory();
            ConnectionFactory.Uri = rabbitUri;
        }

        public static uint GetQueueSize(string queueName)
        {
            ConnectIfNot();

            lock (Model)
            {
                return Model.QueueDeclarePassive(queueName).MessageCount;
            }
        }

        /// <summary>
        /// Single thread. One consumer.
        /// One dequeue, process, ack.
        /// </summary>
        public static void Install<TM>(Action<TM> handler) where TM : class
        {
            Install(ConfigurationManager.AppSettings["RabbitQueue"], handler);
        }

        /// <summary>
        /// Single thread. One consumer.
        /// One dequeue, process, ack.
        /// </summary>
        /// <typeparam name="TM">MessageType</typeparam>
        /// <param name="inputQueue">Subscribtion queue</param>
        /// <param name="handler">Process action</param>
        public static void Install<TM>(string inputQueue, Action<TM> handler) where TM : class
        {
            for (var i = 0; i < NumberOfWorkerThreads; i++)
            {
                var thread = new Thread(() => Work(() => SimpleStrategy(handler))) {IsBackground = true};
                WorkerThreads.Add(thread.ManagedThreadId,
                    new ThreadContext {BatchSize = 1, InputQueue = inputQueue, Thread = thread});
                thread.Start();
            }
        }

        public static void InstallParallel<TM>(string inputQueue, Action<TM> handler, int workersCount) where TM : class
        {
            for (; workersCount > 0; workersCount--)
            {
                var thread = new Thread(() => Work(() => SimpleStrategy(handler))) {IsBackground = true};
                WorkerThreads.Add(thread.ManagedThreadId,
                    new ThreadContext {BatchSize = 1, InputQueue = inputQueue, Thread = thread});
                thread.Start();
            }
        }

        public static void InstallBatch<TM>(string inputQueue, Action<IEnumerable<TM>> handler, int batchSize = 1000)
            where TM : class
        {
            var thread = new Thread(() => Work(() => BatchStrategy(handler))) {IsBackground = true};
            WorkerThreads.Add(thread.ManagedThreadId,
                new ThreadContext {BatchSize = batchSize, InputQueue = inputQueue, Thread = thread});
            thread.Start();
        }

        public static void InstallBatchWithPartialComplete<TM>(string inputQueue,
            Func<IEnumerable<Tuple<TM, ulong>>, IEnumerable<ulong>> handler, int batchSize = 1000) where TM : class
        {
            var thread = new Thread(() => Work(() => BatchWithPartialCompleteStrategy(handler))) {IsBackground = true};
            WorkerThreads.Add(thread.ManagedThreadId,
                new ThreadContext {BatchSize = batchSize, InputQueue = inputQueue, Thread = thread});
            thread.Start();
        }

        #region Strategies

        private static void SimpleStrategy<TM>(Action<TM> handler) where TM : class
        {
            var arg = Consumer.Queue.Dequeue();

            for (var i = 0; i < MaxRetries && !_isTerminating; i++)
            {
                var message = ReadMessage<TM>(arg.Body);
                if (message != null)
                {
                    try
                    {
                        if (i > 0) Log.Instance.InfoFormat("Trying №{0} to process message {1}", i + 1, message);
                        handler(message);
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (i == MaxRetries - 1)
                        {
                            Log.Instance.ErrorFormat("Unable to process message. Skipping. {0}", ex);
                        }
                        else
                        {
                            Log.Instance.WarnFormat("Unable to process message. Will retry in {0}ms. {1}", SleepOnError, ex);
                            Thread.Sleep(SleepOnError);
                        }
                    }
                }
            }

            if (!_isTerminating) Model.BasicAck(arg.DeliveryTag, false);
        }

        private static void BatchStrategy<TM>(Action<IEnumerable<TM>> handler) where TM : class
        {
            var batch = new List<BasicDeliverEventArgs>(BatchSize);
            while (batch.Count < BatchSize)
            {
                BasicDeliverEventArgs temp;
                if (batch.Count > 0)
                {
                    if (false == Consumer.Queue.Dequeue(10000, out temp))
                        break;
                }
                else
                {
                    temp = Consumer.Queue.Dequeue();
                }
                batch.Add(temp);
            }

            var batches = batch
                .Select(m => new Tuple<ulong, TM>(m.DeliveryTag, ReadMessage<TM>(m.Body)))
                .Where(m => m.Item2 != null)
                .ToList();

            handler(batches.Select(b => b.Item2));

            batches.ForEach(b => Model.BasicAck(b.Item1, false));
        }

        private static void BatchWithPartialCompleteStrategy<TM>(
            Func<IEnumerable<Tuple<TM, ulong>>, IEnumerable<ulong>> handler) where TM : class
        {
            var batch = new List<BasicDeliverEventArgs>(BatchSize);
            while (batch.Count < BatchSize)
            {
                BasicDeliverEventArgs temp;
                if (batch.Count > 0)
                {
                    if (false == Consumer.Queue.Dequeue(10000, out temp))
                        break;
                }
                else
                {
                    temp = Consumer.Queue.Dequeue();
                }
                batch.Add(temp);
            }

            foreach (var delivaryTag in handler(batch
                .Select(m => new Tuple<TM, ulong>(ReadMessage<TM>(m.Body), m.DeliveryTag))
                .Where(m => m.Item1 != null)))
            {
                Model.BasicAck(delivaryTag, false);
            }
        }

        #endregion

        public static void Terminate()
        {
            _isTerminating = true;

            WorkerThreads.ToList().ForEach(c =>
            {
                c.Value.Thread.Interrupt();
                c.Value.Thread.Abort();
            });
        }
    }
}