using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

[InitializeOnLoad]
static class ApplyCalibriProjectWide
{
    const string FontPath = "Assets/Fonts/Calibri.ttf";
    const string FontAssetPath = "Assets/Fonts/Calibri SDF.asset";
    const string CompletedKey = "Emotism.ApplyCalibriProjectWide.Completed";
    static int importRetries;

    static ApplyCalibriProjectWide()
    {
        if (!EditorPrefs.GetBool(CompletedKey, false))
            EditorApplication.delayCall += Apply;
    }

    static void Apply()
    {
        if (EditorPrefs.GetBool(CompletedKey, false) || EditorApplication.isCompiling)
            return;

        AssetDatabase.Refresh();
        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        if (sourceFont == null)
        {
            if (++importRetries < 20)
                EditorApplication.delayCall += Apply;
            else
                Debug.LogError("La police Calibri n'a pas pu être importée depuis Assets/Fonts/Calibri.ttf.");
            return;
        }

        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (fontAsset == null)
        {
            fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont, 90, 9,
                GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic);
            if (fontAsset == null)
            {
                Debug.LogError("Impossible de générer l'asset TextMesh Pro de Calibri.");
                return;
            }

            fontAsset.name = "Calibri SDF";
            AssetDatabase.CreateAsset(fontAsset, FontAssetPath);
            foreach (Texture2D atlas in fontAsset.atlasTextures)
                if (atlas != null && !AssetDatabase.Contains(atlas))
                    AssetDatabase.AddObjectToAsset(atlas, fontAsset);
            if (fontAsset.material != null && !AssetDatabase.Contains(fontAsset.material))
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(FontAssetPath);
            fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        }

        if (fontAsset == null || fontAsset.material == null)
        {
            Debug.LogError("L'asset Calibri SDF ou son matériau est introuvable.");
            return;
        }

        AppliquerAuxPrefabs(fontAsset);
        AppliquerAuxScenes(fontAsset);
        AppliquerCommePoliceParDefaut(fontAsset);

        AssetDatabase.SaveAssets();
        EditorPrefs.SetBool(CompletedKey, true);
        Debug.Log("Calibri a été appliquée aux textes du projet sans modifier leur mise en page.");
    }

    static void AppliquerAuxPrefabs(TMP_FontAsset fontAsset)
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (EstExemple(path)) continue;

            GameObject racine = PrefabUtility.LoadPrefabContents(path);
            bool modifie = AppliquerAuxObjets(racine, fontAsset);
            if (modifie) PrefabUtility.SaveAsPrefabAsset(racine, path);
            PrefabUtility.UnloadPrefabContents(racine);
        }
    }

    static void AppliquerAuxScenes(TMP_FontAsset fontAsset)
    {
        string sceneActivePath = SceneManager.GetActiveScene().path;
        string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (EstExemple(path)) continue;

            Scene scene = SceneManager.GetSceneByPath(path);
            bool dejaChargee = scene.IsValid() && scene.isLoaded;
            if (!dejaChargee)
                scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            bool modifie = false;
            foreach (GameObject racine in scene.GetRootGameObjects())
                modifie |= AppliquerAuxObjets(racine, fontAsset);

            if (modifie) EditorSceneManager.SaveScene(scene);
            if (!dejaChargee) EditorSceneManager.CloseScene(scene, true);
        }

        Scene active = SceneManager.GetSceneByPath(sceneActivePath);
        if (active.IsValid() && active.isLoaded)
            SceneManager.SetActiveScene(active);
    }

    static bool AppliquerAuxObjets(GameObject racine, TMP_FontAsset fontAsset)
    {
        bool modifie = false;
        foreach (TMP_Text texte in racine.GetComponentsInChildren<TMP_Text>(true))
        {
            if (texte.font != fontAsset || texte.fontSharedMaterial != fontAsset.material)
            {
                texte.font = fontAsset;
                texte.fontSharedMaterial = fontAsset.material;
                EditorUtility.SetDirty(texte);
                modifie = true;
            }
        }

        Font policeLegacy = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        foreach (Text texte in racine.GetComponentsInChildren<Text>(true))
        {
            if (texte.font == policeLegacy) continue;
            texte.font = policeLegacy;
            EditorUtility.SetDirty(texte);
            modifie = true;
        }
        return modifie;
    }

    static void AppliquerCommePoliceParDefaut(TMP_FontAsset fontAsset)
    {
        TMP_Settings settings = Resources.Load<TMP_Settings>("TMP Settings");
        if (settings == null) return;
        TMP_Settings.defaultFontAsset = fontAsset;
        EditorUtility.SetDirty(settings);
    }

    static bool EstExemple(string path)
    {
        return path.IndexOf("TextMesh Pro/Examples & Extras/", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
