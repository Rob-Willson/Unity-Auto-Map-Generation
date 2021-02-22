using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkController : MonoBehaviour
{
    public List<TilingController> chunkTilingControllers = new List<TilingController>();
    public List<TerrainGenerator> chunkTerrainGenerators = new List<TerrainGenerator>();
    public List<BorderController> chunkBorderControllers = new List<BorderController>();
    private Transform tr;
    private WaitForEndOfFrame wait = new WaitForEndOfFrame();
    private WaitForSeconds waitTenthOfSecond = new WaitForSeconds(0.1f);

    // Has the chunk started recycling yet?
    bool isRecyclingChunk;

    void Start ()
    {
        tr = this.transform;
        FindAllChildTilingControllers();
        FindAllChildTerrainGenerators();
        FindAllChildBorderControllers();
        StartCoroutine(InitializeTilingControllers());
    }

    IEnumerator InitializeTilingControllers ()
    {
        int i = 0;
        while(!isRecyclingChunk && i < chunkTilingControllers.Count)
        {
            chunkTilingControllers[i].Initialize();
            i++;
            yield return wait;
        }
        // Start next phase: terrain generation
        StartCoroutine(InitializeTerrainGeneration());
    }

    IEnumerator InitializeTerrainGeneration ()
    {
        // Start a timer until initializing terrain generation on chunks and continually check that this chunk hasn't started recycling into the pools
        float timeUntilInitialize = 1f;
        while(!isRecyclingChunk && timeUntilInitialize > 0)
        {
            timeUntilInitialize -= Time.unscaledDeltaTime;
            yield return null;
        }

        for(int i = 0; i < chunkTerrainGenerators.Count; i++)
        {
            chunkTerrainGenerators[i].Initialize();
        }

        // Start next phase: border generation
        StartCoroutine(InitializeBorderGeneration());
    }

    IEnumerator InitializeBorderGeneration ()
    {
        bool init = false;
        while(!isRecyclingChunk && !init)
        {
            if(tr.position.x < LevelGenerator.instance.highestChunkTile.x
                && tr.position.x > LevelGenerator.instance.lowestChunkTile.x
                && tr.position.y < LevelGenerator.instance.highestChunkTile.y
                && tr.position.y > LevelGenerator.instance.lowestChunkTile.y)
            {
                for(int i = 0; i < chunkBorderControllers.Count; i++)
                {
                    chunkBorderControllers[i].Initialize();
                }
                init = true;
            }
            yield return waitTenthOfSecond;
        }
    }

    public void RecycleChunk ()
    {
        StartCoroutine(LabelChunkForPooling());
        isRecyclingChunk = true;
    }

    // Quickly (but sequentially) start recycling the chunk's components into the pools
    public IEnumerator LabelChunkForPooling ()
    {
        WaitForSeconds waitUntilNextFrame = new WaitForSeconds(0);

        // Gradually loop through and unload all into the pools (or delete if no pool)
        int i;
        while(this.transform.childCount > 0)
        {
            i = tr.childCount - 1;
            Pool.instance.AddTileToPool(tr.GetChild(i).gameObject);
            yield return waitUntilNextFrame;
        }

        Destroy(gameObject);
    }

    public void FindAllChildTilingControllers ()
    {
        chunkTilingControllers.Clear();
        for(int i = 0; i < transform.childCount; i++)
        {
            chunkTilingControllers.Add(transform.GetChild(i).GetComponent<TilingController>());
        }
    }

    public void FindAllChildTerrainGenerators ()
    {
        chunkTerrainGenerators.Clear();
        for(int i = 0; i < transform.childCount; i++)
        {
            chunkTerrainGenerators.Add(transform.GetChild(i).GetComponent<TerrainGenerator>());
        }
    }

    public void FindAllChildBorderControllers ()
    {
        chunkBorderControllers.Clear();
        for(int i = 0; i < transform.childCount; i++)
        {
            for(int n = 0; n < transform.GetChild(i).childCount; n++)
            {
                if(transform.GetChild(i).GetChild(n).gameObject.layer == 9)
                {
                    chunkBorderControllers.Add(transform.GetChild(i).GetChild(n).GetComponent<BorderController>());
                }
            }
        }
    }
}
