﻿namespace TelegramThemeCreator
{
    public static class ColorUtils
    {
        public static bool IsColor(string color)
        {
            return color.StartsWith("#");
        }

        public static bool StandartColor(string hexColor)
        {
            return (hexColor.StartsWith("#ffffff")) || (hexColor.StartsWith("#000000"));
        }
    }
}