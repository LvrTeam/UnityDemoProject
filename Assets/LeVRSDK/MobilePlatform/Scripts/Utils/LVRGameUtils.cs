using UnityEngine;
using System.Collections.Generic;

public class LVRGameUtils
{
    static public GameObject FindChildNoWarning(GameObject obj, string name)
    {
        if (obj.name == name)
        {
            return obj;
        }

        foreach (Transform child in obj.transform)
        {
            GameObject found = FindChildNoWarning(child.gameObject, name);
            if (found)
            {
                return found;
            }
        }

        return null;
    }

    static public GameObject FindChild(GameObject obj, string name)
    {
        GameObject go = FindChildNoWarning(obj, name);
        if (go == null)
        {
            LVRDebugUtils.Print( "child " + name + " was not found!" );
        }

        return go;
    }
};
