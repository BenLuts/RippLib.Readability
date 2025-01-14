using FluentAssertions;
using RippLib.Readability;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xunit;

namespace Enumerables;
public class Has
{
    public class Lists
    {
        [Fact]
        public void Searching_for_non_existing_value_should_return_false()
        {
            var listWithValues = new List<string>() { "not so empty string", "another not so empty string" };
            var result = listWithValues.Has(x => x.StartsWith("none existing value"));
            result.Should().BeFalse();
        }

        [Fact]
        public void Searching_for_existing_value_should_return_true()
        {
            var listWithValues = new List<string>() { "not so empty string", "another not so empty string" };
            var result = listWithValues.Has(x => x.StartsWith("not"));
            result.Should().BeTrue();
        }
    }

    public class Arrays
    {
        [Fact]
        public void Searching_for_non_existing_value_should_return_false()
        {
            var arrayWithValues = new string[2] { "not so empty string", "another not so empty string" };
            var result = arrayWithValues.Has(x => x.StartsWith("none existing value"));
            result.Should().BeFalse();
        }

        [Fact]
        public void Searching_for_existing_value_should_return_true()
        {
            var arrayWithValues = new string[2] { "not so empty string", "another not so empty string" };
            var result = arrayWithValues.Has(x => x.StartsWith("not"));
            result.Should().BeTrue();
        }
    }

    public class Enumerable
    {
        [Fact]
        public void Searching_for_non_existing_value_should_return_false()
        {
            var enumerableWithValues = new Collection<string>() { "not so empty string", "another not so empty string" }.AsEnumerable();
            var result = enumerableWithValues.Has(x => x.StartsWith("none existing value"));
            result.Should().BeFalse();
        }

        [Fact]
        public void Searching_for_existing_value_should_return_true()
        {
            var enumerableWithValues = new List<string>() { "not so empty string", "another not so empty string" }.AsEnumerable();
            var result = enumerableWithValues.Has(x => x.StartsWith("not"));
            result.Should().BeTrue();
        }
    }
}
