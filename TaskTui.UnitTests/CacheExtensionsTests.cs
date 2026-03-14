using FluentAssertions;
using TaskTui.Models;

namespace TaskTui.UnitTests;

public class CacheExtensionsTests
{
    [Test]
    public void GetSortedTasks_Should_RemoveDuplicatesFromCache()
    {
        // arrange
        var latestTaskNames = new List<string> { "3", "3", "3", "4", "4", "4" };
        var actualTaskNames = new List<string> { "1", "2", "3", "4", "5", "6" };
        var expectedTaskNames = new List<string> { "3", "4", "1", "2", "5", "6" };

        var cache = FakeCache.Create(latestTaskNames);
        var tasks = actualTaskNames.Select(name => new RunnableTask(name, null)).ToList();
        var expectedTasks = expectedTaskNames.Select(name => new RunnableTask(name, null));
        
        // act
        var sorterTasks = cache.GetSortedTasks(tasks);

        // assert
        sorterTasks.Should().BeEquivalentTo(expectedTasks);
        cache.State.LatestTasks.Should().BeEquivalentTo("3", "4");
    }

    [Test]
    public void GetSortedTasks_Should_RemoveNonExistentTasksFromCache()
    {
        // arrange
        var latestTaskNames = new List<string> { "5", "5", "8", "8", "2", "2" };
        var actualTaskNames = new List<string> { "1", "2", "3", "4", "5", "6" };
        var expectedTaskNames = new List<string> { "5", "2", "1", "3", "4", "6" };

        var cache = FakeCache.Create(latestTaskNames);
        var tasks = actualTaskNames.Select(name => new RunnableTask(name, null)).ToList();
        var expectedTasks = expectedTaskNames.Select(name => new RunnableTask(name, null));
        
        // act
        var sorterTasks = cache.GetSortedTasks(tasks);

        // assert
        sorterTasks.Should().BeEquivalentTo(expectedTasks);
        cache.State.LatestTasks.Should().BeEquivalentTo("5", "2");
    }
}

public class FakeCache : ICache
{
    public required CacheState State { get; set; }

    public static FakeCache Create(List<string> latestTasks)
    {
        return new FakeCache
        {
            State = new CacheState
            {
                LatestTasks = latestTasks
            }
        };
    }
}