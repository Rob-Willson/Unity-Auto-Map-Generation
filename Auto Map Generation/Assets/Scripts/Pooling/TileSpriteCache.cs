using UnityEngine;

public class TileSpriteCache : SpriteCache
{
    public static TileSpriteCache instance = null;

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
