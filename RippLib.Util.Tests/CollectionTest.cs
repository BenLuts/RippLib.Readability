using RippLib.Readability;
using RippLib.Util.Tests.POCO;
using System;
using System.Linq;
using Xunit;

namespace RippLib.Util.Tests;

public class CollectionTests
{
    [Fact]
    public void TestShuffle()
    {
        var list = SimplePoco.CreateDummyList(20);
        var shuffledList = list.Shuffle().ToList();
        Assert.True(list.Count == shuffledList.Count && list[0].ID != shuffledList[0].ID);
    }

    [Fact]
    public void TestBatch()
    {
        var list = SimplePoco.CreateDummyList(20);
        var batchList = list.Batch(2).ToList();
        Assert.Equal(10, batchList.Count);
        batchList.ForEach(x => Assert.Equal(2, x.Count()));
    }
}
