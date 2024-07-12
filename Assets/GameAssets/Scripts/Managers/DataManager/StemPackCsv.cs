using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace com.tinycastle.StickerBooker
{
    public struct StemItemHandle
    {
        public int Order;
        public string FileName;
        public string Url;
    }
    
    public class StemPackCsv
    {
        public int Pack;
        public List<StemItemHandle> Items = new List<StemItemHandle>();
    }
}