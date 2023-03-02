using System;
using Xunit;

namespace EMS.Unit.Tests
{
    public class ConfigurationManagerTests
    {
        [Fact(DisplayName = "Read valid config file properly")]
        public void ReadValidConfigFileProperly()
        {
            var content = ConfigurationManager.ReadConfig("config.json");
            Assert.Equal(System.Text.Json.JsonValueKind.Object, content.ValueKind);
            Assert.True(content.EnumerateObject().ToArray().Length > 0);
        }
    }
}
