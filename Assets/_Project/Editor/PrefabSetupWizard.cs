#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace QLDMathApp.Editor
{
    /// <summary>
    /// EDITOR UTILITY: Creates prefabs for the math app.
    /// Menu: Tools > QLD Math App > Create Prefabs
    /// </summary>
    public static class PrefabSetupWizard
    {
        private const string PREFAB_PATH = "Assets/_Project/Prefabs/";
        
        [MenuItem("Tools/QLD Math App/Create All Prefabs")]
        public static void CreateAllPrefabs()
        {
            EnsureFolderExists();
            CreateFireflyPrefab();
            CreateAnswerButtonPrefab();
            CreatePatternPiecePrefab();
            CreateFoodItemPrefab();
            CreateMapNodePrefab();
            Debug.Log("[PrefabWizard] All prefabs created in " + PREFAB_PATH);
        }

        [MenuItem("Tools/QLD Math App/Create Firefly Prefab")]
        public static void CreateFireflyPrefab()
        {
            EnsureFolderExists();
            
            GameObject firefly = new GameObject("Firefly");
            
            // Add sprite renderer with glow
            SpriteRenderer sr = firefly.AddComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.95f, 0.4f); // Yellow glow
            
            // Add animator component
            firefly.AddComponent<QLDMathApp.Modules.Subitising.FireflyAnimator>();
            
            // Save as prefab
            string path = PREFAB_PATH + "Firefly.prefab";
            PrefabUtility.SaveAsPrefabAsset(firefly, path);
            Object.DestroyImmediate(firefly);
            
            Debug.Log("[PrefabWizard] Created: " + path);
        }

        [MenuItem("Tools/QLD Math App/Create Answer Button Prefab")]
        public static void CreateAnswerButtonPrefab()
        {
            EnsureFolderExists();
            
            // Create button with Neo-Skeuomorphic styling
            GameObject buttonGO = new GameObject("AnswerButton");
            RectTransform rt = buttonGO.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120, 120); // Large touch target
            
            // Background image
            Image bg = buttonGO.AddComponent<Image>();
            bg.color = new Color(0.95f, 0.85f, 0.75f); // Cream/clay
            
            // Shadow (child)
            GameObject shadowGO = new GameObject("Shadow");
            shadowGO.transform.SetParent(buttonGO.transform);
            RectTransform shadowRT = shadowGO.AddComponent<RectTransform>();
            shadowRT.anchorMin = Vector2.zero;
            shadowRT.anchorMax = Vector2.one;
            shadowRT.offsetMin = new Vector2(4, -8);
            shadowRT.offsetMax = new Vector2(8, -4);
            Image shadowImg = shadowGO.AddComponent<Image>();
            shadowImg.color = new Color(0.6f, 0.5f, 0.4f, 0.5f);
            shadowGO.transform.SetAsFirstSibling();
            
            // Number text (child)
            GameObject textGO = new GameObject("NumberText");
            textGO.transform.SetParent(buttonGO.transform);
            RectTransform textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;
            Text txt = textGO.AddComponent<Text>();
            txt.text = "5";
            txt.fontSize = 48;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = new Color(0.25f, 0.2f, 0.15f);
            
            // Button component
            Button btn = buttonGO.AddComponent<Button>();
            btn.targetGraphic = bg;
            
            // Answer button script
            buttonGO.AddComponent<QLDMathApp.Modules.Subitising.AnswerButton>();
            
            // Save
            string path = PREFAB_PATH + "AnswerButton.prefab";
            PrefabUtility.SaveAsPrefabAsset(buttonGO, path);
            Object.DestroyImmediate(buttonGO);
            
            Debug.Log("[PrefabWizard] Created: " + path);
        }

        [MenuItem("Tools/QLD Math App/Create Pattern Piece Prefab")]
        public static void CreatePatternPiecePrefab()
        {
            EnsureFolderExists();
            
            GameObject pieceGO = new GameObject("PatternPiece");
            RectTransform rt = pieceGO.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80, 80);
            
            // Background
            Image bg = pieceGO.AddComponent<Image>();
            bg.color = Color.white;
            
            // Shape image (child)
            GameObject shapeGO = new GameObject("ShapeImage");
            shapeGO.transform.SetParent(pieceGO.transform);
            RectTransform shapeRT = shapeGO.AddComponent<RectTransform>();
            shapeRT.anchorMin = new Vector2(0.1f, 0.1f);
            shapeRT.anchorMax = new Vector2(0.9f, 0.9f);
            shapeRT.offsetMin = Vector2.zero;
            shapeRT.offsetMax = Vector2.zero;
            Image shapeImg = shapeGO.AddComponent<Image>();
            shapeImg.color = Color.red;
            
            // Mystery icon (child, hidden by default)
            GameObject mysteryGO = new GameObject("MysteryIcon");
            mysteryGO.transform.SetParent(pieceGO.transform);
            RectTransform mysteryRT = mysteryGO.AddComponent<RectTransform>();
            mysteryRT.anchorMin = Vector2.zero;
            mysteryRT.anchorMax = Vector2.one;
            mysteryRT.offsetMin = Vector2.zero;
            mysteryRT.offsetMax = Vector2.zero;
            Text mysteryTxt = mysteryGO.AddComponent<Text>();
            mysteryTxt.text = "?";
            mysteryTxt.fontSize = 48;
            mysteryTxt.alignment = TextAnchor.MiddleCenter;
            mysteryGO.SetActive(false);
            
            // Button
            pieceGO.AddComponent<Button>();
            pieceGO.AddComponent<QLDMathApp.Modules.Patterns.PatternPiece>();
            
            // Save
            string path = PREFAB_PATH + "PatternPiece.prefab";
            PrefabUtility.SaveAsPrefabAsset(pieceGO, path);
            Object.DestroyImmediate(pieceGO);
            
            Debug.Log("[PrefabWizard] Created: " + path);
        }

        [MenuItem("Tools/QLD Math App/Create Food Item Prefab")]
        public static void CreateFoodItemPrefab()
        {
            EnsureFolderExists();
            
            GameObject foodGO = new GameObject("FoodItem");
            RectTransform rt = foodGO.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80, 80);
            
            Image img = foodGO.AddComponent<Image>();
            img.color = Color.red; // Strawberry color
            
            foodGO.AddComponent<CanvasGroup>();
            foodGO.AddComponent<QLDMathApp.Modules.Counting.DraggableItem>();
            
            string path = PREFAB_PATH + "FoodItem.prefab";
            PrefabUtility.SaveAsPrefabAsset(foodGO, path);
            Object.DestroyImmediate(foodGO);
            
            Debug.Log("[PrefabWizard] Created: " + path);
        }

        [MenuItem("Tools/QLD Math App/Create Map Node Prefab")]
        public static void CreateMapNodePrefab()
        {
            EnsureFolderExists();
            
            GameObject nodeGO = new GameObject("MapNode");
            RectTransform rt = nodeGO.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100, 100);
            
            // Icon
            Image icon = nodeGO.AddComponent<Image>();
            icon.color = new Color(1f, 0.9f, 0.7f);
            
            // Lock overlay (child)
            GameObject lockGO = new GameObject("LockOverlay");
            lockGO.transform.SetParent(nodeGO.transform);
            RectTransform lockRT = lockGO.AddComponent<RectTransform>();
            lockRT.anchorMin = Vector2.zero;
            lockRT.anchorMax = Vector2.one;
            lockRT.offsetMin = Vector2.zero;
            lockRT.offsetMax = Vector2.zero;
            Image lockImg = lockGO.AddComponent<Image>();
            lockImg.color = new Color(0, 0, 0, 0.5f);
            
            // Glow ring (child)
            GameObject glowGO = new GameObject("GlowRing");
            glowGO.transform.SetParent(nodeGO.transform);
            RectTransform glowRT = glowGO.AddComponent<RectTransform>();
            glowRT.anchorMin = new Vector2(-0.1f, -0.1f);
            glowRT.anchorMax = new Vector2(1.1f, 1.1f);
            glowRT.offsetMin = Vector2.zero;
            glowRT.offsetMax = Vector2.zero;
            Image glowImg = glowGO.AddComponent<Image>();
            glowImg.color = new Color(1f, 0.8f, 0.3f, 0.5f);
            glowGO.transform.SetAsFirstSibling();
            glowGO.SetActive(false);
            
            // Avatar anchor (child)
            GameObject anchorGO = new GameObject("AvatarAnchor");
            anchorGO.transform.SetParent(nodeGO.transform);
            RectTransform anchorRT = anchorGO.AddComponent<RectTransform>();
            anchorRT.anchoredPosition = new Vector2(0, -60);
            
            // Button & script
            nodeGO.AddComponent<Button>();
            nodeGO.AddComponent<QLDMathApp.Modules.Hub.MapNode>();
            
            string path = PREFAB_PATH + "MapNode.prefab";
            PrefabUtility.SaveAsPrefabAsset(nodeGO, path);
            Object.DestroyImmediate(nodeGO);
            
            Debug.Log("[PrefabWizard] Created: " + path);
        }

        private static void EnsureFolderExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project"))
            {
                AssetDatabase.CreateFolder("Assets", "_Project");
            }
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
            }
        }
    }
}
#endif
