using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Moq.Protected;
using EMS.Library;

namespace EMS.Library.Tests
{
    public class BackgroundWorkerTests
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        [Fact]
        public void CreateAndDispose()
        {
            Semaphore semaphore = new Semaphore(0, 1);

            var mock = new Mock<BGTester>();

            mock.Protected().Setup("Dispose", ItExpr.IsAny<bool>()).CallBase();

            var alfen = mock.Object as BGTester;
            // and dispose immediatly
            alfen.Dispose();

            Assert.Null(alfen.BackgroundTask);
        }

        [Fact]
        public void CreateAndDoubleDispose()
        {
            Semaphore semaphore = new Semaphore(0, 1);

            var mock = new Mock<BGTester>();

            mock.Protected().Setup("Dispose", ItExpr.IsAny<bool>()).CallBase();

            var alfen = mock.Object as BGTester;
            // and dispose immediatly a few times...
            alfen.Dispose();
            alfen.Dispose();
            alfen.Dispose();
        }

        //[Fact]
        //public void CreateStopAndDispose()
        //{
        //    Semaphore semaphore = new Semaphore(0, 1);

        //    var mock = new Mock<BGTester>();

        //    mock.Protected().Setup("Dispose", ItExpr.IsAny<bool>()).CallBase();

        //    var alfen = mock.Object as BGTester;
        //    // stop and dispose immediatly
        //    alfen.Stop();
        //    alfen.Dispose();

        //    Assert.Null(alfen.BackgroundTask);
        //}

        //[Fact]
        //public void StartStop()
        //{
        //    Semaphore semaphore = new Semaphore(0, 1);

        //    var mock = new Mock<BGTester>() { CallBase = true };

        //    mock.Protected().Setup("Dispose", ItExpr.IsAny<bool>()).CallBase();

        //    //mock.Protected().Setup("ShowProductInformation").Callback(() => { }).Verifiable();
        //    mock.Protected().Setup("DoBackgroundWork").Callback(() => { Thread.Sleep(150); semaphore.Release(); }).Verifiable();

        //    var alfen = mock.Object as BGTester;
        //    alfen.Start();

        //    // immediatly after starting, the background task should be registered
        //    Assert.NotNull(alfen.BackgroundTask);

        //    // wait for the task has started, but we
        //    // don't want to wait for ever till the DoBackgroundWork has been called
        //    semaphore.WaitOne(5000);

        //    try
        //    {
        //        alfen.Stop();
        //        Task.WaitAll(new Task[] { alfen.BackgroundTask }, 5000);
        //    }
        //    catch (System.AggregateException ae)
        //    {
        //        foreach (var ie in ae.InnerExceptions)
        //        {
        //            if (typeof(System.OperationCanceledException) != ie.GetType())
        //            {
        //                Console.WriteLine($"There was a task with an error");
        //                throw;
        //            }
        //        }
        //    }

        //    alfen.Dispose();

        //    Assert.Null(alfen.BackgroundTask);
        //    mock.VerifyAll();
        //}

        //[Fact]
        //public void StartStopCrashedTask()
        //{
        //    Semaphore semaphore = new Semaphore(0, 1);

        //    var mock = new Mock<BGTester>() { CallBase = true };

        //    mock.Protected().Setup("Dispose", ItExpr.IsAny<bool>()).CallBase();

        //    //mock.Protected().Setup("ShowProductInformation").Callback(() => { }).Verifiable();
        //    mock.Protected().Setup("DoBackgroundWork").Callback(() => { Thread.Sleep(60000); semaphore.Release(); }).Verifiable();

        //    var alfen = mock.Object as BGTester;
        //    alfen.Start();

        //    // immediatly after starting, the background task should be registered
        //    Assert.NotNull(alfen.BackgroundTask);

        //    // wait for the task has started, but we
        //    // don't want to wait for ever till the DoBackgroundWork has been called
        //    semaphore.WaitOne(5000);

        //    try
        //    {
        //        alfen.Stop();
        //        Task.WaitAll(new Task[] { alfen.BackgroundTask }, 2000);
        //    }
        //    catch (System.AggregateException ae)
        //    {
        //        foreach (var ie in ae.InnerExceptions)
        //        {
        //            if (typeof(System.OperationCanceledException) != ie.GetType())
        //            {
        //                Console.WriteLine($"There was a task with an error");
        //                throw;
        //            }
        //        }
        //    }

        //    alfen.Dispose();

        //    Assert.Null(alfen.BackgroundTask);
        //    mock.VerifyAll();
        //}

        //[Fact]
        //public void StartStopCrashedTask2()
        //{
        //    Semaphore semaphore = new Semaphore(0, 1);

        //    var mock = new Mock<BGTester>() { CallBase = true };

        //    mock.Protected().Setup("Dispose", ItExpr.IsAny<bool>()).CallBase();
        //    mock.Protected().Setup("DoBackgroundWork").Callback(() => { semaphore.Release(); throw new NullReferenceException("test exception"); }).Verifiable();

        //    var alfen = mock.Object as BGTester;
        //    alfen.Start();

        //    // immediatly after starting, the background task should be registered
        //    Assert.NotNull(alfen.BackgroundTask);

        //    // wait for the task has started, but we
        //    // don't want to wait for ever till the DoBackgroundWork has been called
        //    semaphore.WaitOne(5000);
        //    // need to make sure that the task did had some time to raise the exception
        //    for(var i=0; (i < 100) && !alfen.BackgroundTask.IsCompleted; i++)
        //        Thread.Sleep(50);

        //    alfen.Stop();

        //    var ae = Record.Exception(() => Task.WaitAll(new Task[] { alfen.BackgroundTask }, 2000));

        //    alfen.Dispose();

        //    Assert.Null(alfen.BackgroundTask);
        //    Assert.NotNull(ae);
        //    Assert.Equal(typeof(AggregateException), ae.GetType());
        //    Assert.False(((AggregateException)ae).InnerExceptions.Any((ie) => (typeof(System.OperationCanceledException) == ie.GetType())));
        //    mock.VerifyAll();
        //}

        //[Fact]
        //public void StartAndDisposeWithoutStopping()
        //{
        //    Semaphore semaphore = new Semaphore(0, 1);

        //    var mock = new Mock<BGTester>() { CallBase = true };

        //    mock.Protected().Setup("Dispose", ItExpr.IsAny<bool>()).CallBase();
        //    // mock.Protected().Setup("ShowProductInformation").Callback(() => { }).Verifiable();
        //    mock.Protected().Setup("DoBackgroundWork").Callback(() => { Thread.Sleep(150); semaphore.Release(); }).Verifiable();

        //    var alfen = mock.Object as BGTester;
        //    alfen.Start();

        //    // immediatly after starting, the background task should be registered
        //    Assert.NotNull(alfen.BackgroundTask);

        //    // wait for the task has started, but we
        //    // don't want to wait for ever till the DoBackgroundWork has been called
        //    semaphore.WaitOne(5000);

        //    // the task should still be running
        //    Assert.Equal(TaskStatus.Running, alfen.BackgroundTask.Status);

        //    // begin with dispose while the background task is running
        //    alfen.Dispose();
        //    Assert.Null(alfen.BackgroundTask);
        //    mock.VerifyAll();
        //}
    }

    public class BGTester : BackgroundWorker
    {

        protected override void DoBackgroundWork()
        {

        }

        protected override void Start()
        {
            throw new NotImplementedException();
        }

        protected override void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
