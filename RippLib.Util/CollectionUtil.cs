using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RippLib.Util
{
    public static class CollectionUtil
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> list)
        {
            var rnd = new Random();
            var shuffledList = list.ToList();
            int n = shuffledList.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                T value = shuffledList[k];
                shuffledList[k] = shuffledList[n];
                shuffledList[n] = value;
            }
            return shuffledList;
        }
        public static Dictionary<string, T> CreateDictionaryFromSinglePropertyValues<T>(this IEnumerable<T> collection,
            Expression<Func<T, object>> propertyLambda)
        {
            var propertyMultipleDic = collection.CreateDictionaryFromPropertyValues(propertyLambda);
            var propertySingleDic = new Dictionary<string, T>();
            foreach (string key in propertyMultipleDic.Keys)
                propertySingleDic[key] = propertyMultipleDic[key][0];
            return propertySingleDic;
        }
        public static Dictionary<string, List<T>> CreateDictionaryFromPropertyValues<T>(this IEnumerable<T> collection,
            Expression<Func<T,object>> propertyLambda)
        {
            if (!(propertyLambda.Body is MemberExpression me))
            {
                UnaryExpression ubody = (UnaryExpression)propertyLambda.Body;
                me = ubody.Operand as MemberExpression;
            }
            string propertyName = me.Member.Name;

            var propertyDic = new Dictionary<string, List<T>>();

            //loop through the list
            var listEnumerator = collection.GetEnumerator();
            while (listEnumerator.MoveNext())
            {
                //construct the dictionary key
                string key = "";

                var pi = listEnumerator.Current.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if ((pi == null) && (listEnumerator.Current.GetType().BaseType != null))
                {
                    pi = listEnumerator.Current.GetType().BaseType.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                }
                object propOrFieldValue = pi.GetValue(listEnumerator.Current, null);
                //add the value of the property or field to the key
                if (propOrFieldValue != null)
                    key = propOrFieldValue.ToString();
                //add the current object to the dictionary
                if (!propertyDic.ContainsKey(key)) propertyDic[key] = new List<T>();
                propertyDic[key].Add(listEnumerator.Current);
            }
            return propertyDic;
        }
    }
}