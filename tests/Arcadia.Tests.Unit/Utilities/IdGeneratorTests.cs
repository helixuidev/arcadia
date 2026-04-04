using FluentAssertions;
using Arcadia.Core.Utilities;
using Xunit;

namespace Arcadia.Tests.Unit.Utilities;

public class IdGeneratorTests : IDisposable
{
    public IdGeneratorTests()
    {
        IdGenerator.Reset();
    }

    public void Dispose()
    {
        IdGenerator.Reset();
    }

    [Fact]
    public void Generate_ReturnsArcadiaPrefix()
    {
        var id = IdGenerator.Generate();

        id.Should().StartWith("arcadia-");
    }

    [Fact]
    public void Generate_WithCustomPrefix_UsesPrefix()
    {
        var id = IdGenerator.Generate("input");

        id.Should().StartWith("input-");
    }

    [Fact]
    public void Generate_ReturnsUniqueIds()
    {
        var ids = new HashSet<string>();
        for (var i = 0; i < 1000; i++)
        {
            ids.Add(IdGenerator.Generate());
        }

        ids.Should().HaveCount(1000);
    }

    [Fact]
    public void Generate_IncrementsSequentially()
    {
        // IdGenerator uses a static counter that is shared across parallel tests, so we
        // can't assert absolute values. Verify that each call produces a strictly greater
        // numeric suffix than the previous.
        var id1 = IdGenerator.Generate();
        var id2 = IdGenerator.Generate();

        id1.Should().StartWith("arcadia-");
        id2.Should().StartWith("arcadia-");
        var n1 = long.Parse(id1.Substring("arcadia-".Length));
        var n2 = long.Parse(id2.Substring("arcadia-".Length));
        n2.Should().BeGreaterThan(n1);
    }

    [Fact]
    public async Task Generate_IsThreadSafe()
    {
        var ids = new System.Collections.Concurrent.ConcurrentBag<string>();
        var tasks = new Task[10];

        for (var i = 0; i < 10; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                for (var j = 0; j < 100; j++)
                {
                    ids.Add(IdGenerator.Generate());
                }
            });
        }

        await Task.WhenAll(tasks);

        ids.Distinct().Should().HaveCount(1000);
    }

    [Fact]
    public void Reset_RestartsCounter()
    {
        IdGenerator.Generate();
        IdGenerator.Generate();
        IdGenerator.Reset();

        var id = IdGenerator.Generate();

        id.Should().Be("arcadia-1");
    }
}
