using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;

// Editor tool to modify all texture sizes in the project
// Including all the sprite sheets. Making it easy to turn a whole 16x16 resolution game into a 32x32 game or higher.
// DO NOTE: Please backup your files before executing these commands, it is a destructive action.

// TextureScale class is based off https://wiki.unity3d.com/index.php/TextureScale by Author: Eric Haines (Eric5h5)
// And converted to C# by another user. Everything except for the TextureScale class is made by me. (Alex Meesters)
// Licence https://creativecommons.org/licenses/by-sa/3.0/

public class ResizeTextures : MonoBehaviour
{
    [MenuItem("Resize", menuItem = "Tools/Resize Textures/Selected Texture(s) 2X (Backup before doing this)")]
    private static void ResizeSelection()
    {
        foreach (Object obj in Selection.objects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);

            ResizeTexture(assetPath);
        }
    }

    [MenuItem("Resize", menuItem = "Tools/Resize Textures/All Texture(s) 2X (Backup before doing this)")]
    private static void ResizeAll()
    {
        Debug.Log("Started resizing of all textures in the project. This may take a moment");

        string[] assetGuids = AssetDatabase.FindAssets("t:texture2d");

        foreach (string guid in assetGuids)
        {
            ResizeTexture(AssetDatabase.GUIDToAssetPath(guid));
        }
    }

    private static void ResizeTexture(string assetPath)
    {
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

        if (tex == null)
            return;

        // Make readable and resize the sprites contained within the asset
        TextureImporter textureImporter = TextureImporter.GetAtPath(assetPath) as TextureImporter;

        if (textureImporter == null)
            return;

        textureImporter.isReadable = true;
        textureImporter.spritePixelsPerUnit *= 2;

        SpriteMetaData[] metaData = textureImporter.spritesheet;
        int spriteCount = metaData.Length;

        for (int i2 = 0; i2 < spriteCount; i2++)
        {
            Rect rect = metaData[i2].rect;
            rect.width *= 2;
            rect.height *= 2;
            rect.position *= 2;
            metaData[i2].rect = rect;
        }

        textureImporter.spritesheet = metaData;
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();


        // Resize actual texture bytes
        int xSize = tex.width * 2;
        int ySize = tex.height * 2;

        Resize(tex, xSize, ySize, assetPath);
    }

    static void SetTextureImporterFormat(Texture2D texture, bool isReadable, int scale)
    {
        if (null == texture) return;

        string assetPath = AssetDatabase.GetAssetPath(texture);
        var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (tImporter != null)
        {
            tImporter.textureType = TextureImporterType.Sprite;
            tImporter.filterMode = FilterMode.Point;
            tImporter.isReadable = isReadable;
            tImporter.spritePixelsPerUnit = scale * 100;
            tImporter.textureCompression = TextureImporterCompression.Uncompressed;

            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();
        }
    }

    static void Resize(Texture2D source, int newWidth, int newHeight, string path)
    {
        Debug.Log(path.Replace(Application.dataPath, "Assets"));
        TextureScale.Point(source, newWidth, newHeight);
        byte[] bytes = source.EncodeToPNG();
        File.WriteAllBytes(path.Replace(Application.dataPath, "Assets"), bytes);
        AssetDatabase.Refresh();
    }
}

public class TextureScale
{
    public class ThreadData
    {
        public int start;
        public int end;
        public ThreadData(int s, int e)
        {
            start = s;
            end = e;
        }
    }

    private static Color[] texColors;
    private static Color[] newColors;
    private static int w;
    private static float ratioX;
    private static float ratioY;
    private static int w2;
    private static int finishCount;
    private static Mutex mutex;

    public static void Point(Texture2D tex, int newWidth, int newHeight)
    {
        ThreadedScale(tex, newWidth, newHeight, false);
    }

    private static void ThreadedScale(Texture2D tex, int newWidth, int newHeight, bool useBilinear)
    {
        texColors = tex.GetPixels();
        newColors = new Color[newWidth * newHeight];

        ratioX = ((float)tex.width) / newWidth;
        ratioY = ((float)tex.height) / newHeight;

        w = tex.width;
        w2 = newWidth;
        var cores = Mathf.Min(SystemInfo.processorCount, newHeight);
        var slice = newHeight / cores;

        finishCount = 0;
        if (mutex == null)
        {
            mutex = new Mutex(false);
        }
        if (cores > 1)
        {
            int i = 0;
            ThreadData threadData;
            for (i = 0; i < cores - 1; i++)
            {
                threadData = new ThreadData(slice * i, slice * (i + 1));
                ParameterizedThreadStart ts = new ParameterizedThreadStart(PointScale);
                Thread thread = new Thread(ts);
                thread.Start(threadData);
            }
            threadData = new ThreadData(slice * i, newHeight);

            PointScale(threadData);

            while (finishCount < cores)
            {
                Thread.Sleep(1);
            }
        }
        else
        {
            ThreadData threadData = new ThreadData(0, newHeight);

            PointScale(threadData);
        }

        tex.Resize(newWidth, newHeight);
        tex.SetPixels(newColors);
        tex.Apply();

        texColors = null;
        newColors = null;
    }

    public static void PointScale(System.Object obj)
    {
        ThreadData threadData = (ThreadData)obj;
        for (var y = threadData.start; y < threadData.end; y++)
        {
            var thisY = (int)(ratioY * y) * w;
            var yw = y * w2;
            for (var x = 0; x < w2; x++)
            {
                newColors[yw + x] = texColors[(int)(thisY + ratioX * x)];
            }
        }

        mutex.WaitOne();
        finishCount++;
        mutex.ReleaseMutex();
    }

    private static Color ColorLerpUnclamped(Color c1, Color c2, float value)
    {
        return new Color(c1.r + (c2.r - c1.r) * value,
                          c1.g + (c2.g - c1.g) * value,
                          c1.b + (c2.b - c1.b) * value,
                          c1.a + (c2.a - c1.a) * value);
    }
}
