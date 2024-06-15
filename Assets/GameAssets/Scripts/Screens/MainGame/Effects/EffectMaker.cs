using System;
using System.Collections.Generic;
using System.Linq;
using com.brg.Common.Localization;
using com.brg.Utilities;
using DG.Tweening;
using JSAM;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace com.tinycastle.StickerBooker.Effects
{
    public class EffectMaker: MonoBehaviour
    {
        [SerializeField] private Transform _frontHost;
        [SerializeField] private ParticleSystem _stickingParticles;
        [SerializeField] private ParticleSystem _hintParticles;
        [SerializeField] private StickerFolder _stickerFolder;
        [SerializeField] private GameObject _thingFlyerPrefab;
        [SerializeField] private GameObject _floaterPrefab;

        [SerializeField] private RectTransform _xmarkHost;
        [SerializeField] private GameObject _xmarkPrefab;
        
        private HashSet<Tween> _flyerTweens = new();
        private Transform _stickingFollower = null;
        private Vector3 _stickingOffset;
        private Transform _hintFollower = null;
        private Vector3 _hintOffset;
        private HashSet<Tween> _xmarkTweens = new();

        private void Update()
        {
            if (_stickingFollower != null)
            {
                var pos = _stickingFollower.position;
                pos.z = _stickingParticles.transform.position.z;
                _stickingParticles.transform.position = pos + _stickingOffset;
            }  
            
            if (_hintFollower != null)
            {
                var pos = _hintFollower.position;
                pos.z = _hintParticles.transform.position.z;
                _hintParticles.transform.position = pos + _hintOffset;
            }
        }

        public Floater MakeFloater(Transform host)
        {
            var go = Instantiate(_floaterPrefab, host, false);
            go.transform.localPosition = Vector3.zero;
            return go.GetComponent<Floater>();
        }

        public void MakeXMark(Vector2 screenPosition)
        {
            var pos = _xmarkHost.transform.position;
            var worldPos = Camera.main.ScreenToWorldPoint(screenPosition);
            var xmark = Instantiate(_xmarkPrefab, _xmarkHost, false);
            xmark.transform.position = worldPos;
            
            // Tween
            AudioManager.PlaySound(LibrarySounds.Failed);
            var tween = DOTween.Sequence()
                .Append(xmark.GetComponent<RectTransform>().DOPunchScale(new Vector3(1.1f, 1.1f, 1.1f), 1.5f, 5, 0f))
                .Append(xmark.GetComponent<CanvasGroup>().DOFade(0f, 0.5f));
            _xmarkTweens.Add(tween);
            tween.OnComplete(() =>
                {
                    _xmarkTweens.Remove(tween);
                    Destroy(xmark.gameObject);
                })
                .Play();
        }

        public void PlayFlyThings(
            Vector3 from,
            Sprite sprite, 
            Transform flyTo,
            int count, 
            Action onEachCompleted,
            Action onAllCompleted,
            float initialFlyDist,
            float time = 2f, 
            float delay = 0.25f)
        {
            var sprites = Enumerable.Repeat(sprite, count).ToArray();
            var actions = Enumerable.Repeat(onEachCompleted, count).ToArray();
            var flyTos = Enumerable.Repeat(flyTo, count).ToArray();
            var counts = Enumerable.Repeat(1, count).ToArray();
            PlayFlyThings(from, sprites, counts, flyTos, actions, onAllCompleted, initialFlyDist, time, delay);
        }
        
        public void PlayFlyThings(
            Vector3 from,
            Sprite[] sprites,
            int[] counts,
            Transform[] flyTos,
            Action[] onEachCompletedActions,
            Action onAllCompleted,
            float initialFlyDist,
            float time = 1.5f,
            float delay = 0.15f)
        {
            var overallSequence = DOTween.Sequence();
            
            var spriteCount = sprites.Length;
            
            var timeSpread = 0.3f * time;
            var timeDelay = 0.2f * time;
            var timeFly = 0.4f * time;
            var timeConclude = 0.1f * time;
            
            var randomInitial = Random.Range(0, 360);
            var randomDirection = Vector2.one.RotateByDeg(randomInitial) * initialFlyDist;
            var increase = 360f / spriteCount;
            
            for (var i = 0; i < spriteCount; ++i)
            {
                var sprite = sprites[i];
                var action = i < onEachCompletedActions.Length ? onEachCompletedActions[i] : null;
                var target = flyTos[i];
                var count = counts[i];
                randomDirection = randomDirection.RotateByDeg(increase);
                var sequence = GetFlySequence(
                    from,
                    sprite, 
                    count,
                    randomDirection, 
                    target, 
                    action,
                    timeSpread,
                    timeDelay, 
                    timeFly, 
                    timeConclude);
                
                overallSequence.Insert(i * delay, sequence);
            }

            overallSequence.OnComplete(() =>
            {
                _flyerTweens.Remove(overallSequence);
                onAllCompleted?.Invoke();
            });
            _flyerTweens.Add(overallSequence);
        }

        private Sequence GetFlySequence(
            Vector3 from,
            Sprite sprite,
            int number,
            Vector2 spreadDir,
            Transform flyTo,
            Action onComplete,
            float timeSpread,
            float timeDelay,
            float timeFly,
            float timeConclude)
        {
            var sequence = DOTween.Sequence();
            var flyer = Instantiate(_thingFlyerPrefab).transform;
            flyer.GetComponent<Image>().sprite = sprite;
            flyer.SetParent(_frontHost);
            from.z = 0f;
            flyer.position = from;
            flyer.localScale = Vector3.zero;

            var numbering = flyer.Find("Numbering");
            var numberText = flyer.Find("Numbering/Number");
            if (number > 1)
            {
                numbering.SetGOActive(true);
                numberText.GetComponent<TextLocalizer>().RawString = number.ToString();
            }
            else
            {
                numbering.SetGOActive(false);
            }

            var tween0 = flyer.transform
                .DOBlendableLocalMoveBy(spreadDir, timeSpread).SetEase(Ease.OutSine); 
            var tween1 = flyer.transform
                .DOScale(1.4f, timeSpread).SetEase(Ease.OutBack); 
            var tween3 = flyer.transform
                .DOScale(0f, timeConclude).SetEase(Ease.InQuart);
                    
            sequence.Append(tween0)
                .Join(tween1)
                .AppendInterval(timeDelay);

            if (flyTo != null && flyTo.gameObject.activeInHierarchy)
            {
                var actualTarget = Camera.main.WorldToScreenPoint(flyTo.position);
                var tween2 = flyer.transform
                    .DOMove(actualTarget, timeFly).SetEase(Ease.InBack);
                sequence.Append(tween2);
            }
            else
            {
                sequence.AppendInterval(timeFly / 2);
            }
            
            sequence.Append(tween3)
            .AppendCallback(() =>
            {
                onComplete?.Invoke();
                Destroy(flyer.gameObject);
            });

            return sequence;
        }
        
        public void PlayStickingParticles(Transform stickTo, Vector3 offset)
        {
            StopStickingParticles();

            _stickingFollower = stickTo;
            offset.z = 0;
            _stickingOffset = offset;
            
            _stickingParticles.Play();
        }
        
        public void StopStickingParticles()
        {
            _stickingParticles.Stop();

            _stickingFollower = null;
            _stickingOffset = Vector3.zero;
        }

        public void PlayHintParticles(Transform stickTo, Vector3 offset)
        {
            StopHintParticles();
            
            _hintFollower = stickTo;
            offset.z = 0;
            _hintOffset = offset;
            
            _hintParticles.Play();
        }

        public void StopHintParticles()
        {
            _hintParticles.Stop();

            _hintFollower = null;
            _hintOffset = Vector3.zero;
        }

        public void PlayStickingAnimation(StaticSticker staticSticker, Action onComplete)
        {
            _stickerFolder.RequestPlay(staticSticker, () =>
            {
                onComplete?.Invoke();
                PlayStickingParticles(staticSticker.transform, Vector3.zero);
            });
        }

        public void StopStickingAnimation()
        {
            _stickerFolder.RequestStop();
        }

        public void PlayFloatyText(string content, Vector3 where, float yMovement)
        {
            
        }

        public void PlayFloatyText(string content, Transform parent, Vector3 offset, float yMovement)
        {
            
        }

        public float GetCanvasScale()
        {
            return 100;
        }

        public void ClearAllEffects()
        {
            _hintParticles.Stop();
            _stickingParticles.Stop();
            
            // TODO
        }
    }
}