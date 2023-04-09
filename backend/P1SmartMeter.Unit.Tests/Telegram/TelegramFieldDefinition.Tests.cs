using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using FluentAssertions;

using P1SmartMeter.Telegram;
using System.Xml.Linq;
using FluentAssertions.Equivalency;

namespace P1SmartMeter.TelegramTests
{
    [SuppressMessage("","S125")]
    public class TelegramFieldDefinitionTests
    {
        [Fact]
        public void InvalidIsDefinedCorrectly()
        {
            var invalidDef = TelegramFieldDefinition.Invalid;
            invalidDef.Code.Should().Be("invalid", "because then we can identify it");
            invalidDef.Name.Should().BeNullOrWhiteSpace("we don't have a proper name for an invalid field definition");
            invalidDef.Types.ToArray().Length.Should().Be(1, "it should contain only one item");
            invalidDef.Types[0].Should().Be(TelegramFieldType.Plain, "it should contain only the field type 'plain'");
        }

        [Fact]
        public void UnknownIsDefinedCorrectly()
        {
            var invalidDef = TelegramFieldDefinition.Unknown("__code_unknown_field__");
            invalidDef.Code.Should().Be("__code_unknown_field__", "that is way we have created it");
            invalidDef.Name.Should().BeNullOrWhiteSpace("we don't have a proper name for an invalid field definition");
            invalidDef.Types.ToArray().Length.Should().Be(1, "it should contain only one item");
            invalidDef.Types[0].Should().Be(TelegramFieldType.Plain, "it should contain only the field type 'plain'");
            invalidDef.GetLabel().Should().Be("__code_unknown_field__", "there is no name, the label is the same as the code");
        }

        [Fact]
        public void LabelIsDefinedCorrectly()
        {
            const string code = "__mycode__";
            const string name = "__myname__";

            var def = new TelegramFieldDefinition(code, name, new[] { TelegramFieldType.Plain });
            def.Code.Should().Be(code, "that is way we have created it");
            def.Name.Should().Be(name, "that is way we have created it");
            def.GetLabel().Should().Be($"{name} ({code})", "we created it with this name and code");
        }

        [Fact]
        public void ReturnsRawValueWhenTheTypeDoesntExists()
        {
            const string code = "__mycode__";
            const string name = "__myname__";
            var def = new TelegramFieldDefinition(code, name, new[] { TelegramFieldType.Plain });
            const string rawValue = "__rawValue__";
            def.ParseValue(0, rawValue).Should().Be(rawValue, "the type doesn't exist so the plain type is used resulting a raw value");
        }

        [Fact]
        public void ProperlyConvertsTheNumericTypeAsAnInteger()
        {
            var def = new TelegramFieldDefinition("1-0:32.32.0", "Num voltage sags L1", new[] { TelegramFieldType.Numeric });
            const string rawValue = "1234";
            var result = def.ParseValue(0, rawValue);
            result.GetType().Should().Be(typeof(int), "the type is TelegramFieldType.Numeric");
            result.Should().Be(1234, "it should be converted to a number value");
        }

        [Fact]
        public void ProperlyConvertsTheNumericWithUnitTypeAsADouble()
        {
            var def = new TelegramFieldDefinition("1-0:22.7.0", "Power Returned L1", new[] { TelegramFieldType.NumericWithUnitAsDouble });
            const string rawValue = "00.900*kW";
            var result = def.ParseValue(0, rawValue);
            result.IsValueTuple().Should().BeTrue("the value and the unit should be returned as a tuple");
            var (x, y) = (ValueTuple<double, string>)result;

            x.GetType().Should().Be(typeof(double), "the type is TelegramFieldType.NumericWithUnitAsDouble");
            x.Should().Be(0.9, "it should be converted to a number value");

            y.GetType().Should().Be(typeof(string), "the unit is always a string");
            y.Should().Be("kW", "...");
        }

        [Fact]
        public void ProperlyConvertsTheNumericWithUnitTypeAsAnInt()
        {
            var def = new TelegramFieldDefinition("1-0:31.7.0", "Current L1", new[] { TelegramFieldType.NumericWithUnit });
            const string rawValue = "003*A";
            var result = def.ParseValue(0, rawValue);
            result.IsValueTuple().Should().BeTrue("the value and the unit should be returned as a tuple");
            var (x, y) = (ValueTuple<int, string>)result;

            x.GetType().Should().Be(typeof(int), "the type is TelegramFieldType.NumericWithUnit");
            x.Should().Be(3, "it should be converted to a number value");

            y.GetType().Should().Be(typeof(string), "the unit is always a string");
            y.Should().Be("A", "...");
        }

        // [Fact]
        // public void ProperlyConvertsTheNumericTypeAsADouble()
        // {
        //     var def = new TelegramFieldDefinition() { Code = "1-0:32.32.0", Name = "Num voltage sags L1" };
        //     def.Types.Add(TelegramFieldType.Numeric);
        //     const string rawValue = "1234.56";
        //     var result = def.ParseValue(0, rawValue);
        //     result.GetType().Should().Be(typeof(double), "the type is TelegramFieldType.Numeric and there is a dot in the input");
        //     result.Should().Be((double)1234.56, "it should be converted to a double value");
        // }

        [Fact]
        public void ProperlyConvertsTheStringTypeAsAString()
        {
            var def = new TelegramFieldDefinition("0-0:96.1.1", "Equipment identifier", new[] { TelegramFieldType.String });
            const string rawValue = "SpaceX";
            var result = def.ParseValue(0, rawValue);
            result.GetType().Should().Be(typeof(string), "the type is TelegramFieldType.String");
            result.Should().Be(rawValue, "it should have the same value as input");
        }

        [Fact]
        public void ProperlyConvertsTheStringTypeAsAStringTag9()
        {
            var def = new TelegramFieldDefinition("0-0:96.1.1", "Equipment identifier", new[] { TelegramFieldType.OctetString });
            const string rawValue = "4B384547303034303436333935353037"; // K8EG004046395507
            var result = def.ParseValue(0, rawValue);
            result.GetType().Should().Be(typeof(string), "the type is TelegramFieldType.OctetString");
            result.Should().Be("K8EG004046395507", "it should be converted from hex to ascii");
        }

        [Fact]
        public void ProperlyConvertsTheTimestampTypeAsADateTime()
        {
            var def = new TelegramFieldDefinition("0-0:1.0.0", "Timestamp", new[] { TelegramFieldType.Timestamp });
            const string rawValue = "210307123722W";
            var result = def.ParseValue(0, rawValue);
            result.GetType().Should().Be(typeof(DateTime), "the type is TelegramFieldType.Timestamp");
            result.Should().Be(new DateTime(2021, 03, 07, 12, 37, 22), "it should have the expected value ;-)");
        }
    }

    static class ValueTupleHelper
    {
        private static readonly HashSet<Type> ValueTupleTypes = new(new Type[]
        {
                typeof(ValueTuple<>),
                typeof(ValueTuple<,>),
                typeof(ValueTuple<,,>),
                typeof(ValueTuple<,,,>),
                typeof(ValueTuple<,,,,>)
        });

        public static bool IsValueTuple(this object obj) => IsValueTupleType(obj.GetType());
        public static bool IsValueTupleType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType && ValueTupleTypes.Contains(type.GetGenericTypeDefinition());
        }
    }
}
