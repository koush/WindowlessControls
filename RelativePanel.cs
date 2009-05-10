using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace WindowlessControls
{
    public class RelativeLayout
    {
        public RelativeLayout()
        {
        }

        public RelativeLayout(IWindowlessControl relative, float x, float y, float width, float height)
        {
            myWidth = width;
            myHeight = height;
            myX = x;
            myY = y;
            myRelative = relative;
        }

        float myWidth = 1;

        public float Width
        {
            get { return myWidth; }
            set { myWidth = value; }
        }
        float myHeight = 1;

        public float Height
        {
            get { return myHeight; }
            set { myHeight = value; }
        }
        float myX = 0;

        public float X
        {
            get { return myX; }
            set { myX = value; }
        }
        float myY = 0;

        public float Y
        {
            get { return myY; }
            set { myY = value; }
        }

        IWindowlessControl myRelative;

        public IWindowlessControl Relative
        {
            get { return myRelative; }
            set { myRelative = value; }
        }

        internal float RelativeX = 0;
        internal float RelativeY = 0;
        internal float RelativeWidth = 1;
        internal float RelativeHeight = 1;
    }

    public class RelativePanel : WindowlessPanel
    {
        int FindOrInsertAncestor(IWindowlessControl control, List<IWindowlessControl> hierarchySorted)
        {
            int index = hierarchySorted.IndexOf(control);
            if (index != -1)
                return index;

            RelativeLayout layout = control.Layout as RelativeLayout;
            // if this is a root, just insert it at the head and go
            if (layout.Relative == null)
            {
                hierarchySorted.Insert(0, control);
                return 0;
            }

            index = FindOrInsertAncestor(layout.Relative, hierarchySorted) + 1;
            hierarchySorted.Insert(index, control);
            return index;
        }

        List<IWindowlessControl> HierarchySort(List<IWindowlessControl> leaves)
        {
            List<IWindowlessControl> hierarchySorted = new List<IWindowlessControl>();
            foreach (IWindowlessControl leaf in leaves)
            {
                FindOrInsertAncestor(leaf, hierarchySorted);
            }
            return hierarchySorted;
        }

        public override bool MeasureUnpadded(System.Drawing.Size bounds, bool boundsChange)
        {
            // prime the root controls and make a list of the leaf controls
            List<IWindowlessControl> leaves = new List<IWindowlessControl>(Controls);
            List<IWindowlessControl> roots = new List<IWindowlessControl>();
            foreach (IWindowlessControl control in Controls)
            {
                RelativeLayout layout = control.Layout as RelativeLayout;
                if (layout == null)
                    control.Layout = layout = new RelativeLayout();
                if (layout.Relative == null)
                {
                    layout.RelativeWidth = layout.RelativeHeight = 1;
                    layout.RelativeX = layout.RelativeY = 0;
                    roots.Add(control);
                }
                else
                {
                    leaves.Remove(layout.Relative);
                }
            }

            List<IWindowlessControl> hierarchySort = HierarchySort(leaves);

            // compute the relative layouts for all but the root controls
            foreach (IWindowlessControl control in Controls)
            {
                RelativeLayout layout = control.Layout as RelativeLayout;
                if (layout.Relative == null)
                    continue;

                layout.RelativeWidth = layout.Width;
                layout.RelativeHeight = layout.Height;
                layout.RelativeX = layout.X;
                layout.RelativeY = layout.Y;
                // climb the lineage and calculate the relative layouts
                IWindowlessControl relative = layout.Relative;
                RelativeLayout relativeLayout = null;
                while (relative != null)
                {
                    relativeLayout = relative.Layout as RelativeLayout;
                    layout.RelativeX = layout.X * relativeLayout.Width + relativeLayout.X;
                    layout.RelativeY = layout.Y * relativeLayout.Height + relativeLayout.Y;
                    layout.RelativeWidth *= relativeLayout.Width;
                    layout.RelativeHeight *= relativeLayout.Height;
                    relative = relativeLayout.Relative;
                }

                // the last thing we should have now is the root controls layout
                // if anything is out of bounds, make note of it in the root control
                relativeLayout.RelativeX = Math.Min(relativeLayout.RelativeX, layout.RelativeX);
                relativeLayout.RelativeY = Math.Min(relativeLayout.RelativeY, layout.RelativeY);
                // for root controls, i am using the relative width/height to record the bottom right corner
                // of the control tree
                relativeLayout.RelativeWidth = Math.Max(layout.RelativeX + layout.RelativeWidth, relativeLayout.RelativeWidth);
                relativeLayout.RelativeHeight = Math.Max(layout.RelativeY + layout.RelativeHeight, relativeLayout.RelativeHeight);
            }

            float maxWidth = 0;
            float maxHeight = 0;
            bool layoutChange = false;
            // measure all the root controls, get the max dims and use that as a reference from
            // here on out
            foreach (IWindowlessControl control in roots)
            {
                RelativeLayout layout = control.Layout as RelativeLayout;
                Rectangle oldRect = new Rectangle(control.Left, control.Top, control.Width, control.Height);
                // always measure, regardless of bounds change or the control needing measure
                // size is relative to other controls...
                Size treeBounds = new Size();
                if (bounds.Width == Int32.MaxValue)
                    treeBounds.Width = Int32.MaxValue;
                else
                    treeBounds.Width = (int)Math.Round(bounds.Width / (layout.RelativeWidth - layout.RelativeX));
                if (bounds.Height == Int32.MaxValue)
                    treeBounds.Height = Int32.MaxValue;
                else
                    treeBounds.Height = (int)Math.Round(bounds.Height / (layout.RelativeHeight - layout.RelativeY));

                control.Measure(treeBounds);

                float treeWidth = (layout.RelativeWidth - layout.RelativeX) * control.Width;
                float treeHeight = (layout.RelativeHeight - layout.RelativeX) * control.Height;
                maxWidth = Math.Max(maxWidth, treeWidth);
                maxHeight = Math.Max(maxHeight, treeHeight);
                layoutChange |= oldRect != new Rectangle(control.Left, control.Top, control.Width, control.Height);
            }

            ClientWidth = (int)Math.Round(maxWidth);
            ClientHeight = (int)Math.Round(maxHeight);

            // lay out the roots
            foreach (IWindowlessControl control in roots)
            {
                RelativeLayout layout = control.Layout as RelativeLayout;
                Rectangle rootRect = new Rectangle();
                // todo: add something to the layout class to persist this?
                float treeWidth = layout.RelativeWidth - layout.RelativeX;
                float treeHeight = layout.RelativeHeight - layout.RelativeY;
                rootRect.Width = (int)Math.Round(ClientWidth / treeWidth);
                rootRect.Height = (int)Math.Round(ClientHeight / treeHeight);

                rootRect.X = (int)Math.Round(-(layout.RelativeX / treeWidth) * ClientWidth) + ClientLeft;
                rootRect.Y = (int)Math.Round(-(layout.RelativeY / treeHeight) * ClientHeight) + ClientTop;
                LayoutControl(control, rootRect);
            }



            // measure out all child controls and layout all controls
            foreach (IWindowlessControl control in hierarchySort)
            {
                RelativeLayout layout = control.Layout as RelativeLayout;
                IWindowlessControl relative = layout.Relative;
                if (relative == null)
                    continue;

                Size childBounds = new Size();
                // calcuate child bounds
                childBounds.Width = (int)Math.Round(layout.RelativeWidth * relative.Width);
                childBounds.Height = (int)Math.Round(layout.RelativeHeight * relative.Height);

                // calculate the layout rect
                Rectangle childRect = new Rectangle();
                childRect.X = (int)Math.Round(layout.RelativeX * relative.Width + relative.Left);
                childRect.Y = (int)Math.Round(layout.RelativeY * relative.Height + relative.Top);
                childRect.Size = childBounds;

                // lay it out
                Rectangle oldRect = new Rectangle(control.Left, control.Top, control.Width, control.Height);
                control.Measure(childBounds);
                LayoutControl(control, childRect);
                layoutChange |= oldRect != new Rectangle(control.Left, control.Top, control.Width, control.Height);
            }

            return layoutChange;
        }
    }
}
