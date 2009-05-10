//////////////////////////////////////////////////////////////
// Koushik Dutta - 9/1/2007
//////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Drawing.Imaging;
using System.Reflection;

namespace WindowlessControls
{
    public partial class WindowlessControlHost : ScrollableControl, IWindowlessControl
    {
        protected override void OnKeyDown(KeyEventArgs e)
        {
            WindowlessControlHost host = this;
            while (host != null)
            {
                host.OnWindowlessKeyDown(this, e);
                host = host.Parent as WindowlessControlHost;
            }
            OnWindowlessUnhandledKeyDown(this, e);
            base.OnKeyDown(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            WindowlessControlHost host = this;
            while (host != null)
            {
                host.OnWindowlessGotFocus(this, e);
                host = host.Parent as WindowlessControlHost;
            }

            base.OnGotFocus(e);
        }

        public void FormlessGotFocus()
        {
            OnGotFocus(new EventArgs());
        }

        public void FormlessKeyDown(KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            WindowlessControlHost host = this;
            while (host != null)
            {
                host.OnWindowlessLostFocus(this, e);
                host = host.Parent as WindowlessControlHost;
            }
            base.OnLostFocus(e);
        }

        public void FormlessLostFocus()
        {
            OnLostFocus(new EventArgs());
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            OnWindowlessKeyUp(this, e);
            base.OnKeyUp(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            WindowlessControlHost host = this;
            while (host != null)
            {
                host.OnWindowlessKeyPress(this, e);
                host = host.Parent as WindowlessControlHost;
            }
            base.OnKeyPress(e);
        }

        protected virtual void OnWindowlessKeyPress(Control sender, KeyPressEventArgs e)
        {
            if (WindowlessKeyPress != null)
                WindowlessKeyPress(sender, e);
        }

        static protected ulong ScoreDistance(long distance, bool positiveDirection)
        {
            long score = distance;

            // stuff in the wrong direction of the edge should be the last choice
            if (score < 0 && positiveDirection)
            {
                return (ulong)Math.Abs(score) + (ulong)long.MaxValue;
            }

            // stuff in the wrong direction of the edge should be the last choice
            if (score > 0 && !positiveDirection)
            {
                return (ulong)score + (ulong)long.MaxValue;
            }

            return (ulong)Math.Abs(score);
        }

        static protected long GetEdgeDistance(int primary1, int secondary1, int dimension1, int primary2, int secondary2, int dimension2, bool positiveDirection)
        {
            Point p1 = new Point(primary1, secondary1 + dimension1 / 2);
            Point p2 = new Point(primary2, secondary2 + dimension2 / 2);

            long dist = 0;
            int sharedEdge1 = secondary1 + dimension1 - secondary2;
            int sharedEdge2 = secondary2 + dimension2 - secondary1;
            int maxShare = Math.Min(dimension1, dimension2);

            // the amount not shared will help differentiate two edges that are on the same primary offset but different shared edge lengths
            if (sharedEdge1 > 0 && sharedEdge1 <= dimension1 && sharedEdge1 <= dimension2)
            {
                p1.Y = p2.Y;
                dist = maxShare - sharedEdge1;
            }
            else if (sharedEdge2 > 0 && sharedEdge2 <= dimension1 && sharedEdge2 <= dimension2)
            {
                p1.Y = p2.Y;
                dist = maxShare - sharedEdge2;
            }
            else if (secondary1 > secondary2 && secondary1 + dimension1 < secondary2 + dimension2)
            {
                p1.Y = p2.Y;
                dist = 0;
            }
            else if (secondary2 > secondary1 && secondary2 + dimension2 < secondary1 + dimension1)
            {
                p1.Y = p2.Y;
                dist = 0;
            }
            else
            {
                if (positiveDirection)
                    dist -= (long.MaxValue / 2);
                else
                    dist += (long.MaxValue / 2);
            }

            //if (sharedEdge1 > 0 && sharedEdge1 <= dimension1 && sharedEdge1 <= dimension2 || sharedEdge2 > 0 && sharedEdge2 <= dimension1 && sharedEdge2 <= dimension2 || secondary1 > secondary2 && secondary1 + dimension1 < secondary2 + dimension2 || secondary2 > secondary1 && secondary2 + dimension2 < secondary1 + dimension1)
            //{
            //    //if (p2.Y >= secondary1 && p2.Y <= secondary1 + dimension1 || p1.Y >= secondary2 && p1.Y <= secondary2 + dimension2)
            //    p1.Y = p2.Y;
            //    //else
            //    //    p1.Y = Math.Min(Math.Abs(p2.Y - secondary1), Math.Abs(p2.Y - (secondary1 + dimension1)));
            //}
            //else
            //{
            //    // ok this isn't a perfect match, so see how close we get.
            //    //p1.Y = Math.Min(Math.Abs(p2.Y - secondary1), Math.Abs(p2.Y - (secondary1 + dimension1)));
            //    if (positiveDirection)
            //        dist -= (Int32.MaxValue / 2);
            //    else
            //        dist += (Int32.MaxValue / 2);
            //}

            int xdif = p1.X - p2.X;
            int ydif = p1.Y - p2.Y;
            // how close they are along the primary dimension is more important than how much of an edge they share. shift by 16 to 
            // weight that property.
            dist += ((long)(xdif * xdif + ydif * ydif) << 16);

            // adjust the score to a signed value depending on whether the edge is in the right directions
            if (positiveDirection)
            {
                if (primary1 < primary2)
                    dist = -Math.Abs(dist);
                else
                    dist = Math.Abs(dist);
            }
            else
            {
                if (primary1 > primary2)
                    dist = Math.Abs(dist);
                else
                    dist = -Math.Abs(dist);
            }
            return dist;
        }

        static Control GetBestNavigationHost(Rectangle rect, Keys key, Control parent, Control exclude, out ulong bestScore)
        {
            // we are in this function because the parent control does not want focus
            bestScore = ulong.MaxValue;

            if (parent.Controls == null || parent == exclude)
                return null;

            Control best = null;
            foreach (Control control in parent.Controls)
            {
                // don't try to refocus onto something within self
                IWindowlessControl host = control as IWindowlessControl;
                bool visible = host != null ? host.Visible : control.Visible;
                if (control == exclude || !visible)
                    continue;

                if (IsFocusable(control))
                {
                    long distance;
                    bool positiveDirection = false;

                    if (key == Keys.Up)
                    {
                        distance = GetEdgeDistance(control.Bottom, control.Left, control.Width, rect.Top, rect.Left, rect.Width, positiveDirection);
                    }
                    else if (key == Keys.Down)
                    {
                        positiveDirection = true;
                        distance = GetEdgeDistance(control.Top, control.Left, control.Width, rect.Bottom, rect.Left, rect.Width, positiveDirection);
                    }
                    else if (key == Keys.Left)
                    {
                        distance = GetEdgeDistance(control.Right, control.Top, control.Height, rect.Left, rect.Top, rect.Height, positiveDirection);
                    }
                    else
                    {
                        positiveDirection = true;
                        distance = GetEdgeDistance(control.Left, control.Top, control.Height, rect.Right, rect.Top, rect.Height, positiveDirection);
                    }

                    ulong score = ScoreDistance(distance, positiveDirection);

                    if (score < bestScore)
                    {
                        bestScore = score;
                        best = control;
                    }
                }

                Rectangle childRect = rect;
                childRect.X -= control.Left;
                childRect.Y -= control.Top;

                // if the control is scrollable, the rect needs to be put into the space of the autoscroll's topleft,
                // not just the client topleft.
                ScrollableControl scrollControl = control as ScrollableControl;
                if (scrollControl != null && control != exclude.Parent)
                {
                    childRect.X += scrollControl.AutoScrollPosition.X;
                    childRect.Y += scrollControl.AutoScrollPosition.Y;
                }

                ulong bestChildScore;
                Control bestChild = GetBestNavigationHost(childRect, key, control, exclude, out bestChildScore);
                if (bestChild != null && bestChildScore < bestScore)
                {
                    bestScore = bestChildScore;
                    best = bestChild;
                }
            }

            // don't return this value, since it would return something in the wrong direction.
            if (bestScore >= long.MaxValue)
                return null;

            return best;
        }

        protected virtual Rectangle GetNavigationSourceRectangle()
        {
            return new Rectangle(0, 0, Width, Height);
        }

        /// <summary>
        /// If a key down event is unhandled, this will attempt to handle it by navigating or scrolling.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="e"></param>
        static void OnWindowlessUnhandledKeyDown(Control control, KeyEventArgs e)
        {
            if (e.Handled)
                return;

            WindowlessControlHost host = control as WindowlessControlHost;

            switch (e.KeyCode)
            {
                case Keys.F19:
                case Keys.F20:
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    {
                        // attempt to navigate first
                        Point p = WindowlessPointToForm(control, Point.Empty);
                        Rectangle sourceRect;
                        if (host != null)
                        {
                            sourceRect = host.GetNavigationSourceRectangle();
                        }
                        else
                        {
                            sourceRect = new Rectangle(0, 0, control.Width, control.Height);
                        }
                        Rectangle rect = sourceRect;
                        rect.X += p.X;
                        rect.Y += p.Y;

                        Control bestControl = null;
                        Control topLevelControl = FormlessTopLevelControl(control);

                        if (e.KeyCode == Keys.F19 || e.KeyCode == Keys.F20)
                        {
                            Control previous = null;
                            Control next = null;
                            bool found = false;
                            RecurseControls(topLevelControl, (current) =>
                            {
                                if (current == control)
                                {
                                    found = true;
                                    return true;
                                }

                                if (!IsFocusable(current))
                                    return true;
                                if (!found)
                                    previous = current;
                                next = current;
                                return !found;
                            },
                               null);

                            if (e.KeyCode == Keys.F19)
                                bestControl = previous;
                            else
                                bestControl = next;
                            //WindowlessNavigateEventArgs ne = new WindowlessNavigateEventArgs(control, 
                        }
                        else
                        {
                            ulong bestScore;
                            bestControl = GetBestNavigationHost(rect, e.KeyCode, topLevelControl, control, out bestScore);
                        }
                        WindowlessNavigateEventArgs ne = new WindowlessNavigateEventArgs(control, sourceRect, e.KeyCode, bestControl);

                        Control parent = control;
                        while (parent != null)
                        {
                            WindowlessControlHost parentHost = parent as WindowlessControlHost;
                            if (parentHost != null)
                                parentHost.OnWindowlessNavigate(control, ne);
                            parent = parent.Parent;
                        }

                        if (ne.Destination != null && ne.Destination != control)
                        {
                            WindowlessControlHost destHost = ne.Destination as WindowlessControlHost;
                            if (destHost != null)
                            {
                                WindowlessControlHost reportChain = destHost;
                                while (reportChain != null)
                                {
                                    reportChain.OnWindowlessNavigatingTo(destHost, ne);
                                    reportChain = reportChain.Parent as WindowlessControlHost;
                                }
                            }
                            if (ne.Destination != null)
                            {
                                e.Handled = true;
                                // focus causes problems if this is not actually on a form
                                if (topLevelControl is Form)
                                    ne.Destination.Focus();
                            }
                        }

                        const int scrollSpeed = 8;

                        // now attempt to scroll
                        parent = control;
                        while (!e.Handled && parent != null)
                        {
                            ScrollableControl scrollControl = parent as ScrollableControl;
                            if (scrollControl != null)
                            {
                                // see if we want to scroll this window
                                if (scrollControl.AutoScroll && !e.Handled)
                                {
                                    int left = Int32.MaxValue;
                                    int top = Int32.MaxValue;
                                    int right = Int32.MinValue;
                                    int bottom = Int32.MinValue;

                                    foreach (Control childControl in scrollControl.Controls)
                                    {
                                        left = Math.Min(left, childControl.Left);
                                        top = Math.Min(top, childControl.Top);
                                        bottom = Math.Max(bottom, childControl.Bottom);
                                        right = Math.Max(right, childControl.Right);
                                    }

                                    Point oldPos = scrollControl.AutoScrollPosition;
                                    if (e.KeyCode == Keys.Down && bottom > scrollControl.ClientSize.Height)
                                    {
                                        scrollControl.AutoScrollPosition = new Point(Math.Abs(scrollControl.AutoScrollPosition.X), Math.Min(Math.Abs(scrollControl.AutoScrollPosition.Y) + scrollSpeed, Math.Abs(scrollControl.AutoScrollPosition.Y) + (bottom - scrollControl.ClientSize.Height)));
                                    }
                                    if (e.KeyCode == Keys.Up && scrollControl.AutoScrollPosition.Y < 0)
                                    {
                                        scrollControl.AutoScrollPosition = new Point(Math.Abs(scrollControl.AutoScrollPosition.X), Math.Max(0, Math.Abs(scrollControl.AutoScrollPosition.Y) - scrollSpeed));
                                    }
                                    if (e.KeyCode == Keys.Right && right > scrollControl.ClientSize.Width)
                                    {
                                        scrollControl.AutoScrollPosition = new Point(Math.Min(Math.Abs(scrollControl.AutoScrollPosition.X) + scrollSpeed, Math.Abs(scrollControl.AutoScrollPosition.X) + (right - scrollControl.ClientSize.Width)), Math.Abs(scrollControl.AutoScrollPosition.Y));
                                    }
                                    if (e.KeyCode == Keys.Left && scrollControl.AutoScrollPosition.X < 0)
                                    {
                                        scrollControl.AutoScrollPosition = new Point(Math.Max(0, Math.Abs(scrollControl.AutoScrollPosition.X) - scrollSpeed), Math.Abs(scrollControl.AutoScrollPosition.Y));
                                    }
                                    e.Handled = scrollControl.AutoScrollPosition != oldPos;
                                }
                            }
                            parent = parent.Parent;
                        }

                    }
                    break;
            }
        }


        protected virtual void OnWindowlessKeyDown(Control sender, KeyEventArgs e)
        {
            if (WindowlessKeyDown != null)
                WindowlessKeyDown(sender, e);
            //WindowlessControlHost.OnWindowlessKeyDown(sender, this, e);
        }

        protected virtual void OnWindowlessNavigatingTo(WindowlessControlHost sender, WindowlessNavigateEventArgs e)
        {
            if (WindowlessNavigatingTo != null)
                WindowlessNavigatingTo(sender, e);
        }

        protected virtual void OnWindowlessNavigate(Control sender, WindowlessNavigateEventArgs e)
        {
            if (WindowlessNavigate != null)
                WindowlessNavigate(sender, e);
        }

        protected virtual void OnWindowlessKeyUp(Control sender, KeyEventArgs e)
        {
        }

        static bool IsFocusable(Control control)
        {
            WindowlessControlHost host = control as WindowlessControlHost;
            if (host != null && host.Focusable)
                return true;
            return control.TabStop;
        }

        public static void WindowlessHookCommonControlKeyEvents(Form form)
        {
            form.KeyPreview = true;
            form.KeyDown += new KeyEventHandler(Control_KeyDown);
            form.KeyPress += new KeyPressEventHandler(Control_KeyPress);
            form.KeyUp += new KeyEventHandler(Control_KeyUp);
        }

        static void Control_KeyUp(object sender, KeyEventArgs e)
        {
            Control control = FindFocusedControl(sender as Control);

            if (control is WindowlessControlHost)
                return;

            Control parent = control;
            while (parent != null)
            {
                WindowlessControlHost host = parent as WindowlessControlHost;
                if (host != null)
                    host.OnWindowlessKeyUp(control, e);
                parent = parent.Parent;
            }
        }

        static void Control_KeyPress(object sender, KeyPressEventArgs e)
        {
            Control control = FindFocusedControl(sender as Control);

            if (control is WindowlessControlHost)
                return;

            Control parent = control;
            while (parent != null)
            {
                WindowlessControlHost host = parent as WindowlessControlHost;
                if (host != null)
                    host.OnWindowlessKeyPress(control, e);
                parent = parent.Parent;
            }
        }

        public static Control FindFocusedControl(Control control)
        {
            if (control.Focused)
                return control;

            foreach (Control child in control.Controls)
            {
                Control result = FindFocusedControl(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        static void Control_KeyDown(object sender, KeyEventArgs e)
        {
            Control control = FindFocusedControl(sender as Control);

            if (control is WindowlessControlHost)
                return;

            Control parent = control;
            while (parent != null)
            {
                WindowlessControlHost parentHost = parent as WindowlessControlHost;
                if (parentHost != null)
                    parentHost.OnWindowlessKeyDown(control, e);
                parent = parent.Parent;
            }
            OnWindowlessUnhandledKeyDown(control, e);
        }

        static Control FindFocusable(Control control)
        {
            WindowlessControlHost host = control as WindowlessControlHost;
            if (host != null && host.Focusable)
                return control;
            foreach (Control child in control.Controls)
            {
                // todo: add more common controls here
                if (IsFocusable(child))
                {
                    return child;
                }
                Control childControl = FindFocusable(child);
                if (childControl != null)
                    return childControl;
            }

            return null;
        }

        protected virtual void OnWindowlessGotFocus(WindowlessControlHost sender, EventArgs e)
        {
            if (WindowlessGotFocus != null)
                WindowlessGotFocus(sender);

            if (sender != this)
                return;

            // if we have focus and are unfocusable, lets just give it to the first focusable control we can find
            // this scenario can happen cause a common control passes focus
            // or the form gives the main host focus at start
            if (!Focusable)
            {
                Control control = FindFocusable(this);
                if (control != null)
                    control.Focus();

                // this control got focus from the form most likely during startup
                // now that it has delegated focus, set its tabstop to false.
                TabStop = false;
            }
        }

        protected virtual void OnWindowlessLostFocus(WindowlessControlHost sender, EventArgs e)
        {
            if (WindowlessLostFocus != null)
                WindowlessLostFocus(sender);
        }

        public static void WindowlessBringIntoView(IWindowlessControl control)
        {
            WindowlessControlHost host = GetHost(control);
            host.SyncWindows();
            Point p = WindowlessPointToHost(control, Point.Empty);
            Rectangle rect = new Rectangle(p.X, p.Y, control.Width, control.Height);
            while (host != null)
            {
                if (host.AutoScroll)
                {
                    Point newScroll = host.AutoScrollPosition;
                    // we favor top and left over bottom and right
                    if (rect.Top < 0)
                    {
                        newScroll.Y -= rect.Top;
                    }
                    else if (rect.Bottom > host.Height && rect.Height <= host.Height)
                    {
                        newScroll.Y += (host.Height - rect.Bottom);
                    }
                    if (rect.Left < 0)
                    {
                        newScroll.X -= rect.Left;
                    }
                    else if (rect.Right > host.Width && rect.Width <= host.Width)
                    {
                        newScroll.Y += (host.Width - rect.Right);
                    }
                    // for some reason you need to pass in positive values to autoscroll, and it converts them to negative values...
                    if (newScroll != host.AutoScrollPosition)
                    {
                        host.AutoScrollPosition = new Point(Math.Abs(newScroll.X), Math.Abs(newScroll.Y));
                        host.SyncWindows();
                    }
                }

                rect.X += host.Left;
                rect.Y += host.Top;
                host = host.Parent as WindowlessControlHost;
            }
        }

        public event WindowlessNavigateEvent WindowlessNavigate;
        public event WindowlessNavigateEvent WindowlessNavigatingTo;
        public event WindowlessKeyEvent WindowlessKeyDown;
        public event WindowlessKeyPressEvent WindowlessKeyPress;
        public event WindowlessFocusEvent WindowlessGotFocus;
        public event WindowlessFocusEvent WindowlessLostFocus;
    }

    public delegate void WindowlessNavigateEvent(Control sender, WindowlessNavigateEventArgs e);
    public delegate void WindowlessKeyEvent(Control sender, KeyEventArgs e);
    public delegate void WindowlessKeyPressEvent(Control sender, KeyPressEventArgs e);
    public delegate void WindowlessFocusEvent(Control sender);

    public class WindowlessNavigateEventArgs
    {
        public WindowlessNavigateEventArgs(Control source, Rectangle sourceRectangle, Keys key, Control destination)
        {
            mySource = source;
            myKey = key;
            myDestination = myBest = destination;
            mySourceRectangle = sourceRectangle;
        }

        Rectangle mySourceRectangle;

        public Rectangle SourceRectangle
        {
            get { return mySourceRectangle; }
        }

        Keys myKey;

        public Keys Key
        {
            get { return myKey; }
        }
        Control mySource;

        public Control Source
        {
            get { return mySource; }
        }
        Control myDestination;

        Control myBest;
        public Control Best
        {
            get { return myBest; }
        }

        bool myHandled = false;
        public bool Handled
        {
            get { return myHandled; }
            set { myHandled = value; }
        }

        public Control Destination
        {
            get { return myDestination; }
            set { myDestination = value; }
        }
    }
}