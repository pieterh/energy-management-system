using System;
using Microsoft.Extensions.Hosting;

namespace EMS.Library
{
    public interface IBackgroundService: IHostedService, IDisposable
    {
    }
}
