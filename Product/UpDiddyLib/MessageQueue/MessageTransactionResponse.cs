using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.MessageQueue
{
    public enum TransactionState { Complete, InProgress, Error, FatalError };

    public class MessageTransactionResponse
    {
        public string InformationalMessage { get; set; }
        public TransactionState State { get; set; }
        public string Data { get; set; }
        public string ResponseJson { get; set; }


    }
}
