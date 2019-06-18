using BaseFilterAssembly;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DirectorFilterAssembly
{
    public class DirectorFilter : BaseFilter
    {
        public override IEnumerable<string> GetFilters(string filterBy)
        {
            var directors =
            (from movie in Element.Elements()
             select movie.Element(filterBy));

            var stringified = (from director in directors
                             select director.Element("first_name").Value + " " + 
                                    director.Element("last_name").Value).Distinct();

            return stringified;   
        }


        public override IEnumerable<XElement> GetFiltered(IEnumerable<XElement> input, string filterBy)
        {
            var name = filterBy.Split(' ');
            return
                 from movie in input
                 where (movie.Element(GetNodeName()).Element("first_name").Value == name[0] &&  
                        movie.Element(GetNodeName()).Element("last_name").Value == name[1])
                 select movie;
        }


        public override string GetNodeName()
        {
            return "director";
        }
    }
}
