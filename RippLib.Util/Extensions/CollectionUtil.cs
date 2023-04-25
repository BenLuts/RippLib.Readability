using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RippLib.Util
{
    public static class CollectionUtil
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> collection)
        {
            var rnd = new Random();
            var shuffledList = collection.ToList();
            var n = shuffledList.Count;
            while (n > 1)
            {
                n--;
                var k = rnd.Next(n + 1);
                var value = shuffledList[k];
                shuffledList[k] = shuffledList[n];
                shuffledList[n] = value;
            }
            return shuffledList;
        }

        [Obsolete("Use Linq ToDictionary instead")]
        public static Dictionary<string, T> CreateDictionaryFromSinglePropertyValues<T>(this IEnumerable<T> collection,
            Func<T, string> propertyLambda)
        {
            var dic = collection.ToDictionary(propertyLambda);
            return dic;
        }

        public static Dictionary<string, List<T>> CreateDictionaryFromPropertyValues<T>(this IEnumerable<T> collection,
            Expression<Func<T, object>> propertyLambda)
        {
            if (!(propertyLambda.Body is MemberExpression me))
            {
                var ubody = (UnaryExpression)propertyLambda.Body;
                me = ubody.Operand as MemberExpression;
            }
            var propertyName = me.Member.Name;

            var propertyDic = new Dictionary<string, List<T>>();

            //loop through the list
            IEnumerator<T> listEnumerator = collection.GetEnumerator();
            while (listEnumerator.MoveNext())
            {
                //construct the dictionary key
                var key = "";

                var pi = listEnumerator.Current.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if ((pi is null) && !(listEnumerator.Current.GetType().BaseType is null))
                {
                    pi = listEnumerator.Current.GetType().BaseType.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                }
                var propOrFieldValue = pi.GetValue(listEnumerator.Current, null);
                //add the value of the property or field to the key
                if (!(propOrFieldValue is null))
                    key = propOrFieldValue.ToString();
                //add the current object to the dictionary
                if (!propertyDic.ContainsKey(key)) propertyDic[key] = new List<T>();
                propertyDic[key].Add(listEnumerator.Current);
            }
            return propertyDic;
        }

        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> collection, int batchSize)
        {
            var nextbatch = new List<T>(batchSize);
            foreach (var item in collection)
            {
                nextbatch.Add(item);
                if (nextbatch.Count == batchSize)
                {
                    yield return nextbatch;
                    nextbatch = new List<T>(batchSize);
                }
            }
            if (nextbatch.Count > 0)
                yield return nextbatch;
        }

        public static bool NotEmpty<T>(this IEnumerable<T> list)
        {
            return list is not null && list.Any();
        }
        public static bool NotEmpty<T>(this IEnumerable<T> list, Func<T, bool> query)
        {
            return list is not null && list.Any(query);
        }
        public static bool Empty<T>(this IEnumerable<T> list)
        {
            return list is null || !list.Any();
        }
        public static bool Empty<T>(this IEnumerable<T> list, Func<T, bool> query)
        {
            return list is null || !list.Any(query);
        }
    }
}