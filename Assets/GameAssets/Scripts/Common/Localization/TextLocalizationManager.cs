namespace com.brg.Common.Localization
{
    public class TextLocalizationManager
    {
        private static TextLocalizationManager _instance;

        public static string STR_IS_RAW = "STR_IS_RAW";

        public static TextLocalizationManager Instance
        {
            get
            {
                return _instance ??= new TextLocalizationManager();
            }
        }

        protected TextLocalizationManager()
        {

        }

        public string Localize(string key)
        {
            return key;
        }
    }
}