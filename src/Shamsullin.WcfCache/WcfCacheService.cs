﻿using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading;
using System.Web.Services;
using log4net;
using Shamsullin.Common.Wcf;

namespace Shamsullin.WcfCache
{
    [ServiceContract]
    [WcfRestErrorHandler]
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class WcfCacheService : WebService
    {
        private static readonly Hashtable Hashtable = new Hashtable {{"ping", new Record("pong")} };
        public ILog Log = LogManager.GetLogger(typeof(WcfCacheService));

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
        public Hashtable Stats()
        {
            // Cleanup
            foreach (var key in Hashtable.Keys.Cast<string>().ToArray())
            {
                var record = Hashtable[key] as Record;
                if (record == null || record.Expiry != null && record.Timestamp+record.Expiry < DateTime.Now)
                {
                    lock (Hashtable) Hashtable.Remove(key);
                }
            }

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
                var record = Hashtable[key] as Record; // Hashtable is thread safe for read
                if (record?.Expiry != null && record.Timestamp+record.Expiry < DateTime.Now)
                {
                    lock (Hashtable) Hashtable.Remove(key);
                    Log?.Debug($"Expired {key} in {sw.ElapsedMilliseconds}ms, readers: {readers}");
                    return null;
                }

                Log?.Debug($"Got {key} in {sw.ElapsedMilliseconds}ms, readers: {readers}");
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
                lock (Hashtable)
                {
                    var sw = Stopwatch.StartNew();
                    var writers = Interlocked.Increment(ref _writers);
                    Hashtable[record.Key] = record;
                    Log?.Debug($"Set {record.Key} in {sw.ElapsedMilliseconds}ms, writers: {writers}");
                }
            }
            finally
            {
                Interlocked.Decrement(ref _writers);
            }
        }
    }
}