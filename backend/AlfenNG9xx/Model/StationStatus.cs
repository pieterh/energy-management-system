namespace AlfenNG9xx.Model
{

    public record StationStatus : EMS.Library.StationStatus
    {
        public override string ToString()
        {
            return $"{ActiveMaxCurrent}; {Temperature}; {OCCPState}; {NrOfSockets}";
        }
    }
}