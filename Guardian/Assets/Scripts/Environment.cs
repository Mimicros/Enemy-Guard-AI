using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment
{
    static Environment instance;
    List<GameObject> wp = new List<GameObject>();
    public List<GameObject> Wp  {get{return wp;}}

    public static Environment Singleton {
        get{
            if(instance==null)
            {
                instance = new Environment();
                instance.Wp.AddRange(GameObject.FindGameObjectsWithTag("Wp"));

            }
            return instance;
        }
    }

}
