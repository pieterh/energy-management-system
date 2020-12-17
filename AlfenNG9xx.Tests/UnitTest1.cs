using System;
using Xunit;
using AlfenNG9xx;

namespace AlfenNG9xx.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var alfen = new AlfenNG9xx.AlfenNg9xx(); 
            alfen.Dispose();
        }
    }
}
