using System.Collections.Generic;

namespace P1SmartMeter.Telegram
{
    public class TelegramField
    {
        private readonly TelegramFieldDefinition _definition;


        public IList<object> Values { get; } = new List<object>();
        public string SingleValue => string.Join("; ", Values);
        public TelegramFieldDefinition Definition => _definition;

        public TelegramField(TelegramFieldDefinition definition)
        {
            _definition = definition;
        }

        public void AddValue(string value)
        {
            var parsedValue = Definition.ParseValue(Values.Count, value);
            Values.Add(parsedValue);
        }

        public override string ToString()
        {
            return $"{Definition.GetLabel()}: {SingleValue}";
        }

        public bool IsField(string fieldCode)
        {
            return Definition.Code == fieldCode;
        }
    }
}
