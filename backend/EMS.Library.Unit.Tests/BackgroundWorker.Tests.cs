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
using FluentAssertions;

namespace BackgroundWorker
{
    public class BackgroundWorkerTests
    {
        [Fact]
        public void CreateAndDispose()
        {
            var mock = new Mock<EMS.Library.BackgroundWorker>
            {
                CallBase = true
            };

            mock.Object.Disposed.Should().BeFalse("Object was just created and not yet disposed");

            var bgWorker = mock.Object;
            // and dispose immediatly
            bgWorker.Dispose();
            bgWorker.Disposed.Should().BeTrue("Object is just disposed");
            bgWorker.BackgroundTask.Should().BeNull("Aftecr disposing there should not ba a background task");
        }

        [Fact]
        public void CreateAndDoubleDispose()
        {
            var mock = new Mock<EMS.Library.BackgroundWorker>
            {
                CallBase = true
            };
            mock.Object.Disposed.Should().BeFalse("Object was just created and not yet disposed");
            var bgWorker = mock.Object;
            // and dispose immediatly a few times...
            bgWorker.Dispose();
            bgWorker.Disposed.Should().BeTrue("Object is just disposed");
            bgWorker.BackgroundTask.Should().BeNull("Aftecr disposing there should not ba a background task");
            bgWorker.Dispose();
            bgWorker.Dispose();
        }

        //[Fact]
        //public void CreateStopAndDispose()
        //{
        //    // var mock = new Mock<EMS.Library.BackgroundWorker>();
        //    var mock = new Mock<BGTester>();
        //    mock.CallBase = true;

        //    var bgWorker = mock.Object;
        //    // stop and dispose immediatly
        //    bgWorker.Stop();
        //    bgWorker.Dispose();

        //    Assert.Null(bgWorker.BackgroundTask);
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
        //                g Console.WriteLine($"There was a task with an error");
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

    public class BGTester : EMS.Library.BackgroundWorker
    {

        protected override void DoBackgroundWork()
        {

        }

        protected override Task Start()
        {
            throw new NotImplementedException();
        }

        protected override void Stop()
        {
            throw new NotImplementedException();
        }
       
    }
}
