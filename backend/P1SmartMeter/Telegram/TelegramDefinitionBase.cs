using System.Collections.Generic;
using System.Linq;

namespace P1SmartMeter.Telegram
{
    public interface ITelegramDefinition
    {
        IList<TelegramFieldDefinition> GetFieldDefinitions();
    }

    public class TelegramDefinitionBase : ITelegramDefinition
    {
        private readonly List<TelegramFieldDefinition> _fieldDefinitions = new();

        protected void DefineField(string code, string name, params TelegramFieldType[] types)
        {
            if (!types.Any())
            {
                // default: numeric with unit
                types = new[] { TelegramFieldType.NumericWithUnit };
            }

            _fieldDefinitions.Add(new TelegramFieldDefinition(code, name, types));
        }

        public IList<TelegramFieldDefinition> GetFieldDefinitions()
        {
            return _fieldDefinitions;
        }
    }
}
