using com.brg.Common;
using com.brg.Common.Localization;
using com.brg.Utilities;
using DG.Tweening;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public class TutorialCursor : MonoBehaviour
    {
        [Header("Params")]
        [SerializeField] private CanvasGroup _blackGroup;
        [SerializeField] private TextLocalizer _text;
        [SerializeField] private Transform _cursor;
        [SerializeField] private Transform _mask;

        private Tween _fadeTween = null;
        private Tween _zoomTween = null;
        private Tween _bobTween = null;
        private Tween _moveTween = null;

        private void Awake()
        {
            _blackGroup.alpha = 0;
            _mask.SetGOActive(false);
            gameObject.SetActive(false);
            _text.SetGOActive(false);
        }

        public void SetPositionAndText(Vector3 where, string text)
        {
            CLog.Log("where " + where);

            _text.RawString = text;
            transform.position = new Vector3(where.x, where.y, _cursor.position.z);
        }

        public void SetMove(Vector3 from, Vector3 to)
        {
            if (_moveTween != null)
            {
                _moveTween.Kill();
                _moveTween = null;
            }

            if (_bobTween != null)
            {
                _bobTween.Kill();
            }

            var distance = (from - to).magnitude;
            var time = 1.5f;

            transform.position = from;
            _cursor.localScale = Vector3.zero;
            var sequence = DOTween.Sequence();
            sequence
                .Append(_cursor.DOScale(1f, 0.5f).SetEase(Ease.OutBack))
                .AppendInterval(0.2f)
                .Append(transform.DOMove(to, time))
                .AppendInterval(0.2f)
                .Append(_cursor.DOScale(0f, 0.5f).SetEase(Ease.InBack))
                .AppendCallback(() =>
                {
                    transform.position = from;
                })
                .SetLoops(-1, LoopType.Restart)
                .Play();
            _moveTween = sequence;
        }

        public void StopMove()
        {
            if (_moveTween != null)
            {
                _moveTween.Kill();
                _moveTween = null;
            }
        }
        
        public void FadeIn()
        {
            if (_fadeTween != null)
            {
                _fadeTween.Kill();
                _fadeTween = null;
            }

            gameObject.SetActive(true);
            _text.SetGOActive(true);
            _blackGroup.alpha = 0;
            _fadeTween = _blackGroup.DOFade(1f, 0.5f).OnComplete(() => _fadeTween = null).Play();

            // Bob cursor

            if (_bobTween != null)
            {
                _bobTween.Kill();
                _bobTween = null;
            }

            _cursor.localScale = Vector3.one;
            var bobSequence = DOTween.Sequence();
            var bobInTween = _cursor.DOBlendableScaleBy(new Vector3(0.5f, 0.5f, 0.5f), 1.25f).SetEase(Ease.InOutSine);
            var bobOutTween = _cursor.DOBlendableScaleBy(new Vector3(-0.5f, -0.5f, -0.5f), 1.25f).SetEase(Ease.InOutSine);
            _bobTween = bobSequence.Append(bobInTween)
                .Append(bobOutTween)
                .SetLoops(-1, LoopType.Restart)
                .Play();
        }

        public void FadeOut()
        {
            if (_fadeTween != null)
            {
                _fadeTween.Kill();
                _fadeTween = null;
            }

            _fadeTween = _blackGroup.DOFade(0f, 0.5f)
                .OnComplete(() =>
                {
                    _fadeTween = null;

                    if (_bobTween != null)
                    {
                        _bobTween.Kill();
                        _bobTween = null;
                    }

                    if (_moveTween != null)
                    {
                        _moveTween.Kill();
                        _moveTween = null;
                    }

                    _cursor.localScale = Vector3.one;

                    _blackGroup.alpha = 0;
                    gameObject.SetActive(false);
                    _text.SetGOActive(false);
                })
                .Play();
        }

        public void ZoomIn()
        {
            if (_zoomTween != null)
            {
                _zoomTween.Kill();
                _zoomTween = null;
            }

            _mask.SetGOActive(true);
            _mask.localScale = new Vector3(16, 16, 16);
            _zoomTween = _mask.DOScale(1f, 1.25f).OnComplete(() => _zoomTween = null)
                .SetEase(Ease.OutQuad)
                .Play();
        }
        public void ZoomOut()
        {
            if (_zoomTween != null)
            {
                _zoomTween.Kill();
                _zoomTween = null;
            }

            _zoomTween = _mask.DOScale(16f, 1.25f).OnComplete(() =>
            {
                _mask.SetGOActive(false);
                _zoomTween = null;
            })
                .SetEase(Ease.OutQuad)
                .Play();
        }
    }
}