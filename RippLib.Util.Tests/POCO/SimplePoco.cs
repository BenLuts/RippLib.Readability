using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RippLib.Util.Tests.POCO
{
    public class SimplePoco
    {
        public int ID { get; set; }
        public string CharSequence { get; set; }

        public static List<SimplePoco> CreateDummyList()
        {
            var random = new Random();
            var list = new List<SimplePoco>();
            for (int i = 0; i < 1000; i++)
            {
                list.Add(new SimplePoco
                {
                    ID = i,
                    CharSequence = new string(Enumerable.Repeat("AB", 5)
                        .Select(s => s[random.Next(s.Length)]).ToArray())
                });
            }
            return list;
        }
    }
}
