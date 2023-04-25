using RippLib.Util.Tests.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RippLib.Util.Tests
{
    public class CollectionTest
    {
        [Fact]
        public void TestDictionaryExtension()
        {
            var list = SimplePoco.CreateDummyList(200);
            var dic = list.CreateDictionaryFromPropertyValues(x=>x.CharSequence);
            Assert.NotNull(dic);
        }

        [Fact]
        public void TestDictionarySinglePropertyExtension()
        {
            var list = SimplePoco.CreateDummyList(200);
            var dic = list.CreateDictionaryFromSinglePropertyValues(x => x.ID.ToString());
            Assert.NotNull(dic);
            Assert.True(dic.ContainsKey(0.ToString()));
        }

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
            Assert.True(batchList.Count == 10);
            batchList.ForEach(x => Assert.True(x.Count() == 2));
        }

        [Fact]
        public void Empty_Null_ReturnsTrue()
        {
            //Arrange
            var list = (List<object>)null;

            //Act
            var result = list.Empty();

            //Assert
            Assert.True(result);
        }

        [Fact]
        public void Empty_ReturnsTrue()
        {
            //Arrange
            var list = new List<object>();

            //Act
            var result = list.Empty();

            //Assert
            Assert.True(result);
        }

        [Fact]
        public void Empty_WithItems_ReturnsTrue()
        {
            //Arrange
            var list = new List<object>() { new() };

            //Act
            var result = list.Empty();

            //Assert
            Assert.False(result);
        }

        [Fact]
        public void Empty_WithItems_LambdaExpression_ReturnsTrue()
        {
            //Arrange
            var list = new List<string>() { "not so empty string", "another not so empty string" };

            //Act
            var result = list.Empty(x => x.StartsWith("values"));

            //Assert
            Assert.True(result);
        }

        [Fact]
        public void Empty_WithItems_LambdaExpression_Matches_ReturnsFalse()
        {
            //Arrange
            var list = new List<string>() { "not so empty string", "another not so empty string" };

            //Act
            var result = list.Empty(x => x.StartsWith("not"));

            //Assert
            Assert.False(result);
        }

        [Fact]
        public void NotEmpty_Null_ReturnsFalse()
        {
            //Arrange
            var list = (List<object>)null;

            //Act
            var result = list.NotEmpty();

            //Assert
            Assert.False(result);
        }

        [Fact]
        public void NotEmpty_ReturnsFalse()
        {
            //Arrange
            var list = new List<object>();

            //Act
            var result = list.NotEmpty();

            //Assert
            Assert.False(result);
        }

        [Fact]
        public void NotEmpty_WithItems_ReturnsTrue()
        {
            //Arrange
            var list = new List<object>() { new() };

            //Act
            var result = list.NotEmpty();

            //Assert
            Assert.True(result);
        }

        [Fact]
        public void NotEmpty_WithItems_LambdaExpression_ReturnsFalse()
        {
            //Arrange
            var list = new List<string>() { "not so empty string", "another not so empty string" };

            //Act
            var result = list.NotEmpty(x => x.StartsWith("values"));

            //Assert
            Assert.False(result);
        }

        [Fact]
        public void NotEmpty_WithItems_LambdaExpression_Matches_ReturnsTrue()
        {
            //Arrange
            var list = new List<string>() { "not so empty string", "another not so empty string" };

            //Act
            var result = list.NotEmpty(x => x.StartsWith("not"));

            //Assert
            Assert.True(result);
        }
    }
}
