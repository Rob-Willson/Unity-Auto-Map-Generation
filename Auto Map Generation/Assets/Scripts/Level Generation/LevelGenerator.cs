using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator instance = null;

    public Texture2D map;
    public TileCache[] tileCache;

    // Adjust these values below to control how many chunks to load in at one time (based on player's screen resolution and settings)
    public int chunkSize = 10;          // Size of a single chunk in in-game sqm
    public int chunkGridWidth = 4;      // How many horizontal chunks will be loaded
    public int chunkGridHeight = 3;     // How many vertical chunks will be loaded

    public Vector2 lowestChunkTile;
    public Vector2 highestChunkTile;

    public List<Transform> allLoadedChunks = new List<Transform>();
    public List<Vector2> surroundingChunks = new List<Vector2>();

    public Vector2 lastPlayerChunkPosition; // The transform.position of the chunk that the player is currently in

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

        // Get the identifying colors and ids for all prefabs
        for(int i = 0; i < tileCache.Length; i++)
        {
            ThingData data = tileCache[i].prefab.GetComponent<ThingData>();
            // Get the id, ThingType and LayerType for each of the prefabs
            tileCache[i].id = data.id;
            tileCache[i].thingType = data.thingType;
            tileCache[i].blockMovement = data.blockMovement;
            tileCache[i].objectLayerType = tileCache[i].prefab.GetComponent<OrderInLayerController>().objectLayerType;
            // Get the prefab's identifying color only if it is not clear (0,0,0,0)

            // TEMP: checking if color is already set in inspector
            if(tileCache[i].color == Color.clear) tileCache[i].color = data.color;
        }
    }

    void Start ()
    {
        // Start generating the chunk around the camera's position
        Vector3 pos = Camera.main.transform.position;
        pos = ResolveVectorToChunk(pos);

        // Get relative positions of the chunks surrounding player and load in the required number
        for(int xx = -chunkGridWidth * chunkSize; xx <= chunkGridWidth * chunkSize; xx += chunkSize)
        {
            for(int yy = -chunkGridHeight * chunkSize; yy <= chunkGridHeight * chunkSize; yy += chunkSize)
            {
                Vector3 chunkPos = new Vector3(xx, yy, 0);
                chunkPos += pos;
                GenerateChunk(chunkPos);
            }
        }

        lastPlayerChunkPosition = pos;
        StartCoroutine(CheckIfNewChunksNeedLoading());
    }

    IEnumerator CheckIfNewChunksNeedLoading ()
    {
        while(true)
        {
            Vector2 pos = ResolveVectorToChunk(Camera.main.transform.position);
            if(!pos.Equals(lastPlayerChunkPosition))
            {
                surroundingChunks.Clear();
                // Get direction that chunks need to be generated in
                Vector2 dir = pos - lastPlayerChunkPosition;
                for(int xx = -chunkGridWidth * chunkSize; xx <= chunkGridWidth * chunkSize; xx += chunkSize)
                {
                    for(int yy = -chunkGridHeight * chunkSize; yy <= chunkGridHeight * chunkSize; yy += chunkSize)
                    {
                        Vector2 chunkPos = new Vector2(xx, yy);
                        chunkPos += pos;
                        surroundingChunks.Add(chunkPos);
                    }
                }

                // Check if any outer chunks need recycling
                for(int i = allLoadedChunks.Count - 1; i >= 0; i--)
                {
                    RemoveChunkIfExtraneous(allLoadedChunks[i]);
                }

                // Calculate actual world positions of identified neighbouring chunks
                for(int i = 0; i < surroundingChunks.Count; i++)
                {
                    if(!DoesChunkAlreadyExist(surroundingChunks[i]))
                    {
                        GenerateChunk(surroundingChunks[i]);
                    }
                }
                lastPlayerChunkPosition = pos;
            }
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// Check list of loaded chunks to see if it contains a chunk at the v2 position
    /// </summary>
    bool DoesChunkAlreadyExist (Vector2 pos)
    {
        for(int i = 0; i < allLoadedChunks.Count; i++)
        {
            if(pos.x == allLoadedChunks[i].position.x && pos.y == allLoadedChunks[i].position.y)
            {
                return true;
            }
        }
        return false;
    }

    void RemoveChunkIfExtraneous (Transform chunk)
    {
        // Loop through surrounding chunks to compare
        for(int n = 0; n < surroundingChunks.Count; n++)
        {
            if(surroundingChunks[n].x == chunk.position.x && surroundingChunks[n].y == chunk.position.y)
            {
                // Match made, keep loaded chunk
                return;
            }
        }
        // No matches, recycle the chunk! 
        chunk.GetComponent<ChunkController>().RecycleChunk();
        allLoadedChunks.Remove(chunk);
        UpdateBoundsOfAllLoadedChunkTiles();
    }


    /// <summary>
    /// Refreshes the current bounds of all loaded chunks
    /// </summary>
    void UpdateBoundsOfAllLoadedChunkTiles ()
    {
        lowestChunkTile = allLoadedChunks[0].position;
        highestChunkTile = allLoadedChunks[0].position;
        for(int i = 0; i < allLoadedChunks.Count; i++)
        {
            // Compare current with last chunk and update lowest/highest if necessary
            if(lowestChunkTile.x > allLoadedChunks[i].position.x) lowestChunkTile.x = allLoadedChunks[i].position.x;
            if(lowestChunkTile.y > allLoadedChunks[i].position.y) lowestChunkTile.y = allLoadedChunks[i].position.y;
            if(highestChunkTile.x < allLoadedChunks[i].position.x) highestChunkTile.x = allLoadedChunks[i].position.x;
            if(highestChunkTile.y < allLoadedChunks[i].position.y) highestChunkTile.y = allLoadedChunks[i].position.y;
        }
    }


    /// <summary>
    /// Returns the bottom-left-most position in the chunk of a vector passed to it
    /// </summary>
    Vector2 ResolveVectorToChunk (Vector2 pos)
    {
        float remainderX = pos.x % chunkSize;
        float remainderY = pos.y % chunkSize;
        pos.x = (pos.x - remainderX);
        pos.y = (pos.y - remainderY);
        return pos;
    }

    /// <summary>
    /// Create new chunk GameObject folder with all the required tiles according to the color map
    /// </summary>
    void GenerateChunk (Vector2 chunk)
    {
        GameObject NewChunk = new GameObject();
        NewChunk.transform.parent = transform;
        NewChunk.transform.position = chunk;
        NewChunk.name = "Chunk_" + (int)chunk.x + "," + (int)chunk.y;
        ChunkController newChunkController = NewChunk.AddComponent<ChunkController>();
        allLoadedChunks.Add(NewChunk.transform);

        // Define the start & end points of the chunk
        int chunkXStart = (int)chunk.x;
        int chunkYStart = (int)chunk.y;
        int chunkXEnd = (int)chunk.x + chunkSize;
        int chunkYEnd = (int)chunk.y + chunkSize;

        // Check chunk start & end points are within bounds of map image 
        if(chunkXStart < 0 || chunkYStart < 0 || chunkXEnd > map.width || chunkYEnd > map.height)
        {
            Debug.LogError("Trying to generate chunks outside of map image! You're too close to the edges!");
            return;
        }

        // Keep generation within bounds of chunk
        for(int x = chunkXStart; x < chunkXEnd; x++)
        {
            for(int y = chunkYStart; y < chunkYEnd; y++)
            {
                TryGenerateTile(x, y, NewChunk.transform);
            }
        }

        UpdateBoundsOfAllLoadedChunkTiles();
    }

    void TryGenerateTile (int x, int y, Transform chunkTransform)
    {
        Color pixelColor = map.GetPixel(x, y);
        // Pixel is transparent/empty, do nothing
        // TODO: This won't work when you add multiple levels with lots of empty space
        if(pixelColor.a == 0) return;

        // Check for prefab corresponding to pixel colour 
        foreach(TileCache tp in tileCache)
        {
            if(tp.color == pixelColor)
            {
                // Matched colour with prefab, instatiate the prefab at that position 
                Vector2 pos = new Vector2(x, y);
                Transform t;
                t = Pool.instance.ExtractTileFromPool();
                t.SetParent(chunkTransform);
                t.position = pos;
                t.GetComponent<ThingData>().id = tp.id;
                t.GetComponent<OrderInLayerController>().objectLayerType = tp.objectLayerType;

                // If the TilePrefab [tp] has this listed as a border, than spawn a border on top of it as well
                if(tp.thingType == ThingType.border)
                {
                    Transform b = Pool.instance.ExtractBorderFromPool();
                    b.SetParent(t);
                    b.transform.localPosition = Vector3.zero;
                    b.GetComponent<BorderController>().tileUnderID = tp.id;
                    b.gameObject.SetActive(true);
                }

                t.gameObject.SetActive(true);

                return;
            }
        }
        print("No prefab match for colour found! Colour: " + pixelColor.r * 255 + ", " + pixelColor.g * 255 + ", " + pixelColor.b * 255);
    }
}

[System.Serializable]
public class TileCache
{
    // Used to identify tiles, decorations, props and other objects from a saved json file
    [Tooltip("Leave blank, updated on awake")]
    [HideInInspector] public string id;
    public GameObject prefab;
    // Used to identify tiles from color image
    [HideInInspector] public Color color;
    [HideInInspector] public ThingType thingType;
    [HideInInspector] public LayerType objectLayerType;
    public bool blockMovement;
}