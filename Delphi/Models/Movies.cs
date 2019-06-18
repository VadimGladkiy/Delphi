using System.Xml.Serialization;


namespace Delphi.Models
{
    [XmlRoot("movies")]
    public class Movies
    {
        [XmlElement("movie")]
        public Movie[] MoviesArray { get; set; }
    }
}
