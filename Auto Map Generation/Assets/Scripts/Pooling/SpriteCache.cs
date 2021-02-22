using System.Collections.Generic;
using UnityEngine;

public class SpriteCache : MonoBehaviour
{
    public List<SpriteWithID> spriteCache = new List<SpriteWithID>();

    public int? GetIndexOfIDInSpriteCache (string id)
    {
        for(int i = 0; i < spriteCache.Count; i++)
        {
            if(id == spriteCache[i].id)
            {
                return i;
            }
        }
        return null;
    }

    public Sprite GetSpriteFromCache (string id, Vector2 pos)
    {
        int? idxNullable = GetIndexOfIDInSpriteCache(id);
        if(idxNullable == null)
        {
            print("Couldn't find id: " + id + " in sprite cache.");
            return null;
        }
        int idx = (int)idxNullable;

        // Calculate the position of the required tile in terms of the full-sized sprite (which is probably > 32x32)
        Vector2 tileReferenceInGroup = CalculateTileReferenceInGroup(pos, spriteCache[idx].tileWidth, (spriteCache[idx].tileHeight));
        int chosenSpriteIndex = DetermineSpriteIndex(spriteCache[idx].tileWidth, spriteCache[idx].tileHeight, tileReferenceInGroup, idx);
        Sprite spr = spriteCache[idx].sprites[chosenSpriteIndex];
        return spr;
    }


    int DetermineSpriteIndex (int tileWidth, int tileHeight, Vector2 tileReferenceInGroup, int idx)
    {
        int groupCount = tileWidth * tileHeight;
        tileWidth -= 1;
        tileHeight -= 1;

        int i = groupCount - 1;
        if(spriteCache[idx].sprites.Length >= groupCount)
        {
            Vector2 v;
            for(int iy = 0; iy <= tileHeight; iy++)
            {
                for(int ix = tileWidth; ix >= 0; ix--)
                {
                    v.x = ix;
                    v.y = iy;
                    if(v == tileReferenceInGroup)
                    {
                        return i;
                    }
                    i--;
                }
            }
        }

        Debug.LogError("Requested tiling parameters do not match spritesToTile length! Add/remove sprites to spritesToTile or change the tiling parameters. Using random seed for tiling!");
        return 0;
    }


    public Vector2 CalculateTileReferenceInGroup (Vector2 pos, string id)
    {
        int? indx = GetIndexOfIDInSpriteCache(id);
        // TODO/HACK: This isn't exactly robust, it just let's you know when something isn't right...
        if(indx == null)
        {
            print("id " + id + " not found in Sprite Cache");
        }
        int index = (int)indx;
        Vector2 v = CalculateTileReferenceInGroup(pos, spriteCache[index].tileWidth, spriteCache[index].tileHeight);
        return v;
    }


    public Vector2 CalculateTileReferenceInGroup (Vector2 pos, int tileWidth, int tileHeight)
    {
        Vector2 tileGroupReference = pos;

        // Convert the tile's position to a grid reference
        for(int i = 0; (tileGroupReference.x % tileWidth) > 0; i++)
        {
            tileGroupReference.x += 1;
        }
        for(int i = 0; (tileGroupReference.y % tileHeight) > 0; i++)
        {
            tileGroupReference.y += 1;
        }
        tileGroupReference.x = (tileGroupReference.x / tileWidth) - 1;
        tileGroupReference.y = (tileGroupReference.y / tileHeight) - 1;

        // Get tile reference within group (the grid reference of this specific tile within the above array of tiles)  
        Vector2 tileReferenceInGroup;
        tileReferenceInGroup.x = pos.x - (tileGroupReference.x * tileWidth) - 1;
        tileReferenceInGroup.y = pos.y - (tileGroupReference.y * tileHeight) - 1;
        return tileReferenceInGroup;
    }
}


[System.Serializable]
public class SpriteWithID
{
    public string id;
    public Sprite[] sprites;
    [Tooltip("Used for blending a tile with an alpha")] public Texture2D texture;
    [Tooltip("How many tiles wide is the pattern?")] [Range(0, 5)] public int tileWidth;
    [Tooltip("How many tiles high is the pattern?")] [Range(0, 5)] public int tileHeight;
}
