using System;
using com.brg.Common;
using com.brg.Common.Localization;
using com.brg.Utilities;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

namespace com.tinycastle.StickerBooker
{
    public class MainGameHudMultiplayerModule : MonoBehaviour
    {
        [SerializeField] private TextLocalizer _playerScore;
        
        [Header("Opponent")]
        [SerializeField] private Image _opponentAvatar;
        [SerializeField] private TextLocalizer _opponentName;
        [SerializeField] private TextLocalizer _opponentScore;
        
        [Header("Others")]
        [SerializeField] private Transform _timeNode;
        [SerializeField] private TextLocalizer _timeText;

        private Tween _bounceTween;
        
        public void SetTimeNode(bool active)
        {
            _timeNode.SetGOActive(active);

            if (!active)
            {
                _bounceTween?.Kill();
                _bounceTween = null;  
            }
        }

        private void OnDisable()
        {
            _bounceTween?.Kill();
            _bounceTween = null;  
        }

        public void SetHud(float time)
        {
            _timeText.RawString = $"{time:00}";

            if (time <= 3) Shake();
            else StopShake();
        }

        public void SetInfo(string opponentName)
        {
            var sprite = GM.Instance.Data.GetAvatar(opponentName);
            _opponentAvatar.sprite = sprite;
            _opponentName.RawString = opponentName;
        }

        public void SetScorePlayer(int score)
        {
            _playerScore.RawString = score.ToString();
            var floater = GM.Instance.Effects.MakeFloater(_playerScore.transform);
            floater.Set($"+{score}", 90f);
        }
        
        public void SetScoreOpponent(int score)
        {
            _opponentScore.RawString = score.ToString();
            var floater = GM.Instance.Effects.MakeFloater(_playerScore.transform);
            floater.Set($"+{score}", 90f);
        }

        private void Shake()
        {
            if (_bounceTween != null) return;

            _timeNode.localScale = Vector3.one;
            
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