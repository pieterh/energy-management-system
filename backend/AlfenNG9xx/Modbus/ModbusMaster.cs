using System;
using SharpModbus;

namespace AlfenNG9xx.Modbus
{
    /// <summary>
    /// Proxy pattern for SharpModbus implementation
    /// </summary>
	public class ModbusMaster : IModbusMaster
    {

        private bool disposedValue;

        protected SharpModbus.ModbusMaster? Master { get; private set; }

        public ModbusMaster(SharpModbus.ModbusMaster master)
        {
            ArgumentNullException.ThrowIfNull(master);
            Master = master;
        }

        public static IModbusMaster TCP(string ip, int port, int timeout = 400)
        {
            var mm = SharpModbus.ModbusMaster.TCP(ip, port, timeout);
            return new ModbusMaster(mm);
        }

        public ushort[] ReadHoldingRegisters(byte slave, ushort address, ushort count)
        {
            if (disposedValue || Master == null) throw new ObjectDisposedException("ModbusMaster", "Object is already disposed");
            return Master.ReadHoldingRegisters(slave, address, count);
        }

        public void WriteRegister(byte slave, ushort address, ushort value)
        {
            if (disposedValue || Master == null) throw new ObjectDisposedException("ModbusMaster", "Object is already disposed");
            Master.WriteRegister(slave, address, value);
        }

        public void WriteRegisters(byte slave, ushort address, params ushort[] values)
        {
            if (disposedValue || Master == null) throw new ObjectDisposedException("ModbusMaster", "Object is already disposed");
            Master.WriteRegisters(slave, address, values);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Master?.Dispose();
                    Master = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

