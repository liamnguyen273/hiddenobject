using System;
using System.Linq;
using com.brg.Common.Logging;
using com.brg.Utilities;
using DG.Tweening;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public class StickerFolder : MonoBehaviour
    {
        [InspectorButton("Restart")]
        public bool restart;         
        [InspectorButton("PlayAnim")]
        public bool playAnim;        
        
        [Header("Params")] 
        [SerializeField] private int _xCount = 5;
        [SerializeField] private int _yCount = 5;
        [SerializeField] private float _time = 1.5f;
        [SerializeField] private Ease _ease = Ease.Linear;

        [Header("Refs")] 
        [SerializeField] private Transform _contentCanvas;
        
        private Vector3[] _startVertices;
        private Vector3[] _endVertices;
        private Vector3[] _vertices;
        
        private float _maxTravelDist;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private Tween _animTween;
        private StaticSticker _follower;
        private Action _onCompleteCallback;
        private float _animProgress;
        
        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        public void RequestPlay(StaticSticker follower, Action onComplete)
        {
            if (follower.GetDefinition().Sprite == null)
            {
                LogObj.Default.Warn($"\"{follower.name}\" does not have valid sprite.");
                onComplete?.Invoke();
                return;
            }

            RefreshSizing();
            RegenerateMesh(follower.GetDefinition().Sprite);
            
            gameObject.SetActive(true);
            if (_animTween != null)
            {
                _animTween.Kill();
                _onCompleteCallback?.Invoke();
            }

            _follower = follower;
            _onCompleteCallback = onComplete;
            
            var a = _startVertices[0];
            var b = _endVertices[0];
            _maxTravelDist = Vector2.Distance(a, b);
            _animProgress = 0;

            var z = transform.position.z;
            
            var pos = _follower.transform.position;
            pos.z = z;
            transform.position = pos;
            
            var scale = _contentCanvas.localScale.x;
            transform.localScale = new Vector3(100 * scale, 100 * scale, 1);
            
            UpdateVertices(0);
            _animTween = DOTween.To(
                    () => _animProgress,
                    (x) => _animProgress = x,
                    _maxTravelDist,
                    _time)
                .SetEase(_ease)
                .OnUpdate(() =>
                {
                    var dist = _animProgress;
                    
                    // Follow position
                    var pos = _follower.transform.position;
                    pos.z = z;
                    transform.position = pos;
                    
                    // Follow scale
                    var scale = _contentCanvas.localScale.x;
                    transform.localScale = new Vector3(100 * scale, 100 * scale, 1);
                    
                    UpdateVertices(dist);
                    UpdateMesh(_vertices);
                })
                .OnComplete(OnConcludePlay)
                .Play();
        }

        public void RequestStop()
        {
            if (_animTween != null)
            {
                _animTween.Kill();
                _onCompleteCallback?.Invoke();
                OnConcludePlay();
            }
        }

        private void OnConcludePlay()
        {
            _onCompleteCallback?.Invoke();
            _animTween = null;
            transform.position = new Vector3(-9999, -9999, transform.position.z);
            gameObject.SetActive(false);
        }

        private void RegenerateMesh(Sprite sprite)
        {
            // 1. Get size in world space.
            const int PPU = 100;
            
            var rect = sprite.rect;
            var sX = rect.width / PPU;
            var sY = rect.height / PPU;
            
            var startVertices = MakeStartVertices(_xCount, _yCount,sX, sY, transform.position.z, 0.1f);
            var frontTriangles = MakeFrontSideTriangles(_xCount, _yCount);
            var backTriangles = MakeBackSideTriangles(_xCount, _yCount);
            var endVertices = MakeEndVertices(_xCount, _yCount, sX, sY, transform.position.z, 0.1f);
            
            _startVertices = startVertices;
            _endVertices = endVertices;
            _vertices = startVertices.Select(x => x).ToArray();
            
            // 3. Make mesh
            var mesh = new Mesh();
            mesh.SetVertices(_vertices);
            mesh.subMeshCount = 2;
            mesh.SetTriangles(frontTriangles, 0);
            mesh.SetTriangles(backTriangles, 1);

            var subUv = sprite.uv;
            var uvs = MakeFitUV(_xCount, _yCount, subUv);
            mesh.SetUVs(0, uvs);
            
            _meshFilter.mesh = mesh;
            
            _meshRenderer.sharedMaterials[0].SetTexture("_MainTex", sprite.texture);
            _meshRenderer.sharedMaterials[1].SetTexture("_MainTex", sprite.texture);
        }
        
        private void UpdateVertices(float dist)
        {
            for (int i = 0; i < _vertices.Length; ++i)
            {
                var start = _startVertices[i];
                var end = _endVertices[i];
                var dir = end - start;

                if (dir.magnitude < dist)
                {
                    _vertices[i] = end;
                }
                else
                {
                    _vertices[i] = start + dir.normalized * dist;
                }
            }
        }

        private void RefreshSizing()
        {
            var scale = Vector3.one * GM.Instance.Effects.GetCanvasScale();
            scale.z = 1;
            transform.localScale = scale;
        }

        private void UpdateMesh(Vector3[] newVertices)
        {
            _meshFilter.mesh.vertices = newVertices;
        }
        
        // private void OnDrawGizmos()
        // {
        //     if (_vertices == null || _startVertices == null || _endVertices == null) return;
        //     
        //     Gizmos.color = Color.green;
        //     for (int i = 0; i < _startVertices.Length; i++) 
        //     {
        //         Gizmos.DrawSphere(_startVertices[i], 0.04f);
        //     }
        //     
        //     Gizmos.color = Color.black;
        //     for (int i = 0; i < _vertices.Length; i++) 
        //     {
        //         Gizmos.DrawSphere(_vertices[i], 0.02f);
        //     }    
        //     
        //     Gizmos.color = Color.red;
        //     for (int i = 0; i < _endVertices.Length; i++) 
        //     {
        //         Gizmos.DrawSphere(_endVertices[i], 0.04f);
        //     }
        // }
        
        private static Vector3[] MakeStartVertices(int gridX, int gridY, float sizeX, float sizeY, float z, float spacing)
        {
            var halfX = sizeX * 0.5f;
            var halfY = sizeY * 0.5f;
            var result1 = new Vector3[(gridX + 1) * (gridY + 1)];
            var result2 = new Vector3[(gridX + 1) * (gridY + 1)];

            var offset = halfX + halfY;
            
            // Make vertices, front side
            for (int i = 0, y = 0; y <= gridY; y++)
            {
                for (int x = 0; x <= gridX; x++, i++)
                {
                    var ratioX = x / (float)gridX;
                    var ratioY = y / (float)gridY;
                    var ax = NumberUtilities.LinearLerp(ratioX, -halfX, halfX);
                    var ay = NumberUtilities.LinearLerp(ratioY, -halfY, halfY);
                    result1[i] = new Vector3(offset - ay,offset - ax, z - spacing);
                }
            }     
            
            // Make vertices, back side
            for (int i = 0, y = 0; y <= gridY; y++)
            {
                for (int x = 0; x <= gridX; x++, i++)
                {
                    var ratioX = x / (float)gridX;
                    var ratioY = y / (float)gridY;
                    var ax = NumberUtilities.LinearLerp(ratioX, -halfX, halfX);
                    var ay = NumberUtilities.LinearLerp(ratioY, -halfY, halfY);
                    result2[i] = new Vector3(offset - ay,offset - ax, z);
                }
            }
            
            return result1.Concat(result2).ToArray();
        }
        
        private static Vector3[] MakeEndVertices(int gridX, int gridY, float sizeX, float sizeY, float z, float spacing)
        {
            var halfX = sizeX * 0.5f;
            var halfY = sizeY * 0.5f;
            var result1 = new Vector3[(gridX + 1) * (gridY + 1)];
            var result2 = new Vector3[(gridX + 1) * (gridY + 1)];

            // Make vertices, front size
            for (int i = 0, y = 0; y <= gridY; y++)
            {
                for (int x = 0; x <= gridX; x++, i++)
                {
                    var ratioX = x / (float)gridX;
                    var ratioY = y / (float)gridY;
                    var ax = NumberUtilities.LinearLerp(ratioX, -halfX, halfX);
                    var ay = NumberUtilities.LinearLerp(ratioY, -halfY, halfY);
                    result1[i] = new Vector3(ax, ay, z);
                }
            }            
            
            // Make vertices, back size
            for (int i = 0, y = 0; y <= gridY; y++)
            {
                for (int x = 0; x <= gridX; x++, i++)
                {
                    var ratioX = x / (float)gridX;
                    var ratioY = y / (float)gridY;
                    var ax = NumberUtilities.LinearLerp(ratioX, -halfX, halfX);
                    var ay = NumberUtilities.LinearLerp(ratioY, -halfY, halfY);
                    result2[i] = new Vector3(ax, ay, z + spacing);
                }
            }
            
            return result1.Concat(result2).ToArray();
        }

        private static int[] MakeFrontSideTriangles(int sizeX, int sizeY)
        {
            var offset = 0;
            int[] triangles = new int[sizeX * sizeY * 6];
            for (int ti = 0, vi = 0, y = 0; y < sizeY; y++, vi++)
            {
                for (int x = 0; x < sizeX; x++, ti += 6, vi++)
                {                    
                    triangles[ti] = offset + vi;
                    triangles[ti + 3] = triangles[ti + 2] = offset + vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = offset + vi + sizeX + 1;
                    triangles[ti + 5] = offset + vi + sizeX + 2;
                }
            }

            return triangles;
        }

        private static int[] MakeBackSideTriangles(int sizeX, int sizeY)
        {
            var offset = (sizeX + 1) * (sizeY + 1);
            int[] triangles = new int[sizeX * sizeY * 6];
            for (int ti = 0, vi = 0, y = 0; y < sizeY; y++, vi++)
            {
                for (int x = 0; x < sizeX; x++, ti += 6, vi++)
                {
                    triangles[ti] = offset + vi;
                    triangles[ti + 3] = triangles[ti + 1] = offset + vi + 1;
                    triangles[ti + 5] = triangles[ti + 2] = offset + vi + sizeX + 1;
                    triangles[ti + 4] = offset + vi + sizeX + 2;
                }
            }

            return triangles;
        }

        private static Vector2[] MakeFitUV(int sizeX, int sizeY, Vector2[] subUV)
        {
            // Get index 2 as bottom left, 1 as top right
            var bottomLeft = subUV[2];
            var topRight = subUV[1];
            
            Vector2[] uv1 = new Vector2[(sizeX + 1) * (sizeY + 1)];
            Vector2[] uv2 = new Vector2[(sizeX + 1) * (sizeY + 1)];
            
            for (int i = 0, y = 0; y <= sizeX; y++) 
            {
                for (int x = 0; x <= sizeY; x++, i++)
                {
                    var ratioX = x / (float)sizeX;
                    var ratioY = y / (float)sizeY;
                    var ax = NumberUtilities.LinearLerp(ratioX, bottomLeft.x, topRight.x);
                    var ay = NumberUtilities.LinearLerp(ratioY, bottomLeft.y, topRight.y);
                    uv1[i] = new Vector2(ax, ay);
                }
            }
                        
            for (int i = 0, y = 0; y <= sizeX; y++) 
            {
                for (int x = 0; x <= sizeY; x++, i++)
                {
                    var ratioX = x / (float)sizeX;
                    var ratioY = y / (float)sizeY;
                    var ax = NumberUtilities.LinearLerp(ratioX, bottomLeft.x, topRight.x);
                    var ay = NumberUtilities.LinearLerp(ratioY, bottomLeft.y, topRight.y);
                    uv2[i] = new Vector2(ax, ay);
                }
            }

            return uv1.Concat(uv2).ToArray();
        }
    }
}