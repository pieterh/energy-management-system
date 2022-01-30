using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using P1SmartMeter.Telegram.DSMR;

namespace P1SmartMeter.Telegram
{
    public class TelegramBase
    {
        private static readonly Regex KeyValueParser = new(@"^(?<key>\d-\d:\d{1,2}\.\d{1,2}\.\d{1,2})(?:\((?<value>[^)]*)\))+$");
        private static readonly CRC16 crc16 = new();

        public IList<TelegramField> Fields { get; } = new List<TelegramField>();
        public string Header { get; private set; }
        public string Crc16 { get; private set; }
        public string Crc16Recalculated { get; private set; }

        public TelegramBase(ITelegramDefinition definition, string raw, bool validateCRC = false)
        {
            var fieldDefinitions = definition.GetFieldDefinitions();

            if (string.IsNullOrEmpty(raw))
            {
                return;
            }

            if (validateCRC){
                byte[] bytes = Encoding.ASCII.GetBytes(raw);
                Crc16Recalculated = crc16.ComputeChecksumAsString(bytes, bytes.Length);
            }

            var lines = raw.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.RemoveEmptyEntries
            );

            foreach (var line in lines)
            {
                var m = KeyValueParser.Match(line);
                
                if (!m.Success)
                {
                    switch (line[0])
                    {
                        case '/':
                            Header = line;
                            break;
                        case '!':
                            Crc16 = line[1..];
                            break;
                        default:
                            var field = new TelegramField(TelegramFieldDefinition.Invalid);
                            field.AddValue(line);
                            Fields.Add(field);
                            break;
                    }
                }
                else
                {
                    var key = m.Groups["key"].Value;
                    var def = fieldDefinitions.SingleOrDefault(x => x.Code == key) ?? TelegramFieldDefinition.Unknown(key);

                    var field = new TelegramField(def);

                    foreach (Capture valueMatch in m.Groups["value"].Captures)
                    {
                        field.AddValue(valueMatch.Value);
                    }

                    Fields.Add(field);
                }
            }
        }

        protected IList<object> GetValues(string fieldCode)
        {
            var field = Fields.FirstOrDefault(x => x.IsField(fieldCode));

            return field == null ? Array.Empty<object>() : field.Values;
        }

        protected T GetValue<T>(string fieldCode)
        {
            var values = GetValues(fieldCode);
            return (T)values.FirstOrDefault();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var field in Fields)
            {
                sb.Append(field);
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
