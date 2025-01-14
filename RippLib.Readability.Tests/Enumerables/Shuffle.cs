using FluentAssertions;
using POCO;
using RippLib.Readability;
using Xunit;

namespace Enumerables;
public class Shuffle
{
    public class List
    {
        [Fact]
        public void Shuffle_should_keep_the_same_number_of_elements()
        {
            var list = SimplePoco.CreateDummyList(20);
            var shuffledList = list.Shuffle();
            shuffledList.Count.Should().Be(list.Count);
        }
    }
}
