using System.Collections.Generic;
using UnityEngine;

public class DevExtension
{
    public static List<T> Shuffle<T>(List<T> _list)
    {
        for (int i = 0; i < _list.Count; i++)
        {
            T temp = _list[i];
            int randomIndex = Random.Range(i, _list.Count);
            _list[i] = _list[randomIndex];
            _list[randomIndex] = temp;
        }

        return _list;
    }

    public static string ToString<VirtualMove>(List<VirtualMove> _list)
    {
        string result = "";
        foreach(var move in  _list)
        {
            result += move.ToString() + "\n";
        }
        return result;
    }

    public static bool Equals(List<AnimalProp> props1, List<AnimalProp> props2)
    {
        if(props1.Count != props2.Count) return false;
        for (int i = 0; i < props1.Count; i++) {
            AnimalProp prop1 = props1[i];
            for(int j = i; j < props2.Count; j++)
            {

            }
        }
        return true;
    }

}
