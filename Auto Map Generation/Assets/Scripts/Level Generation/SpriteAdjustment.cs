using UnityEngine;

public class SpriteAdjustment : MonoBehaviour
{
    public static SpriteAdjustment instance = null;
    // Place linear alphas in here (e.g. for creating transparent water edges)
    public Texture2D[] blendAlphas;

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

    public Sprite AlphaBlendToSprite (Texture2D originalTexture, int alphaTextureIndex, int x, int y)
    {
        // Convert textures to data and count pixels in alpha texture
        var alphaData = blendAlphas[alphaTextureIndex].GetPixels();
        int count = alphaData.Length;
        // Only get 32x32 pixel sample of original texture, starting at the defined position
        var combinedData = originalTexture.GetPixels(x, y, 32, 32);
        for(int i = 0; i < count; i++)
        {
            combinedData[i].a = alphaData[i].r;
        }
        var result = new Texture2D(32, 32);
        result.SetPixels(combinedData);
        result.Apply();
        Sprite combinedSprite = Sprite.Create(result, new Rect(0f, 0f, result.width, result.height), new Vector2(0.5f, 0.5f), 32f);
        return combinedSprite;
    }

}
