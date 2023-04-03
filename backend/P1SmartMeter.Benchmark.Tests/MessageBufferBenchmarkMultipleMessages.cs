namespace P1SmartMeter.Benchmark.Tests;

[MemoryDiagnoser]
[Config(typeof(Config))]
[SimpleJob(RuntimeMoniker.Net70)]
public class MessageBufferBenchmarkMultipleMessages
{
    [SuppressMessage("", "CA1812")]
    [SuppressMessage("", "S125")]
    private sealed class Config : ManualConfig
    {
        public Config()
        {
            //AddJob(Job.MediumRun.WithGcServer(true).WithGcForce(true).WithId("ServerForce"));
            //AddJob(Job.MediumRun.WithGcServer(true).WithGcForce(false).WithId("Server"));
            //AddJob(Job.MediumRun.WithGcServer(false).WithGcForce(true).WithId("Workstation"));
            //AddJob(Job.MediumRun.WithGcServer(false).WithGcForce(false).WithId("WorkstationForce"));
        }
    }

    [Fact]
    [Benchmark(Description = "Multiple - baseline", Baseline = true)]
    public void Test1()
    {
        var mb = new MessageBufferBaseline();
        mb.Add(message_1);
        mb.Add("/ISK5\\2M550T-1013\n\r1-3:0.2.8(50)<noise>");
        mb.Add(message_1);
        mb.Add("/ISK5\\2M550T-1013\n\r1-3:0.2.8(50)<noise>");
        mb.Add(message_1);
        _ = mb.TryTake(out _);
        _ = mb.TryTake(out _);
        _ = mb.TryTake(out _);
        mb.IsEmpty.Should().BeTrue();
    }

    [Fact]
    [Benchmark(Description = "Multiple - optimized")]
    public void Test2()
    {
        var mb = new MessageBuffer();
        mb.Add(message_1);
        mb.Add("/ISK5\\2M550T-1013\n\r1-3:0.2.8(50)<noise>");
        mb.Add(message_1);
        mb.Add("/ISK5\\2M550T-1013\n\r1-3:0.2.8(50)<noise>");
        mb.Add(message_1);
        _ = mb.TryTake(out _);
        _ = mb.TryTake(out _);
        _ = mb.TryTake(out _);
        mb.IsEmpty.Should().BeTrue();
    }

    private static string MsgToString(params string[] message)
    {
        var str = new StringBuilder();
        foreach (string line in message)
        {
            str.Append(line + '\r' + '\n');
        }
        return str.ToString();
    }

    private static string message_1 = MsgToString(
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
        );
}
