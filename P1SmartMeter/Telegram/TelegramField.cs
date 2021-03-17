using System.Collections.Generic;

namespace P1SmartMeter.Telegram
{
    public class TelegramField
    {
        private readonly TelegramFieldDefinition _definition;

        public TelegramField(TelegramFieldDefinition definition)
        {
            _definition = definition;
        }

        public IList<object> Values { get; } = new List<object>();
        public string SingleValue => string.Join("; ", Values);

        public void AddValue(string value)
        {
            var parsedValue = _definition.ParseValue(Values.Count, value);
            Values.Add(parsedValue);
        }

        public override string ToString()
        {
            return $"{_definition.GetLabel()}: {SingleValue}";
        }

        public bool IsField(string fieldCode)
        {
            return _definition.Code == fieldCode;
        }
    }
}
