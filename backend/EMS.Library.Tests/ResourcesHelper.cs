using System;
using EMS.Library;
using Xunit;
using System.Reflection;

namespace ResourcesHelper
{
	public class ResourcesHelperTests
	{
        [Fact(DisplayName = "Load valid resource returns proper content")]
        public void LoadValidResource()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var str = EMS.Library.ResourceHelper.ReadAsString(assembly, "EMS.Library.Tests.ResourcesHelper.Resource.txt");
            Assert.NotNull(str);
            Assert.False(string.IsNullOrWhiteSpace(str));
            Assert.Equal("dit is een embedded resource text", str);
        }

        [Fact(DisplayName = "Load invalid resource throws FileNotFoundException")]
        public void LoadInvalidResourceThrowsFileNotFoundException()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            EMS.Library.ResourceHelper.LogAllResourcesInAssembly(assembly);
            Assert.Throws<FileNotFoundException>(() => EMS.Library.ResourceHelper.ReadAsString(assembly, "Resource that doesn't exist.txt"));
        }
    }
}

