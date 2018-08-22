using FluentAssertions;
using Rocket.Surgery.Extensions.Testing;
using Xunit.Abstractions;
using Cake.Common.Tools.GitVersion;
using Xunit;
using System.Collections.Generic;

namespace Rocket.Surgery.Cake.Tests
{
    public class SettingsTests : AutoTestBase
    {
        public SettingsTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        [Fact]
        public void ItConstructs()
        {
            var settings = new Settings(new GitVersion(), new Dictionary<string, string>());

            settings.Coverage.Should().NotBeNull();
            settings.Environment.Should().NotBeNull();
            settings.Pack.Should().NotBeNull();
            settings.Version.Should().NotBeNull();
            settings.XUnit.Should().NotBeNull();
        }
    }
}
