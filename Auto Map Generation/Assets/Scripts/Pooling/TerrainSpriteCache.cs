using UnityEngine;

public class TerrainSpriteCache : SpriteCache
{
    public static TerrainSpriteCache instance = null;

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

}
