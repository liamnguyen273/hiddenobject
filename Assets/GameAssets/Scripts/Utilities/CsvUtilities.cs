using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using com.brg.Common.Logging;
using UnityEngine;

namespace com.brg.Utilities
{
    public static class CsvUtilities
    {
        public static bool ProcessPositionCsv(string content, out Dictionary<string, int> indexedStickers,
            out Dictionary<string, Vector2> positions)
        {
            var lines = content.Split(new[] { "\n", "\r", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            indexedStickers = new Dictionary<string, int>();
            positions = new Dictionary<string, Vector2>();
            
            var number = 0;
            foreach (var line in lines)
            {
                var tokens = line.Split(",", StringSplitOptions.RemoveEmptyEntries);

                if (tokens.Length != 3)
                {
                    continue;
                }

                var name = tokens[0];
                
                if (indexedStickers.ContainsKey(name))
                {
                    continue;
                }

                float x;
                float y;
                
                if (!float.TryParse(tokens[1], NumberStyles.Float ,CultureInfo.InvariantCulture, out x) 
                 || !float.TryParse(tokens[2], NumberStyles.Float ,CultureInfo.InvariantCulture, out y))
                {
                    continue;
                }
                
                ++number;
                
                indexedStickers.Add(name, number);
                positions.Add(name, new Vector2(x, y));
            }

            return number > 0;
        }

        public static void ProcessNumberingCsv(string content, in Dictionary<string, int> reference,
            out Dictionary<string, Vector2> positions)
        {
            var lines = content.Split(new[] { "\n", "\r", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            positions = new Dictionary<string, Vector2>();
            
            foreach (var line in lines)
            {
                var tokens = line.Split(",", StringSplitOptions.RemoveEmptyEntries);

                if (tokens.Length != 3)
                {
                    continue;
                }

                var name = tokens[0];
                float x;
                float y;
                
                if (!reference.ContainsKey(name)
                    || !float.TryParse(tokens[1], NumberStyles.Float ,CultureInfo.InvariantCulture, out x) 
                    || !float.TryParse(tokens[2], NumberStyles.Float ,CultureInfo.InvariantCulture, out y))
                {
                    continue;
                }

                positions.Add(name, new Vector2(x, y));
            }
        }

        public static string GetSavableCsvContent(string[] header, object[][] lines)
        {
            var builder = new StringBuilder();
            var headerLine = string.Join(',', header);
            builder.Append(headerLine);

            foreach (var line in lines)
            {
                var sline = string.Join(',', line);
                builder.Append("\n");
                builder.Append(sline);
            }

            return builder.ToString();
        }
    }
}
