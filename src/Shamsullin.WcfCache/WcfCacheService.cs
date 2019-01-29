using System;
using System.Collections;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Web.Services;
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

        [WebInvoke(Method = "POST")]
        public byte[] Get(string key)
        {
            var result = Hashtable[key] as Record;
            if (result?.Expiry != null && result.Timestamp+result.Expiry > DateTime.Now)
            {
                Hashtable[key] = null;
                return null;
            }

            return result?.Value;
        }

        [WebInvoke(Method = "POST")]
        public void Set(Record record)
        {
            Hashtable[record.Key] = record;
        }
    }
}