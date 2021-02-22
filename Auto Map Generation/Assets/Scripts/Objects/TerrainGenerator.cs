using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    private ThingData td;
    private string id;
    private Tuple_BoolInt TGOIndex;
    private WaitForSeconds WaitBetweenStages = new WaitForSeconds(0.1f);
    private LayerMask groundLayer = 1 << 8;

    public List<Tuple_StringInt> modifiedPlacementProbabilities = new List<Tuple_StringInt>();

    void Start ()
    {
        td = GetComponent<ThingData>();
    }

    // Called by chunk
    public void Initialize ()
    {
        // Find the index of the terrain data on overlord (if it exists)
        id = td.id;
        TGOIndex = TerrainGeneratorOverlord.instance.GetIndexFromTileID(id);
        if(!TGOIndex.isTrue)
        {
            print("terrain data for tile with id: " + id + " does not exist on overlord. Cancelling terrain generation on tile at position: " + transform.position);
            // NOTE: If the above is being called with a null ID and the tile at the position no longer exists, it's because there's a big delay before terrain generation. 
            // What's happening is that the chunk goes out of focus and so starts to unload into the pools, the chunk doesn't consider this when initializing objects
            // TODO: Add a check for this case in chunk controller!
            return;
        }
        StartCoroutine(GenerateDetails1());
    }

    // Generate various floor-level details
    IEnumerator GenerateDetails1 ()
    {
        // Find out if a detail needs to be placed, and if so, place it and set its ThingData id
        GameObject detail = TerrainGeneratorOverlord.instance.GenerateDetail(TGOIndex.index);
        if(detail != null)
        {
            detail.transform.parent = this.transform;
            detail.transform.position = this.transform.position;
        }
        yield return WaitBetweenStages;
        StartCoroutine(GenerateTerrain());
    }

    // Generate various terrain objects
    IEnumerator GenerateTerrain ()
    {
        int repeatCount = 5;    // Number of times the process repeats to get a natural feel to the placement of terrain objects
        while(repeatCount > 0)
        {
            // Find out if a piece of terrain needs to be placed, and if so, place it and set its ThingData id
            GameObject terrain = TerrainGeneratorOverlord.instance.GenerateTerrain(TGOIndex.index, modifiedPlacementProbabilities);
            if(terrain != null)
            {
                // Important to set active AFTER setting the position, else the OrderInLayerController won't be right!
                terrain.transform.parent = this.transform;
                terrain.transform.position = this.transform.position;
                terrain.SetActive(true);

                // HACK: Pause briefly for all tiles to run the generation pass and instantiate objects
                yield return new WaitForSeconds(0.01f);
                repeatCount = 0;

                string terrainID = terrain.GetComponent<ThingData>().id;
                TerrainData terrainData = TerrainGeneratorSpecificOverlord.instance.GetTerrainDataFromID(terrainID);

                Vector2[] surroundingCoordinates = Positioning.ActualSurroundingCoordinates(terrainData.spreadType, transform.position);

                // Raycast down onto all surrounding tiles, for any hits get the hit tile's TerrainGenerator and add a modifier
                for(int t = 0; t < surroundingCoordinates.Length; t++)
                {
                    RaycastHit2D hit = Physics2D.Raycast(surroundingCoordinates[t], Vector2.zero, 12f, groundLayer);
                    if(hit)
                    {
                        TerrainGenerator neighbouringTerrainGenerator = hit.collider.GetComponent<TerrainGenerator>();
                        if(terrainID != null)
                        {
                            neighbouringTerrainGenerator.AdjustPlacementProbabilitiesModifiers(terrainID, terrainData.spreadPromiscuity);
                        }
                    }
                }
            }

            repeatCount -= 1;
            yield return WaitBetweenStages;
        }
        StartCoroutine(GenerateDetails2());
    }

    IEnumerator GenerateDetails2 ()
    {
        // Adjust probabilities and run a SECOND PASS OF details to help populate low-density areas
        yield return WaitBetweenStages;
        StartCoroutine(GenerateOverlayAdditions());
    }

    IEnumerator GenerateOverlayAdditions ()
    {
        // Add a small number of additional terrain items on top of certain already generated items (e.g. mushrooms on trees, moss on rocks)
        // TODO: This should actually be handled by the individual objects as they are generated, since some objects allow it, but most will not
        yield return WaitBetweenStages;
        FinishTerrainGeneration();
    }

    void FinishTerrainGeneration ()
    {
        StopAllCoroutines();
    }

    public void AdjustPlacementProbabilitiesModifiers (string id, float amount)
    {
        for(int i = 0; i < modifiedPlacementProbabilities.Count; i++)
        {
            // If a placement modifier already exists for object with given id
            if(modifiedPlacementProbabilities[i].str == id)
            {
                modifiedPlacementProbabilities[i].num += amount;
                return;
            }
        }

        Tuple_StringInt t = new Tuple_StringInt();
        t.str = id;
        t.num = amount;
        modifiedPlacementProbabilities.Add(t);
    }

}
