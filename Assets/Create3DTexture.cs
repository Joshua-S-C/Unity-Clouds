using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class Create3DTexture : MonoBehaviour
{
    [MenuItem("3DTextures/White Texture")]
    static void Create3DTexture_White()
    {
        // Settings
        int size = 64;
        TextureFormat format = TextureFormat.RGBA32;
        TextureWrapMode wrapMode = TextureWrapMode.Clamp;

        Texture3D tex = new Texture3D(size, size, size, format, false);
        tex.wrapMode = wrapMode;

        //Create a 3 - dimensional array to store color data
        Color[] colors = new Color[size * size * size];


        // Populate the array so that the x, y, and z values of the texture map to red, blue, and green colors
        float inverseResolution = 1.0f / (size - 1.0f);
        for (int z = 0; z < size; z++)
        {
            int zOffset = z * size * size;
            for (int y = 0; y < size; y++)
            {
                int yOffset = y * size;
                for (int x = 0; x < size; x++)
                {
                    colors[x + yOffset + zOffset] = new Color(1, 1, 1, 1.0f);
                }
            }
        }

        // Copy the color values to the texture
        tex.SetPixels(colors);

        // Apply the changes to the texture and upload the updated texture to the GPU
        tex.Apply();

        AssetDatabase.CreateAsset(tex, "Assets/Test3DTex_White.asset");
    }
    
    [MenuItem("3DTextures/Colour Texture")]
    static void Create3DTexture_Colour()
    {
        // Settings
        int size = 64;
        TextureFormat format = TextureFormat.RGBA32;
        TextureWrapMode wrapMode = TextureWrapMode.Clamp;

        Texture3D tex = new Texture3D(size, size, size, format, false);
        tex.wrapMode = wrapMode;

        //Create a 3 - dimensional array to store color data
        Color[] colors = new Color[size * size * size];

        // Populate the array so that the x, y, and z values of the texture map to red, blue, and green colors
        float inverseResolution = 1.0f / (size - 1.0f);
        for (int z = 0; z < size; z++)
        {
            int zOffset = z * size * size;
            for (int y = 0; y < size; y++)
            {
                int yOffset = y * size;
                for (int x = 0; x < size; x++)
                {
                    colors[x + yOffset + zOffset] = new Color(x * inverseResolution, y * inverseResolution, z * inverseResolution, 1.0f);
                }
            }
        }

        // Copy the color values to the texture
        tex.SetPixels(colors);

        // Apply the changes to the texture and upload the updated texture to the GPU
        tex.Apply();

        AssetDatabase.CreateAsset(tex, "Assets/Test3DTex_Colour.asset");
    }
}
