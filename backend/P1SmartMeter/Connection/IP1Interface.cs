using System;
namespace P1SmartMeter.Connection
{
    public interface IP1Interface: IDisposable                              //NOSONAR
    {
        void Start();
        void Stop();
        event EventHandler<DataArrivedEventArgs> DataArrived;

        public class DataArrivedEventArgs : EventArgs
        {
            public string Data { get; set; }
        }
    }
}
