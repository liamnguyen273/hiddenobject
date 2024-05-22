using UnityEngine;
using UnityEngine.UI;

namespace com.tinycastle.StickerBooker
{
    [RequireComponent(typeof(Image))]
    public class ThemeableImage: ThemeableComponent<Sprite>
    {
        [SerializeField] private Sprite _christmasThemeImage;
        
        public override void SetTheme(string themeName)
        {
            var image = GetComponent<Image>();
            image.sprite = GetThemedResource(themeName);
        }

        public override Sprite GetThemedResource(string themeName)
        {
            if (themeName == GlobalConstants.CHRISTMAS_THEME)
            {
                return _christmasThemeImage;
            }

            return GetComponent<Image>().sprite;
        }
    }
}