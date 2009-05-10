using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml.Serialization;

namespace WindowlessControls
{
    public class SerializableControl : WindowlessControlProxy
    {
        protected virtual Stream GetSerializationStream()
        {
            Type type = this.GetType();
            Assembly assembly = type.Assembly;
            return assembly.GetManifestResourceStream(string.Format("{0}.{1}.xml", assembly.GetName().Name, type.Name));
        }

        public void Connect(IWindowlessControl control)
        {
            Type type = GetType();
            WindowlessControlHost.RecurseWindowlessControls(control, (current) =>
            {
                if (string.IsNullOrEmpty(current.Name))
                    return true;

                PropertyInfo prop = type.GetProperty(current.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                {
                    if (prop.GetValue(this, null) != null)
                        throw new InvalidOperationException(string.Format("Unable to connect to property {0}. The value is non-null", current.Name));
                    prop.SetValue(this, current, null);
                    return true;
                }

                FieldInfo field = type.GetField(current.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    if (field.GetValue(this) != null)
                        throw new InvalidOperationException(string.Format("Unable to connect to field {0}. The value is non-null", current.Name));
                    field.SetValue(this, current);
                }
                return true;
            },
                null);
        }

        protected virtual void OnCreateXmlSerializer(XmlAttributeOverrides overrides, List<Type> additionalTypes, List<Type> additionalControls)
        {
            if (GetType().Assembly != typeof(IWindowlessControl).Assembly)
            {
                AddAssemblyControlsToList(GetType().Assembly, additionalControls);
            }
        }

        static void XmlIgnoreAttributeOverrides(XmlAttributeOverrides overrides, Type type, List<string> ignore)
        {
            List<MemberInfo> members = new List<MemberInfo>();
            members.AddRange(type.GetFields());
            members.AddRange(type.GetProperties());
            XmlAttributes attrs = new XmlAttributes();
            attrs.XmlIgnore = true;
            foreach (MemberInfo member in members)
            {
                if (ignore.Contains(member.Name))
                    continue;
                overrides.Add(member.DeclaringType, member.Name, attrs);
            }
        }

        static void XmlIgnoreAttributeOverrides(XmlAttributeOverrides overrides, Type type, params string[] ignore)
        {
            List<string> ignoreList = new List<string>(ignore);
            XmlIgnoreAttributeOverrides(overrides, type, ignoreList);
        }

        public static void AddAssemblyControlsToList(Assembly assembly, List<Type> controls)
        {
            Type[] allTypes = assembly.GetTypes();
            foreach (Type type in allTypes)
            {
                if (type.IsGenericType || !type.IsPublic)
                    continue;
                Type[] interfaces = type.GetInterfaces();
                foreach (Type iface in interfaces)
                {
                    if (iface == typeof(IWindowlessControl))
                        controls.Add(type);
                }
            }
        }

        IWindowlessControl Load(Stream stream)
        {
            List<Type> additionalControls = new List<Type>();
            AddAssemblyControlsToList(Assembly.GetExecutingAssembly(), additionalControls);

            XmlAttributeOverrides overrides = new XmlAttributeOverrides();
            XmlAttributes xmlArrayItems = new XmlAttributes();
            xmlArrayItems.XmlArray = new XmlArrayAttribute("Controls");
            overrides.Add(typeof(WindowlessControlHost), "SerializableControls", xmlArrayItems);
            overrides.Add(typeof(WindowlessControl), "Controls", xmlArrayItems);

            XmlIgnoreAttributeOverrides(overrides, typeof(WindowlessControlHost).BaseType);

            List<Type> additionalTypes = new List<Type>();
            additionalTypes.Add(typeof(SmartColor));

            OnCreateXmlSerializer(overrides, additionalTypes, additionalControls);
            foreach (Type type in additionalControls)
            {
                xmlArrayItems.XmlArrayItems.Add(new XmlArrayItemAttribute(type));
            }

            XmlSerializer ser = new XmlSerializer(typeof(WindowlessControlHost), overrides, additionalTypes.ToArray(), null, null);

            //{
            //    WindowlessControlHost test = new WindowlessControlHost();
            //    test.Orientation = WindowlessControls.Orientation.Vertical;
            //    test.Control = new StackPanel();
            //    test.Control.Controls.Add(new WindowlessLabel("hello world"));

            //    MemoryStream mem = new MemoryStream();
            //    ser.Serialize(mem, test);
            //    mem.Seek(0, SeekOrigin.Begin);
            //    string text = new StreamReader(mem).ReadToEnd();
            //}

            IWindowlessControl ret = ser.Deserialize(stream) as IWindowlessControl;
            Connect(ret);
            return ret;
        }

        protected virtual void OnLoad()
        {
        }

        public SerializableControl()
        {
            using (Stream stream = GetSerializationStream())
            {
                Control = Load(stream);
            }
            OnLoad();
        }
    }
}
