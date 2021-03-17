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
        private static readonly Regex KeyValueParser = new Regex(@"^(?<key>\d-\d:\d{1,2}\.\d{1,2}\.\d{1,2})(?:\((?<value>[^)]*)\))+$");
        public IList<TelegramField> Fields { get; } = new List<TelegramField>();
        public string header { get; private set; }
        public string crc16 { get; private set; }

        public TelegramBase(TelegramDefinition definition, string raw)
        {
            var fieldDefinitions = definition.GetFieldDefinitions();

            if (string.IsNullOrEmpty(raw))
            {
                return;
            }

            var lines = raw.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.RemoveEmptyEntries
            );

            foreach (var line in lines)
            {
                var m = KeyValueParser.Match(line);
                TelegramField field;
                if (!m.Success)
                {
                    if (line[0] == '/')
                    {
                        header = line;
                    }
                    else
                    if (line[0] == '!')
                    {
                        crc16 = line.Substring(1);
                    }
                    else
                    {
                        field = new TelegramField(TelegramFieldDefinition.Invalid);
                        field.AddValue(line);
                    }
                }
                else
                {
                    var key = m.Groups["key"].Value;
                    var def = fieldDefinitions.SingleOrDefault(x => x.Code == key) ?? TelegramFieldDefinition.Unknown(key);

                    field = new TelegramField(def);

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

            return field == null ? new object[0] : field.Values;
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
