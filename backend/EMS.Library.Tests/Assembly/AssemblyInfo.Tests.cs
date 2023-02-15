using System;
using System.Reflection;
using Xunit;
using EMS.Library.Assembly;

namespace Assembly
{
    public class InfoTests
    {
        [Theory]
        [InlineData("System.Net.Http")]
        [InlineData("Microsoft.TestPlatform.CoreUtilities")]
        public void PlatformAssembliesAreNotLogged(string assemblyName)
        {
            var assemblyToTest = System.Reflection.Assembly.Load(assemblyName);
            MethodInfo dynMethod = typeof(AssemblyInfo).GetMethod("LogAssembly", BindingFlags.NonPublic | BindingFlags.Static);
            var r = dynMethod.Invoke(null, new object[] { assemblyToTest });

            Assert.IsType<bool>(r);
            Assert.Equal(false, r);
        }
        [Theory]
        [InlineData("EMS.Library")]
        [InlineData("Moq")]
        [InlineData("NLog")]
        public void TheseAssembliesAreLogged(string assemblyName)
        {
            var assemblyToTest = System.Reflection.Assembly.Load(assemblyName);
            MethodInfo dynMethod = typeof(AssemblyInfo).GetMethod("LogAssembly", BindingFlags.NonPublic | BindingFlags.Static);
            var r = dynMethod.Invoke(null, new object[] { assemblyToTest });

            Assert.IsType<bool>(r);
            Assert.Equal(true, r);
        }
    }
}
