using EMS.Library;
using static P1SmartMeter.Connection.IP1Reader;

namespace P1SmartMeter.Connection
{
    internal abstract class P1Reader : BackgroundWorker, IP1Reader
    {
        public event EventHandler<DataArrivedEventArgs> DataArrived = delegate { };

        protected P1Reader(IWatchdog watchdog) : base(watchdog)
        {
        }

        protected void OnDataArrived(DataArrivedEventArgs e)
        {
            EventHandler<DataArrivedEventArgs> handler = DataArrived;
            handler.Invoke(this, e);
        }
    }
}
