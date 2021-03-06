﻿//Apache2, 2014-present, WinterDev

using System;
using System.Collections.Generic;
using LayoutFarm.ContentManagers;
namespace LayoutFarm.CustomWidgets
{
    public class HtmlHostContentManager
    {
        HtmlBoxes.HtmlHost _htmlHost;
        Dictionary<TextContentManager, int> _textContentManList = new Dictionary<TextContentManager, int>();


        public HtmlHostContentManager()
        {
        }
        public void Bind(HtmlBoxes.HtmlHost htmlHost)
        {
            _htmlHost = htmlHost;
            _htmlHost.AttachEssentailHandlers(
                //1. image req
                (s, e) =>
                {
                    //------
                    //only 1 manager that handle the img req
                    //------
                    //TODO: review 
                    if (ImgLoadingQueue != null)
                    {
                        ImgLoadingQueue.AddRequestImage(e.ImageBinder);
                    }
                },
                //2. stylesheet request
                (s, e) =>
                {


                });
        }
        /// <summary>
        /// add text content manager
        /// </summary>
        /// <param name="textMan"></param>
        public void AddTextContentMan(TextContentManager textMan)
        {
            if (_textContentManList.ContainsKey(textMan))
            {
                return;
            }
            _textContentManList.Add(textMan, 0);
        }
        public ImageLoadingQueueManager ImgLoadingQueue
        {
            get;
            set;
        }
    }
}