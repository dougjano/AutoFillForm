using System;
using System.Runtime.Serialization;

namespace AutoFillForm.Domain
{
    [Serializable]
    [DataContract]
    public class ChromeSessions
    {
        [DataMember]
        public string devtoolsFrontendUrl { get; set; }

        [DataMember]
        public string url { get; set; }

        [DataMember]
        public string webSocketDebuggerUrl { get; set; }
    }
}
