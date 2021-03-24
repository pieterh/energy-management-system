using System;
using Microsoft.Extensions.Hosting;

namespace EMS.Library
{
    public interface IChargePoint : IAdapter, IHostedService
    {
        void UpdateMaxCurrent(float maxCurrent, ushort phases);

        public class StatusUpdateEventArgs : EventArgs
        {
            public bool IsCharging { get; set; }
        }

        public event EventHandler<StatusUpdateEventArgs> StatusUpdate;
    }
}
