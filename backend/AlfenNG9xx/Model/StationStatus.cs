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
        public float Temperature { get; set; }
        public OccpState OCCPState { get; set; }
        public uint NrOfSockets { get; set; }

        public override string ToString()
        {
            return $"{ActiveMaxCurrent}; {Temperature}; {OCCPState}; {NrOfSockets}";
        }
    }
}