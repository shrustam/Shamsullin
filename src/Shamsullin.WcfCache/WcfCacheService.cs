using System;
using System.Collections;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading;
using System.Web.Services;
using Shamsullin.Common;
using Shamsullin.Wcf;

namespace Shamsullin.WcfCache
{
    [ServiceContract]
    [WcfRestErrorHandler]
    public class WcfCacheService : WebService
    {
        private static readonly Hashtable Hashtable = new Hashtable {{"ping", new Record("pong")} };

        private static int _readers;

        private static int _writers;

        public class Record
        {
            public readonly DateTime Timestamp = DateTime.Now;

            public string Key;

            public string Value;

            public TimeSpan? Expiry;

            public Record(string value)
            {
                Value = value;
            }
        }

        [WebInvoke(Method = "GET")]
        public Hashtable Stats(string key)
        {
            var result = new Hashtable {["curr_items"] = Hashtable.Count};
            return result;
        }

        [WebInvoke(Method = "GET")]
        public string Get(string key)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                var readers = Interlocked.Increment(ref _readers);
                var record = Hashtable[key] as Record;
                if (record?.Expiry != null && record.Timestamp + record.Expiry < DateTime.Now)
                {
                    Hashtable[key] = null;
                    Log.Instance.Debug($"Expired {key} in {sw.ElapsedMilliseconds}ms, readers: {readers}");
                    return null;
                }

                Log.Instance.Debug($"Got {key} in {sw.ElapsedMilliseconds}ms, readers: {readers}");
                var result = record?.Value;
                return result;
            }
            finally
            {
                Interlocked.Decrement(ref _readers);
            }
        }

        [WebInvoke(Method = "POST")]
        public void Set(Record record)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                Interlocked.Increment(ref _writers);
                Hashtable[record.Key] = record;
                Log.Instance.Debug($"Set {record.Key} in {sw.ElapsedMilliseconds}ms");
            }
            finally
            {
                Interlocked.Decrement(ref _writers);
            }
        }
    }
}