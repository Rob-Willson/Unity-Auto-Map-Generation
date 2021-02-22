using System.Collections.Generic;
using UnityEngine;

public class Pool : MonoBehaviour
{
    public static Pool instance = null;

    public GameObject tilePrefabTemplate;
    public GameObject borderPrefabTemplate;
    public GameObject terrainPrefabTemplate;

    List<GameObject> tilePool = new List<GameObject>();
    List<GameObject> borderPool = new List<GameObject>();
    List<GameObject> terrainPool = new List<GameObject>();

    int tilePoolMaxSize = 10000;
    int borderPoolMaxSize = 1000;
    int terrainPoolMaxSize = 1000;

    void Awake ()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            GameObject.DontDestroyOnLoad(gameObject);
        }
        // Create lots and lots and lots of pooled objects
        for(int i = 1; i <= tilePoolMaxSize; i++)
        {
            GameObject obj = Instantiate(tilePrefabTemplate, this.transform.position, Quaternion.identity) as GameObject;
            AddTileToPool(obj);
        }
        for(int i = 1; i <= borderPoolMaxSize; i++)
        {
            GameObject obj = Instantiate(borderPrefabTemplate, this.transform.position, Quaternion.identity) as GameObject;
            AddBorderToPool(obj);
        }
        for(int i = 1; i <= terrainPoolMaxSize; i++)
        {
            GameObject obj = Instantiate(terrainPrefabTemplate, this.transform.position, Quaternion.identity) as GameObject;
            AddTerrainToPool(obj);
        }
    }

    public void AddTileToPool (GameObject obj)
    {
        Transform t = obj.transform;
        string s = "x ";
        // Strip away terrain objects and pool it separately
        for(int i = t.childCount; i > 0; i--)
        {
            s += t.GetChild(i - 1).gameObject.name + " ";

            if(t.GetChild(i - 1).gameObject.layer == 9)
            {
                AddBorderToPool(t.GetChild(i - 1).gameObject);
            }
            else if(t.GetChild(i - 1).gameObject.layer == 10)
            {
                AddTerrainToPool(t.GetChild(i - 1).gameObject);
            }
        }

        obj.name = "tileObj";
        obj.SetActive(false);
        obj.transform.parent = this.transform;
        tilePool.Add(obj);
    }

    public void AddBorderToPool (GameObject obj)
    {
        obj.name = "borderObj";
        obj.SetActive(false);
        obj.transform.parent = this.transform;
        borderPool.Add(obj);
    }

    public void AddTerrainToPool (GameObject obj)
    {
        obj.name = "terrainObj";
        obj.SetActive(false);
        obj.transform.parent = this.transform;
        terrainPool.Add(obj);
    }

    public Transform ExtractTileFromPool ()
    {
        Transform t;
        if(tilePool.Count > 0)
        {
            t = tilePool[tilePool.Count - 1].transform;
            tilePool.RemoveAt(tilePool.Count - 1);
        }
        else
        {
            // TODO: This should never actually be called in final game, it's just useful during development
            //       Instantiated objects probably won't prompt OrderInLayerController to update position correctly (because of different execution order?)
            Debug.LogWarning("Not enough tiles in pool! Instantiating new!");
            t = Instantiate(Pool.instance.tilePrefabTemplate).transform;
            t.name = "Instantiated Tile";
        }
        return t;
    }

    public Transform ExtractBorderFromPool ()
    {
        Transform t;
        if(borderPool.Count > 0)
        {
            t = borderPool[borderPool.Count - 1].transform;
            borderPool.RemoveAt(borderPool.Count - 1);
        }
        else
        {
            // TODO: This should never actually be called in final game, it's just useful during development
            //       Instantiated objects probably won't prompt OrderInLayerController to update position correctly (because of different execution order?)
            Debug.LogWarning("Not enough borders in pool! Instantiating new!");
            t = Instantiate(Pool.instance.borderPrefabTemplate).transform;
            t.name = "Instantiated Border";
        }
        return t;
    }

    public Transform ExtractTerrainFromPool ()
    {
        Transform t;
        if(terrainPool.Count > 0)
        {
            t = terrainPool[terrainPool.Count - 1].transform;
            terrainPool.RemoveAt(terrainPool.Count - 1);
        }
        else
        {
            // TODO: This should never actually be called in final game, it's just useful during development
            //       Instantiated objects probably won't prompt OrderInLayerController to update position correctly (because of different execution order?)
            Debug.LogWarning("Not enough terrain in pool! Instantiating new!");
            t = Instantiate(Pool.instance.terrainPrefabTemplate).transform;
            t.name = "Instantiated Terrain";
        }
        return t;
    }

    // HACK/TODO: Just useful for testing any areas where note enough objects are present in the pools.
    void Update ()
    {
        if(tilePool.Count == 0)
        {
            Debug.LogWarning("tilePool count down to zero? Not generating enough tiles?");
        }
        if(borderPool.Count == 0)
        {
            Debug.LogWarning("borderPool count down to zero? Not generating enough borders?");
        }
        if(terrainPool.Count == 0)
        {
            Debug.LogWarning("terrainPool count down to zero? Not generating enough terrains?");
        }
    }

}
