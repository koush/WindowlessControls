using System;
using System.Collections.Generic;
using System.Text;

namespace WindowlessControls
{
    public class WindowlessControlWrapper<T> : WindowlessControlHost where T : class, IWindowlessControl, new ()
    {
        public WindowlessControlWrapper()
        {
            base.Control = new T();
        }

        public new T Control
        {
            get
            {
                return base.Control as T;
            }
        }
    }

    public class VerticalWrapPanelHost : WindowlessControlWrapper<WrapPanel>
    {
        public VerticalWrapPanelHost()
        {
            Orientation = Orientation.Vertical;
        }
    }


    public class VerticalDockPanelHost : DockPanelHost
    {
        public VerticalDockPanelHost()
        {
            Orientation = WindowlessControls.Orientation.Vertical;
        }
    }

    public class DockPanelHost : WindowlessControlHost
    {
        public DockPanelHost()
        {
            base.Control = new DockPanel();
        }

        public new DockPanel Control
        {
            get
            {
                return base.Control as DockPanel;
            }
        }
    }

    public class VerticalStackPanelHost : WindowlessControlHost
    {
        public VerticalStackPanelHost()
        {
            Orientation = WindowlessControls.Orientation.Vertical;
            base.Control = new StackPanel();
        }

        public new StackPanel Control
        {
            get
            {
                return base.Control as StackPanel;
            }
        }
    }
}