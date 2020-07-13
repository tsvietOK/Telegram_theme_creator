﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TelegramThemeCreator
{
    public class UniColor
    {
        private static readonly Dictionary<HexFormat, int[]> HexFormatDic = new Dictionary<HexFormat, int[]>()
        {
            { HexFormat.RGB,  new int[] { 0, 1, 2 } },
            { HexFormat.ARGB, new int[] { 3, 0, 1, 2 } },
            { HexFormat.RGBA, new int[] { 0, 1, 2, 3 } },
            { HexFormat.BGR,  new int[] { 2, 1, 0 } },
            { HexFormat.ABGR, new int[] { 3, 2, 1, 0 } },
            { HexFormat.BGRA, new int[] { 2, 1, 0, 3 } }
        };

        private double[] rgb = new double[3];

        public UniColor(System.Drawing.Color color)
            : this(color.R, color.G, color.B, color.A)
        {
        }

        public UniColor(System.Windows.Media.Color color)
            : this(color.R, color.G, color.B, color.A)
        {
        }

        public UniColor(byte red, byte green, byte blue)
            : this(red, green, blue, 255)
        {
        }

        public UniColor(byte red, byte green, byte blue, byte alpha)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        public UniColor(string hex, HexFormat hexFormat)
        {
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }

            if (hex.Length != 8 && hex.Length != 6)
            {
                throw new ArgumentException($"Unknown invalid length of input string '{hex}'");
            }

            var b = hex.Select((x, i) => new { Value = x, Group = i / 2 })
                .GroupBy(x => x.Group)
                .Select(x => string.Join(string.Empty, x.Select(y => y.Value)))
                .Select(x => byte.Parse(x, System.Globalization.NumberStyles.HexNumber))
                .ToArray();
            var indexes = HexFormatDic[hexFormat];
            if (b.Length != indexes.Length)
            {
                throw new ArgumentException($"Invalid hex format specified '{hexFormat}' for string '{hex}'");
            }

            Red = b[indexes[0]];
            Green = b[indexes[1]];
            Blue = b[indexes[2]];
            if (indexes.Length == 4)
            {
                Alpha = b[indexes[3]];
            }
        }

        private UniColor()
        {
        }

        public byte Alpha { get; set; } = 255;

        public byte Red
        {
            get => (byte)(rgb[0] * 255);
            set => rgb[0] = value / 255.0;
        }

        public byte Green
        {
            get => (byte)(rgb[1] * 255);
            set => rgb[1] = value / 255.0;
        }

        public byte Blue
        {
            get => (byte)(rgb[2] * 255);
            set => rgb[2] = value / 255.0;
        }

        public double Hue
        {
            get
            {
                var max = rgb.Max();
                var min = rgb.Min();
                double h;
                if (max == min || ((rgb[0] == rgb[1]) && (rgb[0] == rgb[2])))
                {
                    h = 0;
                }
                else if (max == rgb[0])
                {
                    h = 60 * ((rgb[1] - rgb[2]) / (max - min));
                }
                else if (max == rgb[1])
                {
                    h = 60 * (2 + ((rgb[2] - rgb[0]) / (max - min)));
                }
                else
                {
                    h = 60 * (4 + ((rgb[0] - rgb[1]) / (max - min)));
                }

                if (h < 0)
                {
                    h += 360;
                }

                return h;
            }

            set
            {
                ////if (value < 0 || value > 360)
                ////    throw new ArgumentException($"Saturation value should should be inside [0,360]");
                SetHSV(value, SaturationV, Value);
            }
        }

        public double SaturationV
        {
            get
            {
                var max = rgb.Max();
                var min = rgb.Min();
                double s;
                if (max == 0 || ((rgb[0] == rgb[1]) && (rgb[0] == rgb[2]) && (rgb[0] == 0)))
                {
                    s = 0;
                }
                else
                {
                    s = (max - min) / max;
                }

                return s;
            }

            set
            {
                if (value > 1)
                {
                    value = 1;
                }
                else if (value < 0)
                {
                    value = 0;
                }

                SetHSV(Hue, value, Value);
            }
        }

        public double Value
        {
            get
            {
                var max = rgb.Max();
                return max;
            }

            set
            {
                if (value > 1)
                {
                    value = 1;
                }
                else if (value < 0)
                {
                    value = 0;
                }

                SetHSV(Hue, SaturationV, value);
            }
        }

        public double SaturationL
        {
            get
            {
                var max = rgb.Max();
                var min = rgb.Min();
                double s;
                if (max == 0 || ((rgb[0] == rgb[1]) && (rgb[0] == rgb[2]) && (rgb[0] == 0)))
                {
                    s = 0;
                }
                else if (min == 1 || ((rgb[0] == rgb[1]) && (rgb[0] == rgb[2]) && (rgb[0] == 1)))
                {
                    s = 0;
                }
                else
                {
                    s = (max - min) / (1 - Math.Abs(max + min - 1));
                }

                return s;
            }

            set
            {
                if (value > 1)
                {
                    value = 1;
                }
                else if (value < 0)
                {
                    value = 0;
                }

                SetHSL(Hue, value, Value);
            }
        }

        public double Lightness
        {
            get
            {
                var max = rgb.Max();
                var min = rgb.Min();
                return (max + min) / 2;
            }

            set
            {
                if (value > 1)
                {
                    value = 1;
                }
                else if (value < 0)
                {
                    value = 0;
                }

                SetHSV(Hue, SaturationL, value);
            }
        }

        public static UniColor FromHSV(float hue, float sat, float val)
        {
            return FromHSV(hue, sat, val, 255);
        }

        public static UniColor FromHSV(float hue, float sat, float val, byte alpha)
        {
            var result = new UniColor()
            {
                Alpha = alpha
            };
            result.SetHSV(hue, sat, val);
            return result;
        }

        public System.Drawing.Color ToDrawingColor()
        {
            var result = System.Drawing.Color.FromArgb(Alpha, Red, Green, Blue);
            return result;
        }

        public System.Windows.Media.Color ToMediaColor()
        {
            var result = System.Windows.Media.Color.FromArgb(Alpha, Red, Green, Blue);
            return result;
        }

        public string ToHex()
        {
            var color = ToDrawingColor();
            string result = "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
            if (color.A != 255)
            {
                result += color.A.ToString("X2");
            }

            return result;
        }

        private void SetHSV(double h, double s, double v)
        {
            var c = v * s;
            var h_prime = h / 60;
            var x = c * (1 - Math.Abs((h_prime % 2) - 1));
            var m = v - c;
            var k = (int)h_prime;
            int x_index = 2 - Math.Abs((k + 1) % 3);
            int c_index = ((k + 1) / 2) % 3;
            int o_index = ((k + 4) / 2) % 3;

            var indexes = new StringBuilder("###");
            indexes[x_index] = 'X';
            indexes[c_index] = 'C';
            indexes[o_index] = '0';
            rgb[x_index] = x + m;
            rgb[c_index] = c + m;
            rgb[o_index] = 0 + m;
        }

        private void SetHSL(double h, double s, double l)
        {
            var c = (1 - Math.Abs((2 * l) - 1)) * s;
            var h_prime = h / 60;
            var x = c * (1 - Math.Abs((h_prime % 2) - 1));
            var m = l - (c / 2);
            var k = (int)h_prime;
            int x_index = 2 - Math.Abs((k + 1) % 3);
            int c_index = ((k + 1) / 2) % 3;
            int o_index = ((k + 4) / 2) % 3;

            var indexes = new StringBuilder("###");
            indexes[x_index] = 'X';
            indexes[c_index] = 'C';
            indexes[o_index] = '0';
            rgb[x_index] = x + m;
            rgb[c_index] = c + m;
            rgb[o_index] = 0 + m;
        }
    }
}
