using System.Collections.Generic;
using System.Linq;

namespace P1SmartMeter.Telegram
{
    public class TelegramDefinitionBase
    {
        private readonly List<TelegramFieldDefinition> _fieldDefinitions = new List<TelegramFieldDefinition>();

        protected void DefineField(string code, string name, params TelegramFieldType[] types)
        {
            if (!types.Any())
            {
                // default: numeric with unit
                types = new[] { TelegramFieldType.NumericWithUnit };
            }

            _fieldDefinitions.Add(new TelegramFieldDefinition
            {
                Code = code,
                Name = name,
                Types = types
            });
        }

        public IList<TelegramFieldDefinition> GetFieldDefinitions()
        {
            return _fieldDefinitions;
        }
    }
}
