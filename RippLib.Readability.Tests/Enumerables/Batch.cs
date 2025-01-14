using FluentAssertions;
using POCO;
using RippLib.Readability;
using System.Linq;
using Xunit;

namespace Enumerables;
public class Batch
{
    [Fact]
    public void Batched_lists_should_have_the_correct_size()
    {
        var batchSize = 2;
        var numberOfSets = 20;
        var list = SimplePoco.CreateDummyList(numberOfSets);

        var batchList = list.Batch(batchSize).ToList();

        batchList.Count.Should().Be(numberOfSets / batchSize);
        batchList.ForEach(x => x.Count.Should().Be(batchSize));
    }
}
