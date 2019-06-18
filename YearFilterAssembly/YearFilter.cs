using BaseFilterAssembly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace YearFilterAssembly
{
    public class YearFilter : BaseFilter
    {
        public override string GetNodeName()
        {
            return "year";
        }
    }
}
