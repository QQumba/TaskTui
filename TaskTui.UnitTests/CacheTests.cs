using FluentAssertions;

namespace TaskTui.UnitTests;

public class CacheTests
{
    [TestCase(Cache.DefaultCacheFileName, new[] { "default-20", "default-10" })]
    [TestCase("test.cache.json", new[] { "custom-20", "custom-10", "custom-10" })]
    public void Cache_Should_LoadCorrectFile(string fileName, string[] expectedTasks)
    {
        // act
        using var cache = new Cache(fileName);

        // assert
        cache.State.LatestTasks.Should().BeEquivalentTo(expectedTasks);
    }

    [Test]
    public void Cache_Should_LoadDefaultFile()
    {
        // act
        using var cache = new Cache();

        // assert
        cache.State.LatestTasks.Should().BeEquivalentTo("default-20", "default-10");
    }
}