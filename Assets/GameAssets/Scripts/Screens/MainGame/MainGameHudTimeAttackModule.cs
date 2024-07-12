using com.brg.Common;
using com.brg.Common.Localization;
using com.brg.Utilities;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace com.tinycastle.StickerBooker
{
    public class MainGameHudTimeAttackModule : MonoBehaviour
    {
        [SerializeField] private Transform _timeNode;
        [SerializeField] private TextLocalizer _timeText;

        private Tween _bounceTween;
        
        public void SetTimeNode(bool active)
        {
            _timeNode.SetGOActive(active);
        }
        
        public void SetHud(float time)
        {
            var stime = Mathf.CeilToInt(time);
            var m = stime / 60;
            var s = stime % 60;
            _timeText.RawString = m > 0 ? $"{m}:{s:00}" : $"{s:00}";

            if (time <= 3) Shake();
            else StopShake();
        }

        private void Shake()
        {
            if (_bounceTween != null) return;
                
            _bounceTween = DOTween.Sequence()
                .Append(_timeNode.DOScale(1.1f, 0.25f).SetEase(Ease.InOutSine))
                .Append(_timeNode.DOScale(1f, 0.25f).SetEase(Ease.InOutSine))
                .SetLoops(-1, LoopType.Restart)
                .Play();
        }

        private void StopShake()
        {
            if (_bounceTween == null) return;
            
            _bounceTween.Kill();
            _bounceTween = null;
        }
    }
}