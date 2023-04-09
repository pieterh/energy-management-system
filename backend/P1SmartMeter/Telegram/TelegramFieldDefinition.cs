using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace P1SmartMeter.Telegram
{
    public partial class TelegramFieldDefinition
    {
        [GeneratedRegex(@"^([\d\.]*)\*?(.*)$", RegexOptions.None, 100)]
        private static partial Regex NumericWithUnitParser();

        public string Code { get; init; }
        public string Name { get; init; }
        public ReadOnlyCollection<TelegramFieldType> Types { get; init; }

        public TelegramFieldDefinition(string code, string name, IList<TelegramFieldType> types)
        {
            Code = code;
            Name = name;
            Types = types.AsReadOnly();
        }

        public static TelegramFieldDefinition Invalid => new("invalid", string.Empty, new[] { TelegramFieldType.Plain });
        public static TelegramFieldDefinition Unknown(string code) => new (code, string.Empty, new[] { TelegramFieldType.Plain});     
        
        public string GetLabel()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return $"{Name} ({Code})";
            }

            return Code;
        }

        public object ParseValue(int index, string rawValue)
        {
            ArgumentNullException.ThrowIfNull(rawValue);

            var type = TelegramFieldType.Plain;
            if (index < Types.Count)
            {
                type = Types[index];
            }

            switch (type)
            {
                case TelegramFieldType.Numeric:
                    return int.Parse(rawValue, CultureInfo.InvariantCulture);

                case TelegramFieldType.NumericWithUnit:
                case TelegramFieldType.NumericWithUnitAsDouble:
                    // found some examples where the unit of measurement is
                    // 1) behind the value seperated with a star '*'
                    // 2) behind the value without seperator
                    // 3) without the unit of measurement in the case that the meter didn't had a reading (all zero's)
                    // 4) the reading was all zero's and there was no decimal point
                    // We make it a bit more robust by only taking the digits and dots

                    var match = NumericWithUnitParser().Match(rawValue);
                    var numericValue = match.Groups[1].Value;
                    var unit = match.Groups[2].Value;
                    if (numericValue.Contains('.', StringComparison.Ordinal))
                    {
                        var number = double.Parse(numericValue, CultureInfo.InvariantCulture);
                        return (number, unit);
                    }
                    else
                    {
                        var number = int.Parse(numericValue, CultureInfo.InvariantCulture);
                        object o;
                        if (type == TelegramFieldType.NumericWithUnitAsDouble)
                        {
                            o = new ValueTuple<double, string>((double)number, unit);
                        }
                        else
                        {
                            o = new ValueTuple<int, string>(number, unit);
                        }
                        return o;
                    }

                case TelegramFieldType.OctetString:
                    return GetStringFromHex(rawValue);

                case TelegramFieldType.String:
                    return rawValue;

                case TelegramFieldType.Timestamp:
                    var ts = "20" + rawValue.Substring(0, 2) + "-" + rawValue.Substring(2, 2) + "-"
                             + rawValue.Substring(4, 2) + "T" + rawValue.Substring(6, 2) + ":"
                             + rawValue.Substring(8, 2) + ":" + rawValue.Substring(10, 2);
                    return DateTime.Parse(ts);

                default: // plain
                    return rawValue;
            }
        }

        private static string GetStringFromHex(string hex)
        {
            var data = new byte[hex.Length / 2];
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return Encoding.ASCII.GetString(data);
        }
    }
}
