using System.Collections.Generic;
using System.Linq;
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
    
    
    [MenuItem("3DTextures/Noise Test")]
    static void Create3DTexture_Noise_Test()
    {
        // Settings
        int size = 64;
        TextureFormat format = TextureFormat.RGBA32;
        TextureWrapMode wrapMode = TextureWrapMode.Clamp;

        Texture3D tex = new Texture3D(size, size, size, format, false);
        tex.wrapMode = wrapMode;

        //Create a 3 - dimensional array to store color data
        Color[] colors = new Color[size * size * size];
        
        //
        Vector3 origin = Vector3.zero;

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
                    //colors[x + yOffset + zOffset] = new Color(x * inverseResolution, y * inverseResolution, z * inverseResolution, 1.0f);
                    //Vector2 sample = new Vector2((float)x / size, (float)y / size);

                    //float val = Mathf.PerlinNoise(sample.x, sample.y);

                    float val = Vector3.Distance(new Vector3(x,y,z), origin);
                    float invLerp = Mathf.InverseLerp(0, Vector3.Distance(new Vector3(size, size, size), origin), val);
                    val = Mathf.Lerp(0, 1, invLerp);
                    
                    colors[x + yOffset + zOffset] = new Color(val, val, val, 1.0f);
                }
            }
        }

        // Copy the color values to the texture
        tex.SetPixels(colors);

        // Apply the changes to the texture and upload the updated texture to the GPU
        tex.Apply();

        AssetDatabase.CreateAsset(tex, "Assets/Test3DTex_Noise.asset");
    }
    
    [MenuItem("3DTextures/Worley")]
    static void Create3DTexture_Worley()
    {
        // Texture setup
        int size = 64;
        TextureFormat format = TextureFormat.RGBA32;
        TextureWrapMode wrapMode = TextureWrapMode.Clamp;

        Texture3D tex = new Texture3D(size, size, size, format, false);
        tex.wrapMode = wrapMode;

        Color[] colors = new Color[size * size * size];

        float maxDist = size * .5f;
        
        // Worley Setup
        int subdivs = 2;
        List<Vector3> points = new List<Vector3>();
        points.Capacity = subdivs * subdivs * subdivs;
        float sectionSize = (float)size / subdivs;            
        
        for (int x = 0; x < subdivs; x++)
        {
            for (int y = 0; y < subdivs; y++)
            {
                for (int z = 0; z < subdivs; z++)
                {
                    Vector3 origin = Vector3.zero;
                    
                    origin.x = Mathf.Lerp(x * sectionSize, (x + 1) * sectionSize, Random.Range(0f, 1f));
                    origin.y = Mathf.Lerp(y * sectionSize, (y + 1) * sectionSize, Random.Range(0f, 1f));
                    origin.z = Mathf.Lerp(z * sectionSize, (z + 1) * sectionSize, Random.Range(0f, 1f));
                    
                    points.Add(origin);
                }
            }
        }
        
        foreach (var point in points)
            Debug.Log(point.ToString());
        
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
                    List<float> distVals = new List<float>();

                    foreach (var point in points)
                        distVals.Add(Vector3.Distance(new Vector3(x,y,z), point));

                    float dist = distVals.Min(dist => dist);
                    float invLerp = Mathf.InverseLerp(0, maxDist, dist);
                    float val = Mathf.Lerp(0, 1, invLerp);
                    val = 1 - val;
                    
                    colors[x + yOffset + zOffset] = new Color(val, val, val, 1.0f);
                }
            }
        }

        tex.SetPixels(colors);

        tex.Apply();

        AssetDatabase.CreateAsset(tex, "Assets/Worley_Test.asset");
    }
    
    
    [MenuItem("3DTextures/2D Worley")]
    static void Create3DTexture_Worley_2D()
    {
        // Texture setup
        int size = 64;
        TextureFormat format = TextureFormat.RGBA32;
        TextureWrapMode wrapMode = TextureWrapMode.Clamp;

        Texture2D tex = new Texture2D(size * size, size, format, false);
        tex.wrapMode = wrapMode;

        Color[] colors = new Color[size * size * size];

        float maxDist = size * .5f;
        
        // Worley Setup
        int subdivs = 2;
        List<Vector3> points = new List<Vector3>();
        points.Capacity = subdivs * subdivs * subdivs;
        float sectionSize = (float)size / subdivs;            
        
        for (int x = 0; x < subdivs; x++)
        {
            for (int y = 0; y < subdivs; y++)
            {
                for (int z = 0; z < subdivs; z++)
                {
                    Vector3 origin = Vector3.zero;
                    
                    origin.x = Mathf.Lerp(x * sectionSize, (x + 1) * sectionSize, Random.Range(0f, 1f));
                    origin.y = Mathf.Lerp(y * sectionSize, (y + 1) * sectionSize, Random.Range(0f, 1f));
                    origin.z = Mathf.Lerp(z * sectionSize, (z + 1) * sectionSize, Random.Range(0f, 1f));
                    
                    points.Add(origin);
                }
            }
        }
        
        foreach (var point in points)
            Debug.Log(point.ToString());
        
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
                    List<float> distVals = new List<float>();

                    foreach (var point in points)
                        distVals.Add(Vector3.Distance(new Vector3(x,y,z), point));

                    float dist = distVals.Min(dist => dist);
                    float invLerp = Mathf.InverseLerp(0, maxDist, dist);
                    float val = Mathf.Lerp(0, 1, invLerp);
                    val = 1 - val;
                    
                    colors[x + yOffset + zOffset] = new Color(val, val, val, 1.0f);
                }
            }
        }

        tex.SetPixels(colors);

        tex.Apply();

        AssetDatabase.CreateAsset(tex, "Assets/Worley_2D_Test.asset");
    }
}
