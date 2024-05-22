using DG.Tweening;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public partial class MainGameManager
    {
        private DynamicSticker _tutorialDynamicSticker;
        private Vector3 _tutorialStickerPos;
        
        private void StartTutorial()
        {
            var sticker = _dynamicStickers[0];
            _tutorialDynamicSticker = sticker;

            var pos = sticker.transform.position;
            _tutorialStickerPos = pos;
            _cursor.SetPositionAndText(pos, "Drag the sticker...");
            _cursor.FadeIn();
            _cursor.ZoomIn();

            sticker.OnDragStart += OnTutorialStartDrag;
            sticker.OnStickEvent += OnTutorialSticker;
            sticker.OnStickFailedEvent += OnTutorialStickerFailed;
        }

        private void OnTutorialStartDrag()
        {
            //_cursor.ZoomOut();
            DOVirtual.DelayedCall(0.1f, () =>
            {
                var target = _tutorialDynamicSticker.GetLinkedStaticSticker().transform.position;
                _cursor.SetPositionAndText(target, "Move the sticker to\nthe correct frame");
                _cursor.SetMove(_tutorialStickerPos, target);
            });
        }

        private void OnTutorialSticker()
        {
            _cursor.FadeOut();
            DOVirtual.DelayedCall(1f, () =>
            {
                var pop = GM.Instance.Popups.GetPopup<PopupBehaviourTutorialDone>(out var behaviour);
                behaviour.OnShowStart(ConcludeTutorial);
                pop.Show();
            });
        }

        private void OnTutorialStickerFailed()
        {
            _cursor.StopMove();
            StartTutorial();
        }

        private void ConcludeTutorial()
        {
            GM.Instance.Player.SetTutorialPlayed();
            
            _tutorialDynamicSticker.OnDragStart -= OnTutorialStartDrag;
            _tutorialDynamicSticker.OnStickEvent -= OnTutorialSticker;
            _tutorialDynamicSticker.OnStickFailedEvent -= OnTutorialStickerFailed;
            
            GameState = GameState.IN_GAME;
        }
    }
}