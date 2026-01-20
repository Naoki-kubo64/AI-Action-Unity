using UnityEngine;
using UnityEditor;

public class BackgroundSetup : MonoBehaviour
{
    [MenuItem("Tools/Setup Background")]
    public static void Setup()
    {
        string path = "Assets/SunnyLand Artwork/Environment/back.png";
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

        if (bgSprite == null)
        {
            Debug.LogError($"Could not load sprite at {path}");
            return;
        }

        GameObject bgObj = GameObject.Find("Background");
        if (bgObj == null)
        {
            bgObj = new GameObject("Background");
        }

        SpriteRenderer sr = bgObj.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = bgObj.AddComponent<SpriteRenderer>();
        }

        sr.sprite = bgSprite;
        sr.sortingOrder = -100; // Far behind
        
        // Optional: Scale to cover a standard area
        bgObj.transform.position = new Vector3(10, 0, 0); // Centerish for level 1
        bgObj.transform.localScale = new Vector3(20, 10, 1); 

        Debug.Log("Background setup complete!");
    }
}
