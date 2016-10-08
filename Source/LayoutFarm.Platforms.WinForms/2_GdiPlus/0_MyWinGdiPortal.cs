﻿//Apache2, 2014-2016, WinterDev

using PixelFarm.Drawing;
namespace LayoutFarm.UI.GdiPlus
{
    public static class MyWinGdiPortal
    {
        public static GraphicsPlatform Start()
        {
            PixelFarm.Drawing.WinGdi.WinGdiPortal.Start();
            var platform = PixelFarm.Drawing.WinGdi.WinGdiPortal.P;
            //platform.TextEditFontInfo = platform.GetFont("tahoma", 14, PixelFarm.Drawing.FontStyle.Regular);
            return platform;
        }
        public static void End()
        {
            PixelFarm.Drawing.WinGdi.WinGdiPortal.End();
        }
        public static GraphicsPlatform P
        {
            get { return PixelFarm.Drawing.WinGdi.WinGdiPortal.P; }
        }
    }
}