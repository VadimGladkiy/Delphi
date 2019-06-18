using BaseFilterAssembly;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ActorsFilterAssembly
{
    public class ActorsFilter : BaseFilter
    {
        public override IEnumerable<string> GetFilters(string filterBy)
        {
            var filterWord = filterBy.ToString();
            var actors =
            (from movie in Element.Elements()
             select movie.Element(filterWord));

            var stringified = (from actor in actors
                               select actor.Element("first_name").Value + " " +
                                      actor.Element("last_name").Value).Distinct();

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
            return "actor";
        }
    }
}
