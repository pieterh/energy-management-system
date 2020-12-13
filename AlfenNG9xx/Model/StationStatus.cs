namespace AlfenNG9xx.Model
{
    public enum OccpState
    {
        Disconnected = 0,
        Connected = 1
    }
    public class StationStatus
    {

        public float ActiveMaxCurrent { get; set; }
        public float Temparature { get; set; }
        public OccpState OCCPState { get; set; }
        public uint NrOfSockets { get; set; }
    }
}