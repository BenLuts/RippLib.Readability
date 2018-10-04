using RippLib.Util.Tests.POCO;
using System;
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
            var dic = list.CreateDictionaryFromSinglePropertyValues(x => x.ID);
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
    }
}
