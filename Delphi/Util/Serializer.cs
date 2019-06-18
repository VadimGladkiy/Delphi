using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Delphi.Util
{
    internal class Serializer
    {
        public T Deserialize<T>(List<XElement> elements)
        {
            XmlSerializer ser = new XmlSerializer(typeof(Models.Movies), new XmlRootAttribute("movies"));

            var parent = new XElement("movies",elements);

            var reader = parent.CreateReader();

            object c;
            if(ser.CanDeserialize(reader))
            {
                c = ser.Deserialize(reader);
                return (T)c;
            }

            throw new NotImplementedException();
        }

        private T Deserialize<T>(string input) where T : class
        {
            XmlSerializer ser = new XmlSerializer(typeof(Models.Movies), new XmlRootAttribute("Movies"));

            using (StringReader sr = new StringReader(input))
            {
                return (T)ser.Deserialize(sr);
            }
        }
        
        public string Serialize<T>(T ObjectToSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(ObjectToSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, ObjectToSerialize);
                return textWriter.ToString();
            }
        }
    }
}
