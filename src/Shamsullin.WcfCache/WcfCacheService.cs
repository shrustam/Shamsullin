using System;
using System.Collections;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Web.Services;
using Shamsullin.Common;
using Shamsullin.Wcf;

namespace Shamsullin.WcfCache
{
    [ServiceContract]
    [WcfRestErrorHandler]
    public class WcfCacheService : WebService
    {
        private static readonly Hashtable Hashtable = new Hashtable();

        public class Record
        {
            public readonly DateTime Timestamp = DateTime.Now;

            public string Key;

            public byte[] Value;

            public TimeSpan? Expiry;
        }

        [WebInvoke(Method = "GET")]
        public Hashtable Stats(string key)
        {
            var result = new Hashtable();
            result["curr_items"] = Hashtable.Count;
            return result;
        }

        [WebInvoke(Method = "POST")]
        public byte[] Get(string key)
        {
            var sw = Stopwatch.StartNew();
            var result = Hashtable[key] as Record;
            if (result?.Expiry != null && result.Timestamp+result.Expiry < DateTime.Now)
            {
                Hashtable[key] = null;
                Log.Instance.Debug($"Expired {key} in {sw.ElapsedMilliseconds}ms");
                return null;
            }

            Log.Instance.Debug($"Got {key} in {sw.ElapsedMilliseconds}ms");
            return result?.Value;
        }

        [WebInvoke(Method = "POST")]
        public void Set(Record record)
        {
            var sw = Stopwatch.StartNew();
            Hashtable[record.Key] = record;
            Log.Instance.Debug($"Set {record.Key} in {sw.ElapsedMilliseconds}ms");
        }
    }
}