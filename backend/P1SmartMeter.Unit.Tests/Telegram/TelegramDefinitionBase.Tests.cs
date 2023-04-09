using System;

using Xunit;
using Moq;
using Moq.Protected;
using FluentAssertions;

using P1SmartMeter.Telegram;

namespace P1SmartMeter.TelegramTests
{
    public class TelegramDefinitionBaseTests
    {
        [Fact]
        public void InitializesProperly()
        {
            var mock = new Mock<TelegramDefinitionBase>();
            mock.Object.GetFieldDefinitions().Should().NotBeNull();
            mock.Object.GetFieldDefinitions().Should().BeEmpty();
        }

        [Fact]
        public void AllowsOnlyUniqueFieldDefenitions()
        {
            var mock = new TelegramDefinition();
            Action act = () => mock.Init();

            act.Should().NotThrow();
            act.Should().Throw<ArgumentException>();
        }
    }

    public class TelegramDefinition : TelegramDefinitionBase
    {
        public const string VersionInformation = "1-3:0.2.8";

        public void Init()
        {
            DefineField(VersionInformation, "Version information", TelegramFieldType.String);
        }
    }
}
 