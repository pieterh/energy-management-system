using System;
using EMS.Library;
using static P1SmartMeter.Connection.IP1Interface;

namespace P1SmartMeter.Connection
{
    public abstract class Reader : BackgroundWorker, IP1Interface
    {
        public event EventHandler<DataArrivedEventArgs> DataArrived;

        protected void OnDataArrived(DataArrivedEventArgs e)
        {
            EventHandler<DataArrivedEventArgs> handler = DataArrived;
            handler?.Invoke(this, e);
        }
    }
}
