using System.Runtime.Serialization;

namespace Shamsullin.Common.Wcf
{
    /// <summary>
    /// Web services error model. Returns is exception happens.
    /// </summary>
    [DataContract]
    public class ErrorModel
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string Type { get; set; }
    }
}