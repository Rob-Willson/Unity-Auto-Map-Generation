using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TilingController : MonoBehaviour
{

    public SpriteRenderer thisSpriteRenderer;
    // Used for cerain tile types which require alpha blending (e.g. water borders)
    // BorderController is responsible for the heavy lifting here, and only if BorderController.alphaBlendBaseTile is set true
    public Texture2D textureToTile;

    [Range(0, 5)] public int tileWidth;
    [Range(0, 5)] public int tileHeight;

    public bool tile;
    public bool terrain;

    ThingData td;

    void OnEnable ()
    {
        td = GetComponent<ThingData>();
        // If this is a terrain object, initialize it, otherwise wait for call from ChunkController
        if(terrain)
        {
            StartCoroutine(InitializeAllTilingControllers());
        }
    }

    IEnumerator InitializeAllTilingControllers ()
    {
        // HACK: I'm not sure why this delay is necessary
        // It seems like the sprite caches take the whole first frame to 
        yield return new WaitForEndOfFrame();
        Initialize();
    }

    public void Initialize ()
    {
        // If sprite renderer not already set, then look for one on this gameObject
        // This is useful for allowing support for offset sprites (e.g. mountain tiles)
        if(thisSpriteRenderer == null) thisSpriteRenderer = GetComponent<SpriteRenderer>();

        // Check that the ID has actually been set
        if(td.id == "" || td.id == null)
        {
            print("id is null or empty on tile: " + gameObject.name + "//  at position: " + transform.position);
            return;
        }

        if(tile)
        {
            thisSpriteRenderer.sprite = TileSpriteCache.instance.GetSpriteFromCache(td.id, transform.position);
            return;
        }
        if(terrain)
        {
            thisSpriteRenderer.sprite = TerrainSpriteCache.instance.GetSpriteFromCache(td.id, transform.position);
            return;
        }
    }

    public void Recycle ()
    {
        thisSpriteRenderer.sprite = null;
        if(tile)
        {
            thisSpriteRenderer.sprite = null;
        }
    }




}
