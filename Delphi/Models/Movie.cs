using System.Collections.Generic;
using System.Xml.Serialization;


namespace Delphi.Models
{
    public class Movie
    {
        [XmlElement("title")]
        public string Title { get; set; }
        [XmlElement("year")]
        public int Year { get; set; }
        [XmlElement("country")]
        public string Country { get; set; }
        [XmlElement("genre")]
        public string Genre { get; set; }
        [XmlElement("summary")]
        public string Summary { get; set; }
        [XmlElement("director")]
        public Director Director { get; set; }
        [XmlElement("actor")]
        public Actor[] Actors { get; set; }
    }
}
