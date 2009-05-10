using System;
using System.Xml.Serialization;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Xml.Schema;
using System.Drawing;

namespace WindowlessControls
{
    public class SmartColor : IXmlSerializable
    {
        Color myColor;
        public static implicit operator Color(SmartColor other)
        {
            return other.myColor;
        }

        public static implicit operator SmartColor(Color other)
        {
            SmartColor color = new SmartColor();
            color.myColor = other;
            return color;
        }

        static Color Parse(string other)
        {
            return Color.FromArgb(Int32.Parse(other.Remove(0, 1), System.Globalization.NumberStyles.HexNumber));
        }

        public override string ToString()
        {
            return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", myColor.A, myColor.R, myColor.G, myColor.B);
        }

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            myColor = Parse(reader.ReadElementContentAsString());
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteString(ToString());
        }

        #endregion
    }
}
