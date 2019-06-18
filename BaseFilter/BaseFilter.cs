using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


namespace BaseFilterAssembly
{
    public abstract class BaseFilter
    {
        public XElement Element { get; private set; }

        public void InjectElement(XElement document)
        {
            Element = document;
        }

        public virtual IEnumerable<string> GetFilters(string filterBy)
        {
            return
            (from p in Element.Elements()
            select p.Element(filterBy).Value).Distinct();
        }

        public virtual IEnumerable<XElement> GetFiltered(IEnumerable<XElement> input, string filterBy)
        {
            return
                from movie in input
                where movie.Element(GetNodeName()).Value == filterBy
                select movie;
        }

        public abstract string GetNodeName();
    }
}
