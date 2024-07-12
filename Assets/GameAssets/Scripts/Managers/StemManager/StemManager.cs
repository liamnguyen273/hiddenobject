using com.brg.Common;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.brg.Common.Logging;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public class StemManager : MonoBehaviour, IStemInstrumentPlayer
    {
        [SerializeField] private AudioSource _audioSourcePrefab;
        [SerializeField] private OfflinePlayingStemData[] _fallbacks;
        [SerializeField] private float _downloadTimeout = 20f;

        private Dictionary<int, IPlayingStemData> _datas = new();
        
        //Quick Pool implementation
        private List<AudioSource> _audioSourcePool = new List<AudioSource>();
        private List<AudioSource> _audioSourceUsing = new List<AudioSource>();

        private int _currentLayerIndex = 0;

        private const float DEFAULT_VOLUME = 0.6f;
        private const float FADE_TIME = 1.5f;
        
        private IPlayingStemData _currentStemData;

        public bool StemReady
        {
            get
            {
                if (_currentStemData == null)
                {
                    LogObj.Default.Error("Current stem data is null, cannot query readiness.");
                    return true;
                }

                if (_currentStemData is StreamingPlayingStemData streamingPlayingStemData)
                {
                    return streamingPlayingStemData.LoadCompleted;
                }

                return true;
            }
        }

        public float DownloadProgress => _currentStemData?.DownloadProgress ?? 1f;

        private void Update() //Test code
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TurnOnNextLayer();
            }
        }

        public void Deactivate()
        {
            ClearAllAudioSource();
        }

        public void LoadStemsFor(LevelEntry entry)
        {
            var stem = GetPlayingStemData(entry);
            
            if (stem is StreamingPlayingStemData streamingPlayingStemData)
            {
                streamingPlayingStemData.LaunchDownloadTask(_downloadTimeout);
            }

            _currentStemData = stem;
        }

        private IPlayingStemData GetPlayingStemData(LevelEntry entry)
        {
            IPlayingStemData data = null;
            var number = entry.SortOrder % 14;

            // From existing
            if (_datas.TryGetValue(number, out var cachedData))
            {
                data = cachedData;
                return data;
            }
            
            // Make new from: fallback
            foreach (var fallback in _fallbacks)
            {
                if (number == fallback.PackNumber)
                {
                    data = fallback;
                    break;
                }
            }
            
            if (data == null)
            {
                // Get from data
                var def = GM.Instance.Data.GetStemDefinition(number);
                if (def != null)
                {
                    data = new StreamingPlayingStemData(def);
                }
            }

            if (data != null)
            {
                _datas.Add(number, data);
                return data;
            }
            else return null;
        }

        private int _currentFallback = 0;
        public void Activate()
        {
            ClearAllAudioSource();

            var hasNoStem = false;

            if (_currentStemData == null)
            {
                hasNoStem = true;
                LogObj.Default.Info("_currentStemData is null, will use a fallback.");
            }
            else if (_currentStemData is StreamingPlayingStemData { Usable: false })
            {
                hasNoStem = true;
                LogObj.Default.Info("_currentStemData failed to download stems in time, will use a fallback.");
            }

            if (hasNoStem)
            {
                _currentStemData = _fallbacks[_currentFallback];
                
                _currentFallback += 1;
                _currentFallback %= _fallbacks.Length;
            }
            
            foreach (AudioClip clip in _currentStemData.GetAllClips())
            {
                AudioSource audioSource;
                if (_audioSourcePool.Count == 0)
                {
                    audioSource = Instantiate(_audioSourcePrefab, transform);
                }
                else
                {
                    audioSource = _audioSourcePool.First();
                    _audioSourcePool.Remove(audioSource);
                    audioSource.gameObject.SetActive(true);
                }
                _audioSourceUsing.Add(audioSource);
                audioSource.clip = clip;
                audioSource.volume = 0;
                audioSource.Play();
            }
        }

        public void Mute(params int[] instrumentIndexes)
        {
            foreach (int index in instrumentIndexes)
            {
                if (index >= 0 && index < _audioSourceUsing.Count)
                {
                    _audioSourceUsing[index].volume = 0;
                }
                else
                {
                    CLog.Error($"Invalid index: {index}");
                }
            }
        }

        public void MuteAll()
        {
            foreach (AudioSource src in _audioSourceUsing)
            {
                src.volume = 0;
            }
        }

        public void Pause()
        {
            foreach (AudioSource src in _audioSourceUsing)
            {
                src.Pause();
            }
        }

        public void Play(bool immediately, params int[] instrumentIndexes)
        {
            foreach (int index in instrumentIndexes)
            {
                if (index >= 0 && index < _audioSourceUsing.Count)
                {
                    if (immediately)
                    {
                        _audioSourceUsing[index].volume = DEFAULT_VOLUME;
                    }
                    else
                    {
                        _audioSourceUsing[index].DOFade(DEFAULT_VOLUME, FADE_TIME).Play();
                    }
                }
                else
                {
                    CLog.Error($"Invalid index: {index}");
                }
            }
        }

        public void PlayAll()
        {
            foreach (AudioSource src in _audioSourceUsing)
            {
                src.Play();
            }
        }

        public void Resume()
        {
            foreach (AudioSource src in _audioSourceUsing)
            {
                src.UnPause();
            }
        }

        private void ClearAllAudioSource()
        {
            foreach (var audioSource in _audioSourceUsing)
            {
                audioSource.Stop();
                audioSource.gameObject.SetActive(false);
                _audioSourcePool.Add(audioSource);
            }
            _audioSourceUsing.Clear();
        }

        public void TurnOnNextLayer()
        {
            if (_currentStemData == null)
            {
                CLog.Error("No Stem Data set");
                return;
            }

            if (_currentLayerIndex < _currentStemData.ClipCount)
            {
                _audioSourceUsing[_currentLayerIndex++].volume = DEFAULT_VOLUME;
            }

        }
    }
}