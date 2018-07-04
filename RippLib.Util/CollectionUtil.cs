using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace RippLib.Util
{
    public static class CollectionUtil
    {
        public static void Shuffle<T>(IList<T> list, Random rnd)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        //public static Dictionary<string, TListObject> CreateDictionaryFromSinglePropertyValues<TListObject>(IEnumerable<TListObject> listToConvert, params string[] propertyOrFieldNames)
        //{
        //    Dictionary<string, List<TListObject>> propertyMultipleDic = CreateDictionaryFromPropertyValues(listToConvert, propertyOrFieldNames);
        //    Dictionary<string, TListObject> propertySingleDic = new Dictionary<string, TListObject>();

        //    foreach (string key in propertyMultipleDic.Keys)
        //        propertySingleDic[key] = propertyMultipleDic[key][0];

        //    return propertySingleDic;
        //}
        //public static Dictionary<string, List<TListObject>> CreateDictionaryFromPropertyValues<TListObject>(IEnumerable<TListObject> listToConvert, params string[] propertyOrFieldNames)
        //{
        //    Dictionary<string, List<TListObject>> propertyDic = new Dictionary<string, List<TListObject>>();

        //    //loop through the list
        //    IEnumerator<TListObject> listEnumerator = listToConvert.GetEnumerator();
        //    while (listEnumerator.MoveNext())
        //    {
        //        //construct the dictionary key
        //        string key = "";
        //        foreach (string propertyOrFieldName in propertyOrFieldNames)
        //        {
        //            object propOrFieldValue = ObjectUtil.GetPropertyOrFieldValue(listEnumerator.Current, propertyOrFieldName);
        //            //add the value of the property or field to the key
        //            if (propOrFieldValue != null)
        //                key += propOrFieldValue.ToString();
        //        }

        //        //add the current object to the dictionary
        //        if (!propertyDic.ContainsKey(key)) propertyDic[key] = new List<TListObject>();
        //        propertyDic[key].Add(listEnumerator.Current);
        //    }

        //    return propertyDic;
        //}

        //public string GetPropertyName<T>(Expression<Func<T>> propertyLambda)
        //{
        //    var me = propertyLambda.Body as MemberExpression;

        //    if (me == null)
        //    {
        //        throw new ArgumentException("You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'");
        //    }

        //    return me.Member.Name;
        //}

        public static Dictionary<string, List<TListObject>> CreateDictionaryFromPropertyValues<TListObject>(this IEnumerable<TListObject> collection,
            Expression<Func<TListObject,object>> propertyLambda)
        {
            if (!(propertyLambda.Body is MemberExpression me))
            {
                UnaryExpression ubody = (UnaryExpression)propertyLambda.Body;
                me = ubody.Operand as MemberExpression;
            }
            string propertyName = me.Member.Name;

            var propertyDic = new Dictionary<string, List<TListObject>>();

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
                if (!propertyDic.ContainsKey(key)) propertyDic[key] = new List<TListObject>();
                propertyDic[key].Add(listEnumerator.Current);
            }
            return propertyDic;
        }
    }
}
