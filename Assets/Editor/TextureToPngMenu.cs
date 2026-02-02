using UnityEngine;
using UnityEditor;

public class ConvertSelectedTexture2DToPNG : EditorWindow
{
    private Texture2D selectedTexture;

    [MenuItem("Custom/Convert Selected Texture2D to PNG")]
    static void Init()
    {
        ConvertSelectedTexture2DToPNG window = (ConvertSelectedTexture2DToPNG)EditorWindow.GetWindow(typeof(ConvertSelectedTexture2DToPNG));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Convert Selected Texture2D to PNG", EditorStyles.boldLabel);

        selectedTexture = (Texture2D)EditorGUILayout.ObjectField("Selected Texture", selectedTexture, typeof(Texture2D), false);

        if (selectedTexture == null)
        {
            EditorGUILayout.HelpBox("Please select a Texture2D to convert.", MessageType.Info);
            return;
        }

        if (GUILayout.Button("Convert"))
        {
            string assetPath = AssetDatabase.GetAssetPath(selectedTexture);

            // Ensure the texture has the .asset extension
            if (assetPath.EndsWith(".asset"))
            {
                // Specify the file path where you want to save the PNG
                string pngFilePath = "Assets/ConvertedTextures/" + selectedTexture.name + "_tex";

                // Save the texture as PNG
                SaveTextureToFileUtility.SaveTexture2DToFile(selectedTexture, pngFilePath, SaveTextureToFileUtility.SaveTextureFileFormat.PNG);
                Debug.Log("Texture converted to PNG: " + pngFilePath);
            }
            else
            {
                Debug.LogWarning("Selected asset is not a Texture2D with .asset extension.");
            }
        }
    }
}
