using RippLib.Util.Tests.POCO;
using System;
using Xunit;

namespace RippLib.Util.Tests
{
    public class CollectionTest
    {
        [Fact]
        public void TestDictionaryExtension()
        {
            var list = SimplePoco.CreateDummyList();
            var dic = list.CreateDictionaryFromPropertyValues(x=>x.CharSequence);
            Assert.NotNull(dic);
        }
    }
}
