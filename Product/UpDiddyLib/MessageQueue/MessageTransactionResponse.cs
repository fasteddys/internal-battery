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



        public string ToString()
        {

            string RVal = "InformationMessage" + this.InformationalMessage + "|";
            RVal += "State" + this.State.ToString()  + "|";
            RVal += "Data" + this.Data + "|";
            RVal += "ResponseJson" + this.ResponseJson + "|";

            return RVal;
        }

    }
}
