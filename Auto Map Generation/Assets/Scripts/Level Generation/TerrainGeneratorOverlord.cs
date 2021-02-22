using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneratorOverlord : MonoBehaviour
{
    public static TerrainGeneratorOverlord instance = null;
    public List<TileTerrainData> terrainData = new List<TileTerrainData>();

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
    }

    public Tuple_BoolInt GetIndexFromTileID (string id)
    {
        Tuple_BoolInt ifExistAndIndex = new Tuple_BoolInt();
        for(int i = 0; i < terrainData.Count; i++)
        {
            if(terrainData[i].id == id)
            {
                ifExistAndIndex.isTrue = true;
                ifExistAndIndex.index = i;
                return ifExistAndIndex;
            }
        }
        return ifExistAndIndex;
    }

    public GameObject GenerateDetail (int index)
    {
        float sumProbabilities = terrainData[index].probabilityOfGeneratingNothing;
        for(int i = 0; i < terrainData[index].detailsToBirthProbabilities.Length; i++)
        {
            sumProbabilities += terrainData[index].detailsToBirthProbabilities[i];
        }
        float rand = Random.Range(0f, sumProbabilities);
        float cumulativeProbabilities = 0f;
        for(int i = 0; i < terrainData[index].detailsToBirth.Length; i++)
        {
            cumulativeProbabilities += terrainData[index].detailsToBirthProbabilities[i];
            if(rand < cumulativeProbabilities)
            {
                // TODO: Should be using pool props, not instantiate
                GameObject detail = Instantiate(Pool.instance.terrainPrefabTemplate);

                // Get the selected prefab's ThingData to update value(s) of the new ThingData, then return new object
                ThingData detailThingData = terrainData[index].detailsToBirth[i].GetComponent<ThingData>();
                string id = detailThingData.id;
                detail.GetComponent<ThingData>().id = id;
                detail.name = detailThingData.name;
                return detail;
            }
        }
        return null;
    }

    public GameObject GenerateTerrain (int index, List<Tuple_StringInt> modifiedPlacementProbabilities)
    {
        float sumProbabilities = terrainData[index].probabilityOfGeneratingNothing;
        for(int i = 0; i < terrainData[index].terrainToBirthProbabilities.Length; i++)
        {
            sumProbabilities += terrainData[index].terrainToBirthProbabilities[i];
            // Adjust with modifiers from neighbouring sqm:
            for(int n = 0; n < modifiedPlacementProbabilities.Count; n++)
            {
                if(terrainData[index].terrainToBirth[i].GetComponent<ThingData>().id == modifiedPlacementProbabilities[n].str)
                {
                    sumProbabilities += modifiedPlacementProbabilities[n].num;
                }
            }
        }

        // Generate a random number between 0 and the sum of all probabilities
        float rand = Random.Range(0f, sumProbabilities);
        float cumulativeProbabilities = 0f;
        // Add up all probabilities until the cumulative value is greater than that randomly generated number
        // If this is not reached, then rand must have been within the probabilityOfGeneratingNothing value, so nothing is generated
        for(int i = 0; i < terrainData[index].terrainToBirth.Count; i++)
        {
            cumulativeProbabilities += terrainData[index].terrainToBirthProbabilities[i];

            for(int n = 0; n < modifiedPlacementProbabilities.Count; n++)
            {
                if(terrainData[index].terrainToBirth[i].GetComponent<ThingData>().id == modifiedPlacementProbabilities[n].str)
                {
                    cumulativeProbabilities += modifiedPlacementProbabilities[n].num;
                }
            }

            if(rand < cumulativeProbabilities)
            {
                GameObject terrain = Pool.instance.ExtractTerrainFromPool().gameObject;
                // Get the selected prefab's ThingData to update value(s) of the new ThingData, then return new object
                ThingData prefabThingData = terrainData[index].terrainToBirth[i].GetComponent<ThingData>();
                terrain.GetComponent<ThingData>().id = prefabThingData.id;
                terrain.name = prefabThingData.name;
                return terrain;
            }
        }
        return null;
    }

}

[System.Serializable]
public class TileTerrainData
{
    // The id of the tile to match with this terrain generation data
    public string id;
    // The base probability that no terrain is spawned. Adjust with care since this will affect ALL terrain generation phases!
    public float probabilityOfGeneratingNothing;
    // Cached copies of the various prefabs available for a tile to generate
    [Tooltip("Low-lying terrain and details which may go underneath other terrain types")]
    public GameObject[] detailsToBirth;
    public float[] detailsToBirthProbabilities;
    [Tooltip("Normal terrain")]
    public List<GameObject> terrainToBirth = new List<GameObject>();
    public float[] terrainToBirthProbabilities;
}
