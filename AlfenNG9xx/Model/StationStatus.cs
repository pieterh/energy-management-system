namespace AlfenNG9xx.Model
{
            public enum OCCPStateEnum {
            Disconnected = 0,
            Connected = 1
        }
    public class StationStatus
    {

        public float ActiveMaxCurrent {get; set;}
        public float Temparature {get; set;}
        public OCCPStateEnum OCCPState {get; set;}
        public uint NrOfSockets {get; set;}
    }
}