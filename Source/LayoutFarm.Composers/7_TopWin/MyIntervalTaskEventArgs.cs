﻿//2014 Apache2, WinterDev
using System;
using System.Collections.Generic;
using System.Text;
using LayoutFarm.Drawing;

namespace LayoutFarm.UI
{
    class MyIntervalTaskEventArgs : GraphicsTimerTaskEventArgs
    {
        internal void ClearForReuse()
        {
            this.NeedUpdate = 0;
            this.GraphicUpdateArea = Rectangle.Empty;
        }
    }
}