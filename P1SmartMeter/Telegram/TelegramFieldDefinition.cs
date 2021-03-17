using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace P1SmartMeter.Telegram
{
    public class TelegramFieldDefinition
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public IList<TelegramFieldType> Types { get; set; }

        public static TelegramFieldDefinition Invalid => new TelegramFieldDefinition
        {
            Code = "invalid",
            Name = "",
            Types = new[] { TelegramFieldType.Plain }
        };

        public static TelegramFieldDefinition Unknown(string code)
        {
            return new TelegramFieldDefinition
            {
                Code = code,
                Name = "",
                Types = new[] { TelegramFieldType.Plain }
            };
        }

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
            var type = TelegramFieldType.Plain;
            if (index < Types.Count)
            {
                type = Types[index];
            }

            try
            {
                switch (type)
                {
                    case TelegramFieldType.Numeric:
                        if (rawValue.Contains("."))
                        {
                            return double.Parse(rawValue, CultureInfo.InvariantCulture);
                        }

                        return int.Parse(rawValue, CultureInfo.InvariantCulture);

                    case TelegramFieldType.NumericWithUnit:
                        var parts = rawValue.Split("*");
                        var unit = parts[1];
                        var numericValue = parts[0];
                        if (numericValue.Contains("."))
                        {
                            var number = double.Parse(numericValue, CultureInfo.InvariantCulture);
                            return (number, unit);
                        }
                        else
                        {
                            var number = int.Parse(numericValue, CultureInfo.InvariantCulture);
                            return (number, unit);
                        }

                    case TelegramFieldType.String:
                        return GetStringFromHex(rawValue);

                    case TelegramFieldType.Timestamp:
                        var ts = "20" + rawValue.Substring(0, 2) + "-" + rawValue.Substring(2, 2) + "-"
                                 + rawValue.Substring(4, 2) + "T" + rawValue.Substring(6, 2) + ":"
                                 + rawValue.Substring(8, 2) + ":" + rawValue.Substring(10, 2);
                        return DateTime.Parse(ts);

                    default: // plain
                        return rawValue;
                }
            }
            catch
            {
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
