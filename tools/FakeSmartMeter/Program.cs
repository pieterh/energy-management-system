using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

class MyTcpListener
{
    public static void Main()
    {
        bool done = false;
        int port = 3010;
        IPAddress localAddr = IPAddress.Parse("127.0.0.1");
        List<Task> tasks = new();
        CancellationTokenSource ct = new();

        var service = StartServiceAsync(port, localAddr, tasks, ct.Token);
        Console.WriteLine("Press Q to stop");
        Console.WriteLine("Press D to disconnect all");
        while (!done)
        {
            var keyinfo = Console.ReadKey();
            switch (keyinfo.Key)
            {
                case ConsoleKey.D:
                    ct.Cancel();
                    try
                    {
                        service.Wait(2500);
                    }
                    catch (AggregateException) { }
                    ct.Dispose();
                    service.Dispose();
                    ct = new CancellationTokenSource();
                    service = StartServiceAsync(port, localAddr, tasks, ct.Token);
                    break;
                case ConsoleKey.Q:
                    done = true;
                    ct.Cancel();
                    try
                    {
                        service.Wait(2500);
                    }
                    catch (AggregateException ex)
                    {
                        //ex.InnerExceptions
                        //    .Where((e) => { return e is not TaskCanceledException; }).ToList()
                        //    .ForEach((e) => {
                        //        Console.WriteLine(ex.ToString());
                        //    });
                    }
                    break;

            }
        }
        service.Dispose();
        ct.Dispose();
    }

    private static Task StartServiceAsync(int port, IPAddress localAddr, List<Task> tasks, CancellationToken ct)
    {
        var service = Task.Factory.StartNew(async () =>
        {
            TcpListener server;
            // TcpListener server = new TcpListener(port);
            server = new TcpListener(localAddr, port);
            try
            {
                // Start listening for client requests.
                server.Start();

                // Enter the listening loop.
                while (true)
                {
                     Console.Write("Waiting for a connection... ");
                    // Perform a blocking call to accept requests.
                    var client = await server.AcceptSocketAsync(ct);
                    ct.Register(() => {
                        if (client.Connected)
                        {
                            client.Shutdown(SocketShutdown.Both);
                            client.Disconnect(true);
                        }
                        client.Dispose();
                        client = null;
                        Console.WriteLine("Disconnected!");
                    });

                    Console.WriteLine($"Got a connection... {client?.RemoteEndPoint?.ToString()}");
                    var bgTask = Task.Factory.StartNew(async () =>
                    {
                        client.Blocking = false;
                        //  Console.WriteLine("Connected!");
                        var bufC = MsgToString(message_1).ToCharArray();
                        var buf = UTF8Encoding.UTF8.GetBytes(bufC);

                        while (client.Connected)
                        {
                            await Task.Delay(1000, ct).ConfigureAwait(false);
                            try
                            {
                                await client.SendAsync(buf, SocketFlags.Partial, ct);
                            }
                            catch (SocketException se)
                            {
                                Console.WriteLine(se.SocketErrorCode);
                                
                                //
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                        }
                        Console.WriteLine("Disconnected!!");
                        client?.Disconnect(true);
                        client?.Dispose();
                    }, ct, TaskCreationOptions.None, TaskScheduler.Default).Unwrap();
                    tasks.Add(bgTask);
                    CleanupOldTasks(tasks);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            catch (OperationCanceledException oce)
            {
                Task.WaitAll(tasks.ToArray());
                CleanupOldTasks(tasks);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unhandled exception: {0}", e);
            }
            finally
            {
                server.Stop();
            }
        }, ct, TaskCreationOptions.None, TaskScheduler.Default).Unwrap();
        return service;
    }

    private static void CleanupOldTasks(List<Task> tasks)
    {
        // cleanup old bgtasks
        tasks.Where((x) => x.IsCompleted).ToList().ForEach((x) =>
        {
            x.Dispose();
            tasks.Remove(x);
        });
    }

    private static string MsgToString(string[] message)
    {
        var str = new StringBuilder();
        foreach (string line in message)
        {
            str.Append(line + '\r' + '\n');
        }
        return str.ToString();
    }

    private static string[] message_1 = {
            @"/ISK5\2M550T-1013",
            @"",
            @"1-3:0.2.8(50)",
            @"0-0:1.0.0(210307123721W)",
            @"0-0:96.1.1(4530303534303037343736393431373139)",
            @"1-0:1.8.1(002236.186*kWh)",
            @"1-0:1.8.2(001755.060*kWh)",
            @"1-0:2.8.1(000781.784*kWh)",
            @"1-0:2.8.2(001871.581*kWh)",
            @"0-0:96.14.0(0001)",
            @"1-0:1.7.0(00.000*kW)",
            @"1-0:2.7.0(00.662*kW)",
            @"0-0:96.7.21(00004)",
            @"0-0:96.7.9(00004)",
            @"1-0:99.97.0(2)(0-0:96.7.19)(190911154933S)(0000000336*s)(201017081600S)(0000000861*s)",
            @"1-0:32.32.0(00003)",
            @"1-0:52.32.0(00001)",
            @"1-0:72.32.0(00002)",
            @"1-0:32.36.0(00001)",
            @"1-0:52.36.0(00001)",
            @"1-0:72.36.0(00001)",
            @"0-0:96.13.0()",
            @"1-0:32.7.0(233.7*V)",
            @"1-0:52.7.0(233.6*V)",
            @"1-0:72.7.0(230.9*V)",
            @"1-0:31.7.0(003*A)",
            @"1-0:51.7.0(001*A)",
            @"1-0:71.7.0(000*A)",
            @"1-0:21.7.0(00.000*kW)",
            @"1-0:41.7.0(00.213*kW)",
            @"1-0:61.7.0(00.076*kW)",
            @"1-0:22.7.0(00.900*kW)",
            @"1-0:42.7.0(00.000*kW)",
            @"1-0:62.7.0(00.000*kW)",
            @"!E1FA"
        };
}