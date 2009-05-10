using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace WindowlessControls
{
    public class FormattedLabel : WrapPanel
    {
        string myFormattedText;
        Font myFont = WindowlessLabel.DefaultFont;

        public FormattedLabel()
        {
        }

        public FormattedLabel(string text)
            : this()
        {
            FormattedText = text;
        }

        public Font Font
        {
            get { return myFont; }
            set
            {
                myFont = value;
                ApplyFormatting();
            }
        }

        void ApplyFormatting()
        {
            Controls.Clear();

            SuspendRemeasure();
            // shitty hack, must fix someday
            string temp = myFormattedText.Replace("<b>", "`");
            temp = temp.Replace("</b>", "`");
            string[] fragments = temp.Split('`');
            Font boldedFont = new Font(myFont.Name, myFont.Size, FontStyle.Bold);

            for (int i = 0; i < fragments.Length; i++)
            {
                Font font = i % 2 == 0 ? myFont : boldedFont;
                string[] subFragments = fragments[i].Split(' ');
                foreach (string subFragment in subFragments)
                {
                    if (subFragment == string.Empty)
                        continue;
                    WindowlessLabel label = new WindowlessLabel(subFragment + " ");
                    label.Font = font;
                    Controls.Add(label);
                }
            }
            ResumeRemeasure();
            Remeasure();
        }

        public string FormattedText
        {
            get { return myFormattedText; }
            set
            {
                myFormattedText = value;
                ApplyFormatting();
            }
        }
    }
}
