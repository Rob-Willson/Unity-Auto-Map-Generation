using System.Collections.Generic;
using UnityEngine;

public class BorderGeneratorOverlord : MonoBehaviour
{
    public static BorderGeneratorOverlord instance = null;
    public List<TilePair> borderTilePairs = new List<TilePair>();

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

    // Returns the index in the TilePair list of the base tile's id (if it exists)
    int? GetBaseTilePairIndexFromID (string baseTileID)
    {
        int? index = null;
        // First get the index of the base tile in the borderTilePairs list
        for(int i = 0; i < borderTilePairs.Count; i++)
        {
            if(baseTileID == borderTilePairs[i].baseTileID)
            {
                index = i;
                return index;
            }
        }
        print("Could not find tile of id " + baseTileID + " in overlord TilePair list");
        return index;
    }


    public Sprite CalculateBorderRequirements (string[] adjacentTileIDs, BorderController borderControllerUnderling)
    {
        // First, get the index of the base tile in the borderTilePairs list
        int? indexNullable = GetBaseTilePairIndexFromID(borderControllerUnderling.tileUnderID);
        int index;
        if(indexNullable == null)
        {
            // No match found for id, go no further
            return null;
        }
        else
        {
            index = (int)indexNullable;
        }

        // Second, nullify any invalid entries from list
        for(int i = 0; i < adjacentTileIDs.Length; i++)
        {
            if(adjacentTileIDs[i] != null)
            {
                for(int n = 0; n < borderTilePairs[index].tileToLookForID.Length; n++)
                {
                    if(adjacentTileIDs[i] == borderTilePairs[index].tileToLookForID[n])
                    {
                        // adjacent tile is in list of tiles to look for, break out of loop
                        break;
                    }
                    else if(n == borderTilePairs[index].tileToLookForID.Length - 1)
                    {
                        // Only clear id if this is the last id to look for
                        adjacentTileIDs[i] = "";
                    }

                }
            }
        }

        // Third, for any valid entries, set border to be required
        bool[] isBorderRequired = new bool[8];

        for(int i = 0; i < adjacentTileIDs.Length; i++)
        {
            if(adjacentTileIDs[i] != "")
            {
                isBorderRequired[i] = true;
            }
            else
            {
                isBorderRequired[i] = false;
            }
        }

        Sprite sprite = null;

        int alphaBlendIndex = 0;

        if(isBorderRequired[0])
        {
            if(isBorderRequired[2])
            {
                // Top-right
                sprite = borderTilePairs[index].borderCornerSprites[0];
                alphaBlendIndex = 6;
            }
            else if(isBorderRequired[6])
            {
                // Top-left
                sprite = borderTilePairs[index].borderCornerSprites[3];
                alphaBlendIndex = 9;
            }
            else
            {
                // Top
                sprite = borderTilePairs[index].borderEdgeSprites[0];
                alphaBlendIndex = 2;
            }
        }
        else if(isBorderRequired[4])
        {
            if(isBorderRequired[2])
            {
                // Bottom-right
                sprite = borderTilePairs[index].borderCornerSprites[1];
                alphaBlendIndex = 7;
            }
            else if(isBorderRequired[6])
            {
                // Bottom-left
                sprite = borderTilePairs[index].borderCornerSprites[2];
                alphaBlendIndex = 8;
            }
            else
            {
                // Bottom
                sprite = borderTilePairs[index].borderEdgeSprites[2];
                alphaBlendIndex = 4;
            }
            //return;
        }
        else if(isBorderRequired[2])
        {
            // Right
            sprite = borderTilePairs[index].borderEdgeSprites[1];
            alphaBlendIndex = 3;
        }
        else if(isBorderRequired[6])
        {
            // Left
            sprite = borderTilePairs[index].borderEdgeSprites[3];
            alphaBlendIndex = 5;
        }
        // External corners
        else if(isBorderRequired[1])
        {
            if(borderTilePairs[index].borderExternalCornerSprites.Length > 0) sprite = borderTilePairs[index].borderExternalCornerSprites[0];
            alphaBlendIndex = 10;
        }
        else if(isBorderRequired[3])
        {
            if(borderTilePairs[index].borderExternalCornerSprites.Length > 0) sprite = borderTilePairs[index].borderExternalCornerSprites[1];
            alphaBlendIndex = 11;
        }
        else if(isBorderRequired[5])
        {
            if(borderTilePairs[index].borderExternalCornerSprites.Length > 0) sprite = borderTilePairs[index].borderExternalCornerSprites[2];
            alphaBlendIndex = 12;
        }
        else if(isBorderRequired[7])
        {
            if(borderTilePairs[index].borderExternalCornerSprites.Length > 0) sprite = borderTilePairs[index].borderExternalCornerSprites[3];
            alphaBlendIndex = 13;
        }

        AlphaBlend(borderControllerUnderling, alphaBlendIndex);

        return sprite;
    }

    void AlphaBlend (BorderController borderControllerUnderling, int alphaBlendIndex)
    {
        // Find index of tile in SpriteCache
        int? indx = TileSpriteCache.instance.GetIndexOfIDInSpriteCache(borderControllerUnderling.tileUnderID);
        if(indx != null)
        {
            int indexInSpriteCache = (int)indx;
            // If border required alpha blending (according to whether a texture for blending exists on SpriteCache), then do it!
            if(TileSpriteCache.instance.spriteCache[indexInSpriteCache].texture != null)
            {
                Texture2D texture = TileSpriteCache.instance.spriteCache[indexInSpriteCache].texture;
                // Calc position of this 32px tile within the whole tile graphic
                Vector2 tileReferenceInGroup = TileSpriteCache.instance.CalculateTileReferenceInGroup(borderControllerUnderling.transform.position, borderControllerUnderling.tileUnderID);
                int x = (int)tileReferenceInGroup.x * 32;
                int y = (int)tileReferenceInGroup.y * 32;

                Sprite spr = SpriteAdjustment.instance.AlphaBlendToSprite(texture, alphaBlendIndex, x, y);
                borderControllerUnderling.transform.parent.GetComponent<SpriteRenderer>().sprite = spr;
            }
        }
    }
}

[System.Serializable]
public class TilePair
{
    public string baseTileID;
    public string[] tileToLookForID;
    [Tooltip("The straight border sprites")]
    public Sprite[] borderEdgeSprites;
    [Tooltip("The corner border sprites")]
    public Sprite[] borderCornerSprites;
    [Tooltip("The external corner border sprites")]
    public Sprite[] borderExternalCornerSprites;
}
