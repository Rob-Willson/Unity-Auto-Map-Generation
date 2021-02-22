using System.Collections.Generic;
using UnityEngine;

public enum SpreadType { None, Adjacent, OneSqmRadius, TwoSqmRadius, ThreeSqmRadius };

public class TerrainGeneratorSpecificOverlord : MonoBehaviour
{
    public static TerrainGeneratorSpecificOverlord instance = null;
    public List<TerrainData> terrainData = new List<TerrainData>();

    public void Awake ()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public void Start ()
    {
        for(int i = 0; i < terrainData.Count; i++)
        {
            terrainData[i].terrainID = terrainData[i].terrainPrefab.GetComponent<ThingData>().id;
        }
    }

    public TerrainData GetTerrainDataFromID (string terrainID)
    {
        for(int i = 0; i < terrainData.Count; i++)
        {
            if(terrainID == terrainData[i].terrainID)
            {
                return terrainData[i];
            }
        }
        return null;
    }

}

[System.Serializable]
public class TerrainData
{
    [HideInInspector] public string terrainID;
    public GameObject terrainPrefab;
    public SpreadType spreadType;
    public float spreadPromiscuity;
}
