using System;
using System.Collections.Generic;
using System.Linq;

namespace RippLib.Util.Graph;

[Obsolete("Will be removed in the future")]
public class ObjectGraph
{
    public Dictionary<Type, string[]> GraphingDic { get; set; }

    public ObjectGraph()
    {
        GraphingDic = [];
    }

    public bool Contains(Type type, string propertyName)
    {
        return GraphingDic.ContainsKey(type) && GraphingDic[type].Contains(propertyName);
    }
    public void AddGraph(Type type, params string[] properties)
    {
        if (null != properties)
        {
            if (GraphingDic.TryGetValue(type, out var values))
            {
                var startPos = values.Length;
                var newArray = new string[values.Length + properties.Length];
                values.CopyTo(newArray, 0);
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
