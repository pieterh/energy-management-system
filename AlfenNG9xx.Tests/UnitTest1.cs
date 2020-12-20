using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using AlfenNG9xx;
using Moq;
using Moq.Protected;

namespace AlfenNG9xx.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            var alfen = new AlfenNG9xx.AlfenNg9xx(); 
            alfen.Start();
            Thread.Sleep(20000);
            try{
                alfen.Stop();            
                Task.WaitAll(alfen.BackgroundTask);
            }
            catch (System.AggregateException ae)
            {
                foreach (var ie in ae.InnerExceptions)
                {
                    if (typeof(System.OperationCanceledException) != ie.GetType())
                    {
                        Console.WriteLine($"There was a task with an error");
                        throw;
                    }
                }
            }
            alfen.Dispose();
            
        }
    }
}
