using System;

namespace AlfenNG9xx.Model
{  
    public class SocketMeasurement
    {
        public UInt16 MeterState {get; set;}
        public UInt64 MeterTimestamp {get; set;}
        public UInt16 MeterType {get; set;}

        public bool Availability {get; set;}
        public string Mode3State {get; set;}
    }
}
