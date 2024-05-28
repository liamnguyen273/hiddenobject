using com.brg.Common;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public class StemManager : MonoBehaviour, IStemInstrumentPlayer
    {
        [SerializeField] private StemData _stemData;
        [SerializeField] private AudioSource _audioSourcePrefab;

        //Quick Pool implementation
        private List<AudioSource> _audioSourcePool = new List<AudioSource>();
        private List<AudioSource> _audioSourceUsing = new List<AudioSource>();

        private int _currentLayerIndex = 0;

        private const float DEFAULT_VOLUME = 0.6f;
        private const float FADE_TIME = 1.5f;

        private void Start() //Test code
        {
            SetStemData(_stemData);
            // Initialize();
        }

        private void Update() //Test code
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TurnOnNextLayer();
            }
        }

        public void Deinitialize()
        {
            ClearAllAudioSource();
        }

        public void Initialize()
        {
            if(_stemData == null)
            {
                CLog.Error("Empty Stem data");
                return;
            }

            ClearAllAudioSource();

            foreach (AudioClip clip in _stemData.layers)
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

        public void SetStemData(StemData stemData)
        {
            _stemData = stemData;
            _currentLayerIndex = 0;
        }

        public void TurnOnNextLayer()
        {
            if (_stemData == null)
            {
                CLog.Error("No Stem Data set");
                return;
            }

            if (_currentLayerIndex < _stemData.LayerCount)
            {
                _audioSourceUsing[_currentLayerIndex++].volume = DEFAULT_VOLUME;
            }

        }
    }
}