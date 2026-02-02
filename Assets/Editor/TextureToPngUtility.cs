using UnityEngine;

public class SaveTextureToFileUtility
{
    public enum SaveTextureFileFormat
    {
        EXR, JPG, PNG, TGA
    };

    /// <summary>
    /// Saves a Texture2D to disk with the specified filename and image format
    /// </summary>
    /// <param name="tex"></param>
    /// <param name="filePath"></param>
    /// <param name="fileFormat"></param>
    /// <param name="jpgQuality"></param>
    static public void SaveTexture2DToFile(Texture2D tex, string filePath, SaveTextureFileFormat fileFormat, int jpgQuality = 95)
    {
        if (tex == null)
        {
            Debug.LogError("Texture is null.");
            return;
        }

        // Decompress the texture if it's in a compressed format
        Texture2D decompressedTex = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
        RenderTexture.active = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.Default);
        Graphics.Blit(tex, RenderTexture.active);
        decompressedTex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        decompressedTex.Apply();
        RenderTexture.active = null;

        switch (fileFormat)
        {
            case SaveTextureFileFormat.EXR:
                System.IO.File.WriteAllBytes(filePath + ".exr", decompressedTex.EncodeToEXR());
                break;
            case SaveTextureFileFormat.JPG:
                System.IO.File.WriteAllBytes(filePath + ".jpg", decompressedTex.EncodeToJPG(jpgQuality));
                break;
            case SaveTextureFileFormat.PNG:
                System.IO.File.WriteAllBytes(filePath + ".png", decompressedTex.EncodeToPNG());
                break;
            case SaveTextureFileFormat.TGA:
                System.IO.File.WriteAllBytes(filePath + ".tga", decompressedTex.EncodeToTGA());
                break;
            default:
                Debug.LogError("Unsupported file format.");
                break;
        }

        // Destroy the decompressed texture to avoid memory leaks
        Object.DestroyImmediate(decompressedTex);
    }

    /// <summary>
    /// Saves a RenderTexture to disk with the specified filename and image format
    /// </summary>
    /// <param name="renderTexture"></param>
    /// <param name="filePath"></param>
    /// <param name="fileFormat"></param>
    /// <param name="jpgQuality"></param>
    static public void SaveRenderTextureToFile(RenderTexture renderTexture, string filePath, SaveTextureFileFormat fileFormat = SaveTextureFileFormat.PNG, int jpgQuality = 95)
    {
        Texture2D tex;
        if (fileFormat != SaveTextureFileFormat.EXR)
            tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false, false);
        else
            tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBAFloat, false, true);
        var oldRt = RenderTexture.active;
        RenderTexture.active = renderTexture;
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply();
        RenderTexture.active = oldRt;
        SaveTexture2DToFile(tex, filePath, fileFormat, jpgQuality);
        if (Application.isPlaying)
            Object.Destroy(tex);
        else
            Object.DestroyImmediate(tex);

    }

}