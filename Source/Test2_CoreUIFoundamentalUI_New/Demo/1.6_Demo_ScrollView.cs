﻿//2014 Apache2, WinterDev
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using LayoutFarm.Text;
using LayoutFarm.UI;

namespace LayoutFarm
{
    [DemoNote("1.6 ScrollView")]
    class Demo_ScrollView : DemoBase
    {
        protected override void OnStartDemo(UISurfaceViewportControl viewport)
        {

            var scbar = new LayoutFarm.SampleControls.UIScrollBar(15, 200);
            scbar.SetLocation(10, 10);
            scbar.MinValue = 0;
            scbar.MaxValue = 170;
            scbar.SmallChange = 20;
            viewport.AddContent(scbar);
            //------------------------- 

            var panel = new LayoutFarm.SampleControls.UIPanel(300, 200);
            panel.SetLocation(30, 10);
            panel.BackColor = Color.LightGray;
            viewport.AddContent(panel);
            //------------------------- 

            //add relation between viewpanel and scroll bar 
            var scRelation = new LayoutFarm.SampleControls.ScrollingRelation(scbar, panel);

            //add content to panel
            for (int i = 0; i < 10; ++i)
            {
                var box1 = new LayoutFarm.SampleControls.UIButton(30, 30);
                box1.BackColor = Color.OrangeRed;
                box1.SetLocation(i * 20, i * 40);

                panel.AddChildBox(box1);
            }
            //--------------------------   
            panel.SetViewport(0, 50);
        }
    }
}