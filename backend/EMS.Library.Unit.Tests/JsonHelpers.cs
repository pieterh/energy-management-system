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

        [Fact]
        void IsAbleToHandleIncorrectJavascriptObject()
        {
            var basicJavascriptObject =
            """            
                serial = "122011110123"            
            """;
            var resp = JsonHelpers.JavascriptObjectToDictionary(basicJavascriptObject);
            resp.Should().BeEmpty();
        }

        [Fact]
        void IsAbleToParseBasicJavascriptObject()
        {
            var basicJavascriptObject = """
            {
                serial: "122011110123",
                profiles: false,
                show_prompt: false,
                internal_meter: true,
                software_version: "R4.10.35 (6ed292)",
                envoy_type: "EU",
                polling_interval: 300000,
                polling_frequency: 60,
                backbone_public: true,
                cte_mode: false,
                toolkit: false,
                max_errors: 0,
                max_timeouts: 0,
                e_units: "sig_fig"
            }
            """;
            var resp = JsonHelpers.JavascriptObjectToDictionary(basicJavascriptObject);
            resp.Should().NotBeNullOrEmpty();
            resp.GetValueOrDefault("serial").Should().Be("122011110123");
            resp.GetValueOrDefault("profiles").Should().Be("false");
            resp.GetValueOrDefault("polling_interval").Should().Be("300000");
        }
    }
}

