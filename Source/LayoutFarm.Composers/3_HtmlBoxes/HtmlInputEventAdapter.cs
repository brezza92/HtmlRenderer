﻿//BSD 2014,WinterDev
//ArthurHub

using System;
using System.Collections.Generic;
using PixelFarm.Drawing;
using LayoutFarm.HtmlBoxes;
using LayoutFarm.WebDom;
using LayoutFarm;
using LayoutFarm.UI;
using LayoutFarm.Composers;

namespace LayoutFarm.HtmlBoxes
{

    /// <summary>
    /// control Html input logic 
    /// </summary>
    public class HtmlInputEventAdapter
    {
        DateTime lastimeMouseUp;
        //-----------------------------------------------
        HtmlIsland _htmlIsland;
        CssBoxHitChain _latestMouseDownChain = null;
        int _mousedownX;
        int _mousedownY;
        bool _isMouseDown;
        IFonts ifonts;
        bool _isBinded;

        const int DOUBLE_CLICK_SENSE = 150;//ms 
        Stack<CssBoxHitChain> hitChainPools = new Stack<CssBoxHitChain>();

        public HtmlInputEventAdapter(IFonts ifonts)
        {
            this.ifonts = ifonts;
        } 
        public void Bind(HtmlIsland htmlIsland)
        {

            this._htmlIsland = htmlIsland;
            _isBinded = htmlIsland != null;
        }
        public void Unbind()
        {
            this._htmlIsland = null;
            this._isBinded = false;

        }
        static void SetEventOrigin(UIEventArgs e, CssBoxHitChain hitChain)
        {
            int count = hitChain.Count;
            if (count > 0)
            {
                var hitInfo = hitChain.GetHitInfo(count - 1);
                e.SourceHitElement = hitInfo.hitObject;
            }
        }
        //---------------------------------------------- 

        public void MouseDown(UIMouseEventArgs e, CssBox startAt)
        {
            if (!_isBinded) return;
            if (startAt == null) return;
            //---------------------------------------------------- 
            ClearPreviousSelection();
            if (_latestMouseDownChain != null)
            {
                ReleaseHitChain(_latestMouseDownChain);
                _latestMouseDownChain = null;
            }

            //----------------------------------------------------
            int x = e.X;
            int y = e.Y;

            this._mousedownX = x;
            this._mousedownY = y;
            this._isMouseDown = true;

            CssBoxHitChain hitChain = GetFreeHitChain();
            hitChain.SetRootGlobalPosition(x, y);
            //1. hittest 
            BoxHitUtils.HitTest(startAt, x, y, hitChain);
            //2. propagate events
            SetEventOrigin(e, hitChain);

            ForEachOnlyEventPortalBubbleUp(e, hitChain, (portal) =>
            {
                portal.PortalMouseDown(e);
                return true;
            });

            if (!e.CancelBubbling)
            {
                ForEachEventListenerBubbleUp(e, hitChain, () =>
                {
                    e.CurrentContextElement.ListenMouseDown(e);
                    return true;
                });
            }
            //----------------------------------
            //save mousedown hitchain
            this._latestMouseDownChain = hitChain;
        }
        public void MouseDown(UIMouseEventArgs e)
        {
            this.MouseDown(e, _htmlIsland.RootCssBox);
        }
        public void MouseMove(UIMouseEventArgs e, CssBox startAt)
        {
            if (!_isBinded) return;
            if (startAt == null) return;
            //-----------------------------------------
            int x = e.X;
            int y = e.Y;

            if (this._isMouseDown)
            {
                //dragging *** , if changed
                if (this._mousedownX != x || this._mousedownY != y)
                {
                    //handle mouse drag
                    CssBoxHitChain hitChain = GetFreeHitChain();
                    hitChain.SetRootGlobalPosition(x, y);

                    BoxHitUtils.HitTest(startAt, x, y, hitChain);
                    SetEventOrigin(e, hitChain);
                    //---------------------------------------------------------
                    //propagate mouse drag 
                    ForEachOnlyEventPortalBubbleUp(e, hitChain, (portal) =>
                    {
                        portal.PortalMouseMove(e);
                        return true;
                    });
                    //---------------------------------------------------------  


                    if (!e.CancelBubbling)
                    {
                        ClearPreviousSelection();

                        if (hitChain.Count > 0)
                        {
                            //create selection range 
                            this._htmlIsland.SetSelection(new SelectionRange(
                                _latestMouseDownChain,
                                hitChain,
                                this.ifonts));
                        }
                        else
                        {
                            this._htmlIsland.SetSelection(null);
                        }
                        ForEachEventListenerBubbleUp(e, hitChain, () =>
                        {

                            e.CurrentContextElement.ListenMouseMove(e);
                            return true;
                        });
                    }


                    //---------------------------------------------------------
                    ReleaseHitChain(hitChain);
                }
            }
            else
            {
                //mouse move  
                //---------------------------------------------------------
                CssBoxHitChain hitChain = GetFreeHitChain();
                hitChain.SetRootGlobalPosition(x, y);
                BoxHitUtils.HitTest(startAt, x, y, hitChain);
                SetEventOrigin(e, hitChain);
                //---------------------------------------------------------

                ForEachOnlyEventPortalBubbleUp(e, hitChain, (portal) =>
                {
                    portal.PortalMouseMove(e);
                    return true;
                });

                //---------------------------------------------------------
                if (!e.CancelBubbling)
                {
                    ForEachEventListenerBubbleUp(e, hitChain, () =>
                    {
                        e.CurrentContextElement.ListenMouseMove(e);
                        return true;
                    });
                }
                ReleaseHitChain(hitChain);
            }
        }
        public void MouseMove(UIMouseEventArgs e)
        {
            if (!_isBinded) return;
            //---------------------------------------------------- 
            this.MouseMove(e, this._htmlIsland.RootCssBox);

        }
        public void MouseUp(UIMouseEventArgs e, CssBox startAt)
        {
            if (!_isBinded) return;
            if (startAt == null) return;
            //---------------------------------------------------- 

            DateTime snapMouseUpTime = DateTime.Now;
            TimeSpan timediff = snapMouseUpTime - lastimeMouseUp;
            bool isAlsoDoubleClick = timediff.Milliseconds < DOUBLE_CLICK_SENSE;
            this.lastimeMouseUp = snapMouseUpTime;
            //--------------------------------------------

            this._isMouseDown = false;
            //----------------------------------------- 
            CssBoxHitChain hitChain = GetFreeHitChain();

            hitChain.SetRootGlobalPosition(e.X, e.Y);
            //1. prob hit chain only 
            BoxHitUtils.HitTest(startAt, e.X, e.Y, hitChain);
            SetEventOrigin(e, hitChain);

            //2. invoke css event and script event   
            ForEachOnlyEventPortalBubbleUp(e, hitChain, (portal) =>
            {
                portal.PortalMouseUp(e);
                return true;
            });

            if (!e.CancelBubbling)
            {
                ForEachEventListenerBubbleUp(e, hitChain, () =>
                {
                    e.CurrentContextElement.ListenMouseUp(e);
                    return true;
                });
            }

            if (!e.IsCanceled)
            {
                //--------------------
                //click or double click
                //--------------------
                if (isAlsoDoubleClick)
                {
                    ForEachEventListenerBubbleUp(e, hitChain, () =>
                    {
                        e.CurrentContextElement.ListenMouseDoubleClick(e);
                        return true;
                    });
                }
                else
                {
                    ForEachEventListenerBubbleUp(e, hitChain, () =>
                    {
                        e.CurrentContextElement.ListenMouseClick(e);
                        return true;
                    });
                }
            }

            ReleaseHitChain(hitChain);
            this._latestMouseDownChain.Clear();
            this._latestMouseDownChain = null;
        }
        public void MouseUp(UIMouseEventArgs e)
        {
            MouseUp(e, this._htmlIsland.RootCssBox);
        }
        public void MouseWheel(UIMouseEventArgs e)
        {
            this.MouseWheel(e, this._htmlIsland.RootCssBox);
        }
        public void MouseWheel(UIMouseEventArgs e, CssBox startAt)
        {

        }

        void ClearPreviousSelection()
        {
            this._htmlIsland.ClearPreviousSelection();
        }
        public void KeyDown(UIKeyEventArgs e)
        {
            this.KeyDown(e, this._htmlIsland.RootCssBox);
        }

        public void KeyDown(UIKeyEventArgs e, CssBox startAt)
        {
            //send focus to current input element

        }

        public void KeyPress(UIKeyEventArgs e)
        {
            this.KeyPress(e, this._htmlIsland.RootCssBox);
        }
        public void KeyPress(UIKeyEventArgs e, CssBox startAt)
        {
            //send focus to current input element

        }
        public bool ProcessDialogKey(UIKeyEventArgs e)
        {
            return this.ProcessDialogKey(e, this._htmlIsland.RootCssBox);
        }
        public bool ProcessDialogKey(UIKeyEventArgs e, CssBox startAt)
        {
            //send focus to current input element
            return false;
        }
        public void KeyUp(UIKeyEventArgs e)
        {
            this.KeyUp(e, this._htmlIsland.RootCssBox);
        }
        public void KeyUp(UIKeyEventArgs e, CssBox startAt)
        {
        }

        static void ForEachOnlyEventPortalBubbleUp(UIEventArgs e, CssBoxHitChain hitPointChain, EventPortalAction eventPortalAction)
        {
            //only listener that need tunnel down 
            for (int i = hitPointChain.Count - 1; i >= 0; --i)
            {
                //propagate up 
                var hitInfo = hitPointChain.GetHitInfo(i);
                IUserEventPortal controller = null;
                switch (hitInfo.hitObjectKind)
                {
                    default:
                        {
                            continue;
                        }
                    case HitObjectKind.Run:
                        {
                            CssRun run = (CssRun)hitInfo.hitObject;
                            controller = CssBox.UnsafeGetController(run.OwnerBox) as IUserEventPortal;

                        } break;
                    case HitObjectKind.CssBox:
                        {
                            CssBox box = (CssBox)hitInfo.hitObject;
                            controller = CssBox.UnsafeGetController(box) as IUserEventPortal;
                        } break;
                }

                //---------------------
                if (controller != null)
                {
                    e.SetLocation(hitInfo.localX, hitInfo.localY);
                    if (eventPortalAction(controller))
                    {
                        return;
                    }
                }
            }
        }

        static void ForEachEventListenerBubbleUp(UIEventArgs e, CssBoxHitChain hitChain, EventListenerAction listenerAction)
        {

            for (int i = hitChain.Count - 1; i >= 0; --i)
            {
                //propagate up 
                var hitInfo = hitChain.GetHitInfo(i);
                IEventListener controller = null;
                switch (hitInfo.hitObjectKind)
                {
                    default:
                        {
                            continue;
                        }
                    case HitObjectKind.Run:
                        {
                            CssRun run = (CssRun)hitInfo.hitObject;
                            controller = CssBox.UnsafeGetController(run.OwnerBox) as IEventListener;

                        } break;
                    case HitObjectKind.CssBox:
                        {
                            CssBox box = (CssBox)hitInfo.hitObject;
                            controller = CssBox.UnsafeGetController(box) as IEventListener;
                        } break;
                }

                //---------------------
                if (controller != null)
                {
                    //found controller

                    e.CurrentContextElement = controller;
                    e.SetLocation(hitInfo.localX, hitInfo.localY);
                    if (listenerAction())
                    {
                        return;
                    }
                }
            }
        }


        CssBoxHitChain GetFreeHitChain()
        {
            if (hitChainPools.Count > 0)
            {
                return hitChainPools.Pop();
            }
            else
            {
                return new CssBoxHitChain();
            }
        }
        void ReleaseHitChain(CssBoxHitChain hitChain)
        {
            hitChain.Clear();
            this.hitChainPools.Push(hitChain);
        }

    }
}