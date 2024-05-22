
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using com.brg.Utilities;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.tinycastle.StickerBooker
{
    public class MapGenerator : MonoBehaviour
    {
        private const string DATA_PATH_FULL = "Assets/GameAssets/Data";
        private const string DATA_PATH_LEAN = "/GameAssets/Data";
        
        [SerializeField] private PreviewMap _previewMap;
        [FormerlySerializedAs("_path")] [SerializeField] private string _name;
        [SerializeField] private GameObject _stickerPrefab;
        
        [InspectorButton("LoadMap")]
        public bool loadMap;        
        [InspectorButton("OnSaveMainData")]
        public bool saveCSV;         
        [InspectorButton("OnSaveNumberingData")]
        public bool saveNCsv;        
        [InspectorButton("OnClean")]
        public bool clean;
        
        private void LoadMap()
        {
#if UNITY_EDITOR
            if (_previewMap.HasMap)
            {
                _previewMap.Clean();
            }
            
            var levelName = _name;
            var pathFullSprite = GetFullSpritePath(levelName);
            var fullSprite = AssetDatabase.LoadAssetAtPath<Sprite>(pathFullSprite);
            var mainCsvPath = GetMainCsvPath(levelName);
            var mainCsvContent = AssetDatabase.LoadAssetAtPath<TextAsset>(mainCsvPath).text;
            CsvUtilities.ProcessPositionCsv(mainCsvContent, out var indexedStickers, out var positions);
            var numCsvPath = GetNumberingCsvPath(levelName);

            Dictionary<string, Vector2> numberPositions = new Dictionary<string, Vector2>();
            var numberAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(numCsvPath);
            if (numberAsset == null)
            {
                numberPositions = new Dictionary<string, Vector2>();
            }
            else
            {
                var numCsvContent = numberAsset.text;
                CsvUtilities.ProcessNumberingCsv(numCsvContent, in indexedStickers, out numberPositions);
            }
            
            // Generate map
            _previewMap.SetPreviewMap(levelName, fullSprite);
            
            var dimension = new Vector2(fullSprite.bounds.extents.x * 100, fullSprite.bounds.extents.y * 100); 
            Debug.Log($"Dimension {dimension}");
            
            // Stickers
            foreach (var (stickerName, number) in indexedStickers)
            {
                var spritePath = GetSpritePath(levelName, stickerName);
                Sprite sprite = null;
                try
                {
                    sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                    Debug.Log($"Cannot read ${spritePath}.");
                    throw;
                }

                var definition = new StickerDefinition()
                {
                    Number = number,
                    Name = stickerName,
                    NormalizedPosition = positions[stickerName],
                    AbsoluteNumberingPosition = numberPositions.TryGetValue(stickerName, out var position)
                        ? position
                        : Vector2.zero,
                    Sprite = sprite
                };

                var sticker = MakePreviewSticker();

                _previewMap.AddSticker(sticker, definition, dimension);
            }
#endif
        }

        private void OnSaveMainData()
        {
#if UNITY_EDITOR
            if (!_previewMap.HasMap)
            {
                Debug.LogWarning("There is no map loaded.");
                return;
            }

            var levelName = _previewMap.MapName;
            
            var mainCsvPath = GetMainCsvPath(levelName);
            var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(mainCsvPath);

            var stickers = _previewMap.GetOrderedStickers();
            var write = stickers.ToDictionary(x => x.Number, x => x)
                .OrderBy(x => x.Key)
                .Select(x =>
                {
                    var sname = x.Value.Name;
                    var m = x.Value.ExportStickerPosition();
                    return new object[] { sname, m.x, m.y };
                }).ToArray();
            

            var content = CsvUtilities.GetSavableCsvContent(new[] { "name,x,y" }, write);
            
            File.WriteAllText(AssetDatabase.GetAssetPath(textAsset), content);
            EditorUtility.SetDirty(textAsset);
            
            AssetDatabase.Refresh();
#endif
        }        
        
        private void OnSaveNumberingData()
        {
#if UNITY_EDITOR
            if (!_previewMap.HasMap)
            {
                Debug.LogWarning("There is no map loaded.");
                return;
            }
            
            var levelName = _previewMap.MapName;
            
            var numCsvPath = GetNumberingCsvPath(levelName);
            var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(numCsvPath);

            var stickers = _previewMap.GetOrderedStickers();
            var write = stickers
                .Where(x =>
                {
                    var vec2 = x.ExportStickerNumberingPosition();
                    return vec2.magnitude >= 0.0001f;
                })
                .ToDictionary(x => x.Number, x => x)
                .OrderBy(x => x.Key)
                .Select(x =>
                {
                    var sname = x.Value.Name;
                    var m = x.Value.ExportStickerNumberingPosition();
                    return new object[] { sname, m.x, m.y };
                }).ToArray();
            

            var content = CsvUtilities.GetSavableCsvContent(new[] { "name,x,y" }, write);
            
            File.WriteAllText(GetWriteNumberingCsvPath(levelName), content);

            if (textAsset)
            {
                EditorUtility.SetDirty(textAsset);
            }
            
            AssetDatabase.Refresh();
#endif
        }

        private void OnClean()
        {
#if UNITY_EDITOR
            if (!_previewMap.HasMap)
            {
                Debug.LogWarning("There is no map loaded to clean.");
                return;
            }
            else
            {
                _previewMap.Clean();
                Debug.Log("Cleaned.");
            }
#endif
        }
        
        private static string GetSpritePath(string levelName, string stickerName)
        {
            return $"{DATA_PATH_FULL}/{DataManager.STICKERS_KEY}/{levelName}/{stickerName}.png";
        }

        private static string GetNumberingCsvPath(string levelName)
        {
            return $"{DATA_PATH_FULL}/{DataManager.CSV_NUMBERING_KEY}/{levelName}.csv";
        }

        private static string GetWriteNumberingCsvPath(string levelName)
        {
            return Application.dataPath + $"{DATA_PATH_LEAN}/{DataManager.CSV_NUMBERING_KEY}/{levelName}.csv";
        }

        private static string GetFullSpritePath(string levelName)
        {
            return $"{DATA_PATH_FULL}/{DataManager.FULL_IMAGE_KEY}/{levelName}.png";
        }

        private static string GetMainCsvPath(string levelName)
        {
            return $"{DATA_PATH_FULL}/{DataManager.CSV_KEY}/{levelName}.csv";
        }

        private PreviewSticker MakePreviewSticker()
        {
#if UNITY_EDITOR
            var go = PrefabUtility.InstantiatePrefab(_stickerPrefab) as GameObject;
            return go.GetComponent<PreviewSticker>();
#else
            return null;
#endif
        }
    }
}