﻿namespace BundlesLoader
{
    public static class AssetsRegexs
    {
        public const string TEXTURE_REGEX = "[^\\s]+(.*?)([jJ][pP][gG]|[jJ][pP][eE][gG]|[pP][nN][gG])$";
        public const string META_REGEX = "[^\\s]+(.*?)([mM][eE][tT][aA])$";
        public const string GIF_REGEX = "[^\\s]+(.*?)([gG][iI][fF])$";
        public const string BYTE_REGEX = "[^\\s]+(.*?)([bB][yY][tT][eE][sS]|[bB][yY][tT][eE])$";
        public const string SPRITEATLAS_REGEX = "[^\\s]+(.*?)([sS][pP][rR][iI][tT][eE][aA][tT][lL][aA][sS])$";
        public const string CONFIG_REGEX = "[^\\s]+(.*?)([jJ][sS][oO][nN])$";
    }
}
