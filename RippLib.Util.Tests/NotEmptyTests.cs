﻿using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xunit;
using RippLib.Readability;

namespace RippLib.Util.Tests;

public class NotEmptyTests
{
    public class Lists
    {
        [Fact]
        public void Null_value_should_return_false()
        {
            var nullList = (List<object>)null;
            var result = nullList.NotEmpty();
            result.Should().BeFalse();
        }

        [Fact]
        public void Newly_initialized_without_values_should_return_false()
        {
            var emptyList = new List<object>();
            var result = emptyList.NotEmpty();
            result.Should().BeFalse();
        }

        [Fact]
        public void Newly_initialized_with_single_value_should_return_true()
        {
            var listWithValues = new List<object>() { new() };
            var result = listWithValues.NotEmpty();
            result.Should().BeTrue();
        }

        [Fact]
        public void Newly_initialized_with_multiple_values_should_return_true()
        {
            var listWithValues = new List<object>() { new(), new() };
            var result = listWithValues.NotEmpty();
            result.Should().BeTrue();
        }

        [Fact]
        public void Searching_for_non_existing_value_should_return_false()
        {
            var listWithValues = new List<string>() { "not so empty string", "another not so empty string" };
            var result = listWithValues.NotEmpty(x => x.StartsWith("none existing value"));
            result.Should().BeFalse();
        }

        [Fact]
        public void Searching_for_existing_value_should_return_true()
        {
            var listWithValues = new List<string>() { "not so empty string", "another not so empty string" };
            var result = listWithValues.NotEmpty(x => x.StartsWith("not"));
            result.Should().BeTrue();
        }
    }

    public class Arrays
    {
        [Fact]
        public void Null_value_should_return_false()
        {
            var nullArray = (object[])null;
            var result = nullArray.NotEmpty();
            result.Should().BeFalse();
        }

        [Fact]
        public void Newly_initialized_without_values_should_return_false()
        {
            var emptyArray = Array.Empty<object>();
            var result = emptyArray.NotEmpty();
            result.Should().BeFalse();
        }

        [Fact]
        public void Newly_initialized_without_value_but_defined_size_should_return_true()
        {
            var arrayWithValues = new object[1];
            var result = arrayWithValues.NotEmpty();
            result.Should().BeTrue();
        }

        [Fact]
        public void Newly_initialized_with_single_value_should_return_true()
        {
            var arrayWithValues = new object[1] { new() };
            var result = arrayWithValues.NotEmpty();
            result.Should().BeTrue();
        }

        [Fact]
        public void Newly_initialized_with_multiple_values_should_return_true()
        {
            var arrayWithValues = new object[2] { new(), new() };
            var result = arrayWithValues.NotEmpty();
            result.Should().BeTrue();
        }

        [Fact]
        public void Searching_for_non_existing_value_should_return_false()
        {
            var arrayWithValues = new string[2] { "not so empty string", "another not so empty string" };
            var result = arrayWithValues.NotEmpty(x => x.StartsWith("none existing value"));
            result.Should().BeFalse();
        }

        [Fact]
        public void Searching_for_existing_value_should_return_true()
        {
            var arrayWithValues = new string[2] { "not so empty string", "another not so empty string" };
            var result = arrayWithValues.NotEmpty(x => x.StartsWith("not"));
            result.Should().BeTrue();
        }
    }

    public class Enumerable
    {
        [Fact]
        public void Newly_initialized_without_values_should_return_false()
        {
            var emptyEnumerable = new Collection<object>().AsEnumerable();
            var result = emptyEnumerable.NotEmpty();
            result.Should().BeFalse();
        }

        [Fact]
        public void Newly_initialized_with_single_value_should_return_true()
        {
            var enumerableWithValues = new Collection<object>() { new() }.AsEnumerable();
            var result = enumerableWithValues.NotEmpty();
            result.Should().BeTrue();
        }

        [Fact]
        public void Newly_initialized_with_multiple_values_should_return_true()
        {
            var enumerableWithValues = new Collection<object>() { new(), new() }.AsEnumerable();
            var result = enumerableWithValues.NotEmpty();
            result.Should().BeTrue();
        }

        [Fact]
        public void Searching_for_non_existing_value_should_return_false()
        {
            var enumerableWithValues = new Collection<string>() { "not so empty string", "another not so empty string" }.AsEnumerable();
            var result = enumerableWithValues.NotEmpty(x => x.StartsWith("none existing value"));
            result.Should().BeFalse();
        }

        [Fact]
        public void Searching_for_existing_value_should_return_true()
        {
            var enumerableWithValues = new List<string>() { "not so empty string", "another not so empty string" }.AsEnumerable();
            var result = enumerableWithValues.NotEmpty(x => x.StartsWith("not"));
            result.Should().BeTrue();
        }
    }
}
