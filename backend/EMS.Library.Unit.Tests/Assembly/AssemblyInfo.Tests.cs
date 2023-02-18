using System;
using System.Reflection;
using Xunit;
using EMS.Library.Assembly;

namespace Assembly;

public class AssemblyInfoTests
{
    [Theory(DisplayName = "Platform assemblies are not logged")]
    [InlineData("System.Net.Http")]
    [InlineData("Microsoft.TestPlatform.CoreUtilities")]
    public void PlatformAssembliesAreNotLogged(string assemblyName)
    {
        var assemblyToTest = System.Reflection.Assembly.Load(assemblyName);
        var dynMethod = typeof(AssemblyInfo).GetMethod("LogAssembly", BindingFlags.NonPublic | BindingFlags.Static);
        var r = dynMethod?.Invoke(null, new object[] { assemblyToTest });

        Assert.NotNull(r);
        Assert.IsType<bool>(r);
        Assert.Equal(false, r);
    }

    [Theory(DisplayName = "Application and thirdparty assemblies are logged")]
    [InlineData("EMS.Library")]
    [InlineData("Moq")]
    [InlineData("NLog")]
    public void TheseAssembliesAreLogged(string assemblyName)
    {
        var assemblyToTest = System.Reflection.Assembly.Load(assemblyName);
        var dynMethod = typeof(AssemblyInfo).GetMethod("LogAssembly", BindingFlags.NonPublic | BindingFlags.Static);
        var r = dynMethod?.Invoke(null, new object[] { assemblyToTest });

        Assert.NotNull(r);
        Assert.IsType<bool>(r);
        Assert.Equal(true, r);
    }
}
