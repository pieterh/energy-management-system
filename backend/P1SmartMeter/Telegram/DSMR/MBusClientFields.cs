using System;

namespace P1SmartMeter.Telegram.DSMR
{
    public struct MBusClientFields : IEquatable<MBusClientFields>
    {
        public string Type { get; }
        public string Ident { get; }
        public string Measurement { get; }

        public MBusClientFields(string t, string i, string m)
        {
            Type = t;
            Ident = i;
            Measurement = m;
        }

        public override bool Equals(object obj)
        {
            if (obj is not MBusClientFields) return false;
            var other = (MBusClientFields)obj;
            return Equals(other);
        }

        public bool Equals(MBusClientFields other)
        {
            return this.Type.Equals(other.Type, StringComparison.Ordinal) &&
                   this.Ident.Equals(other.Ident, StringComparison.Ordinal) &&
                   this.Measurement.Equals(other.Measurement, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return String.Concat(Type, Ident, Measurement).GetHashCode(StringComparison.Ordinal);
        }

        public static bool operator ==(MBusClientFields left, MBusClientFields right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MBusClientFields left, MBusClientFields right)
        {
            return !(left == right);
        }
    }
}