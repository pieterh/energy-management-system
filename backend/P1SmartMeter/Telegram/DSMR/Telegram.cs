using System;
using System.Collections.Generic;

namespace P1SmartMeter.Telegram.DSMR
{
    public class DSMRTelegram : TelegramBase                                //NOSONAR
    {
        public DSMRTelegram(string raw, bool validateCRC = false) : base(TelegramDefinition.Instance, raw, validateCRC)
        {
            Timestamp = GetValue<DateTime?>(TelegramDefinition.Timestamp);
            Electricity1FromGrid = GetValue<(double, string)?>(TelegramDefinition.Electricity1ToClient)?.Item1;
            Electricity2FromGrid = GetValue<(double, string)?>(TelegramDefinition.Electricity2ToClient)?.Item1;
            Electricity1ToGrid = GetValue<(double, string)?>(TelegramDefinition.Electricity1FromClient)?.Item1;
            Electricity2ToGrid = GetValue<(double, string)?>(TelegramDefinition.Electricity2FromClient)?.Item1;

            PowerUsedL1 = GetValue<(double, string)?>(TelegramDefinition.PowerUsedL1)?.Item1;
            PowerUsedL2 = GetValue<(double, string)?>(TelegramDefinition.PowerUsedL2)?.Item1;
            PowerUsedL3 = GetValue<(double, string)?>(TelegramDefinition.PowerUsedL3)?.Item1;

            PowerReturnedL1 = GetValue<(double, string)?>(TelegramDefinition.PowerReturnedL1)?.Item1;
            PowerReturnedL2 = GetValue<(double, string)?>(TelegramDefinition.PowerReturnedL2)?.Item1;
            PowerReturnedL3 = GetValue<(double, string)?>(TelegramDefinition.PowerReturnedL3)?.Item1;

            TariffIndicator = GetValue<int?>(TelegramDefinition.TariffIndicator);
            ActualPowerUse = GetValue<(double, string)?>(TelegramDefinition.ActualPowerUse)?.Item1;
            ActualPowerReturn = GetValue<(double, string)?>(TelegramDefinition.ActualPowerReturn)?.Item1;

            PowerFailures = GetValue<int?>(TelegramDefinition.PowerFailures);
            PowerFailuresLong = GetValue<int?>(TelegramDefinition.PowerFailuresLong);
            PowerFailureEventLog = new List<PowerFailureEvent>();
            var t = GetValues(TelegramDefinition.PowerFailureEventLog);            
            var nr = GetValue<int>(TelegramDefinition.PowerFailureEventLog);
            for(int i=2; i < 2 +(nr*2); i+=2){
                PowerFailureEventLog.Add(new PowerFailureEvent((DateTime)t[i], ((ValueTuple<int, string>)t[i+1]).Item1));
            }
            PowerSagsL1 = GetValue<int?>(TelegramDefinition.PowerSagsL1);
            PowerSagsL2 = GetValue<int?>(TelegramDefinition.PowerSagsL2);
            PowerSagsL3 = GetValue<int?>(TelegramDefinition.PowerSagsL3);
            PowerSwellsL1 = GetValue<int?>(TelegramDefinition.PowerSwellsL1);
            PowerSwellsL2 = GetValue<int?>(TelegramDefinition.PowerSwellsL2);
            PowerSwellsL3 = GetValue<int?>(TelegramDefinition.PowerSwellsL3);

            TextMessage = GetValue<string>(TelegramDefinition.TextMessage);

            VoltageL1 = GetValue<(double, string)?>(TelegramDefinition.VoltageL1)?.Item1;
            VoltageL2 = GetValue<(double, string)?>(TelegramDefinition.VoltageL2)?.Item1;
            VoltageL3 = GetValue<(double, string)?>(TelegramDefinition.VoltageL3)?.Item1;

            CurrentL1 = GetValue<(int, string)?>(TelegramDefinition.CurrentL1)?.Item1;
            CurrentL2 = GetValue<(int, string)?>(TelegramDefinition.CurrentL2)?.Item1;
            CurrentL3 = GetValue<(int, string)?>(TelegramDefinition.CurrentL3)?.Item1;

            MBusDevice1 = HandleMBusDevice(1); 
            MBusDevice2 = HandleMBusDevice(2); 
            MBusDevice3 = HandleMBusDevice(3); 
            MBusDevice4 = HandleMBusDevice(4); 
        }

        private MBusDevice HandleMBusDevice(int i){
            var fields = TelegramDefinition.GetMBusClientFields(i); 

            var mbusClient = GetValue<int?>(fields.Type);
            if (mbusClient == null)
                return null;

            var ident = GetValue<string>(fields.Ident);
            var measurementInfo = GetValues(fields.Measurement);         
            var timestamp = (DateTime)measurementInfo[0];
            var measurement = (System.ValueTuple<double,System.String>) measurementInfo[1];                        
                
            var retval = new MBusDevice((MBusDevice.DeviceTypes)mbusClient, ident, timestamp, measurement.Item1, measurement.Item2);                    
            return retval;
        }

        public DateTime? Timestamp { get; }

        public string TextMessage { get; }

        public double? ActualPowerUse { get; }
        public double? ActualPowerReturn { get; }

        public int? PowerFailures {get; }
        public int? PowerFailuresLong {get; }
        public IList<PowerFailureEvent> PowerFailureEventLog { get; }
        public int? PowerSagsL1 {get; }
        public int? PowerSagsL2 {get; }
        public int? PowerSagsL3 {get; }
        public int? PowerSwellsL1 {get; }
        public int? PowerSwellsL2 {get; }
        public int? PowerSwellsL3 {get; }

        public int? TariffIndicator { get; }

        public double? Electricity1FromGrid { get; }

        public double? Electricity2FromGrid { get; }

        public double? Electricity1ToGrid { get; }

        public double? Electricity2ToGrid { get; }

        public double? PowerUsedL1 { get; }
        public double? PowerUsedL2 { get; }
        public double? PowerUsedL3 { get; }

        public double? PowerReturnedL1 { get; }
        public double? PowerReturnedL2 { get; }
        public double? PowerReturnedL3 { get; }

        public double? VoltageL1 { get; }
        public double? VoltageL2 { get; }
        public double? VoltageL3 { get; }

        public int? CurrentL1 { get; }
        public int? CurrentL2 { get; }
        public int? CurrentL3 { get; }

        public MBusDevice MBusDevice1 {get; }
        public MBusDevice MBusDevice2 {get; }
        public MBusDevice MBusDevice3 {get; }
        public MBusDevice MBusDevice4 {get; }

        public override string ToString()
        {
            return $"Timestamp: {Timestamp}\nE1: {Electricity1FromGrid}\nE2: {Electricity1FromGrid}\nTariff: {TariffIndicator}\nActualPower: {ActualPowerUse}\nTextMessage: {TextMessage}";
        }
    }

     public record PowerFailureEvent {
        public DateTime Timestamp {get; }
        public int Duration {get;}

        public PowerFailureEvent(DateTime timeStamp, int duration) {
            Timestamp = timeStamp;
            Duration = duration;
        }
    }

    // device types are taken from "OMS-Spec_Vol2_Primary_v421", page 17, table 2
    public record MBusDevice {
        public MBusDevice(){}
        public MBusDevice(DeviceTypes type, string identifier, DateTime measurementTimestamp, double measurement, string units){
            DeviceType = type;
            Identifier = identifier;
            MeasurementTimestamp = measurementTimestamp;
            Measurement = measurement;
            UnitOfMeasurement = units;
        }

        public enum DeviceTypes {None = 0, Electricity = 2, Gas = 3, Heat = 4, WarmWater = 6,  Water = 7 }
        public DeviceTypes DeviceType { get; }

        public string Identifier { get; }
        public DateTime MeasurementTimestamp {get;}
        public double Measurement { get; }        
        public string UnitOfMeasurement { get; }   
    }

}
