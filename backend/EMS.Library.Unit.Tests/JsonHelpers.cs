using System;
using System.Text.Json;
using EMS.Library.JSon;

namespace JsonHelpersUnitTests
{
    public class JsonHelpersTest
    {
        private const string validSchema = "JsonHelpersExampleSchema.json";
        private const string invalidSchema = "ResourcesHelper.Resource.txt";

        [Fact]
        void IsAbleToReadSchemaFromResource()
        {
            var schema = JsonHelpers.GetSchema(validSchema);
            schema.Should().NotBeNull();
        }

        [Fact]
        void ThrowsExceptionWhenReadingInvalidSchemaFromResource()
        {
            Action act = () => { JsonHelpers.GetSchema(invalidSchema); };
            act.Should().Throw<JsonException>();
        }

        [Fact]
        void IsAbleToLoadAndRegisterSchema()
        {
            Uri schemaUri = new("https://petteflet.org");
            var isLoaded = JsonHelpers.LoadAndRegisterSchema(schemaUri, validSchema);
            isLoaded.Should().BeTrue();
        }

        [Fact]
        void IsAbleToValidateSchema()
        {
            Uri schemaUri = new("https://petteflet.org");
            var isLoaded = JsonHelpers.LoadAndRegisterSchema(schemaUri, validSchema);
            isLoaded.Should().BeTrue();
            var validJsonString =
                """
                {
                   "latitude": 48.858093,
                   "longitude": 2.294694
                }
                """;
            using var json = JsonDocument.Parse(validJsonString);
            var isValid = JsonHelpers.Evaluate(validSchema, json.RootElement);
            isValid.Should().BeTrue();
        }

        [Fact]
        void IsAbleToValidateSchemaInvalid()
        {
            Uri schemaUri = new("https://petteflet.org");
            var isLoaded = JsonHelpers.LoadAndRegisterSchema(schemaUri, validSchema);
            isLoaded.Should().BeTrue();
            var validJsonString =
                """
                {
                   "latitude": 48.858093,
                   "longitude": "2.294694"
                }
                """;
            using var json = JsonDocument.Parse(validJsonString);
            var isValid = JsonHelpers.Evaluate(validSchema, json.RootElement);
            isValid.Should().BeFalse();
        }
    }
}

