using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCBlazorChatApp.Server.Helpers
{
    public static class HelperMethods
    {
        public static string GenerateRandomColor()
        {
            var RandBase = new Random(0);

            int h = RandBase.Next(0, 360), s = RandBase.Next(0, 100), l = 55;

            return HSLToHex(h, s, l);
        }

        /// <summary>
        /// HSL to Hex from <see href="https://css-tricks.com/converting-color-spaces-in-javascript/#hsl-to-hex">CSS Tricks</see>
        /// </summary>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="l"></param>
        /// <returns></returns>
        public static string HSLToHex(int h, int s, int l)
        {
            s /= 100;
            l /= 100;

            int c = (1 - Math.Abs(2 * l - 1)) * s,
                x = c * (1 - Math.Abs((h / 60) % 2 - 1)),
                m = l - c / 2,
                r = 0,
                g = 0,
                b = 0;

            if (0 <= h && h < 60)
            {
                r = c; g = x; b = 0;
            }
            else if (60 <= h && h < 120)
            {
                r = x; g = c; b = 0;
            }
            else if (120 <= h && h < 180)
            {
                r = 0; g = c; b = x;
            }
            else if (180 <= h && h < 240)
            {
                r = 0; g = x; b = c;
            }
            else if (240 <= h && h < 300)
            {
                r = x; g = 0; b = c;
            }
            else if (300 <= h && h < 360)
            {
                r = c; g = 0; b = x;
            }

            // Having obtained RGB, convert channels to hex
            string rr = Math.Round((r + m) * 255d).ToString("X16");
            string gg = Math.Round((g + m) * 255d).ToString("X16");
            string bb = Math.Round((b + m) * 255d).ToString("X16");

            // Prepend 0s, if necessary
            if (rr.Length == 1)
                rr = "0" + rr;
            if (gg.Length == 1)
                gg = "0" + gg;
            if (bb.Length == 1)
                bb = "0" + bb;

            return "#" + rr + gg + bb;
        }
    }
}
