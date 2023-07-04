using System;
using System.Net;
using System.Numerics;

namespace AlfenNG9xx.Modbus
{
	public interface IModbusMaster: IDisposable
    {        
        ushort[] ReadHoldingRegisters(byte slave, ushort address, ushort count);
        void WriteRegister(byte slave, ushort address, ushort value);
        void WriteRegisters(byte slave, ushort address, params ushort[] values);
    }
}

