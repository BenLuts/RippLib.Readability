using System;
using System.Collections.Generic;
using System.Linq;

namespace RippLib.Util.Graph
{
    public class ObjectGraph
    {
        public Dictionary<Type, string[]> GraphingDic { get; set; }

        public ObjectGraph()
        {
            GraphingDic = new Dictionary<Type, string[]>();
        }

        public bool Contains(Type type, string propertyName)
        {
            return GraphingDic.ContainsKey(type) && GraphingDic[type].Contains(propertyName);
        }
        public void AddGraph(Type type, params string[] properties)
        {
            if (null != properties)
            {
                if (GraphingDic.ContainsKey(type))
                {
                    var startPos = GraphingDic[type].Length;
                    var newArray = new string[GraphingDic[type].Length + properties.Length];
                    GraphingDic[type].CopyTo(newArray, 0);
                    for (var i = 0; i < properties.Length; i++)
                    {
                        newArray[i + startPos] = properties[i];
                    }
                    GraphingDic[type] = newArray;
                }
                else
                {
                    GraphingDic.Add(type, properties);
                }
            }
        }
    }
}
