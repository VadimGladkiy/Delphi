using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Delphi.Models;

namespace Delphi.Util
{
    class MovieComparer : IEqualityComparer<Movie>
    {
        private delegate bool CompareBy(Movie a, Movie b);

        private CompareBy compareBy;
        public MovieComparer(FilterEnums filter)
        {
            switch (filter) {
                case FilterEnums.year:
                    compareBy = (a, b) => a.Year == b.Year; 
                    break;
                case FilterEnums.country:
                    compareBy = (a, b) => a.Country == b.Country;
                    break;
                case FilterEnums.genre:
                    compareBy = (a, b) => a.Genre == b.Genre;
                    break;
                case FilterEnums.director:
                    compareBy = (a, b) => a.Director.Equals(b.Director);
                    break;
                case FilterEnums.actor:
                    compareBy = (a, b) => (a.Actors.Intersect(b.Actors)).Count() > 0 ;
                    break;
            }
        }
        public bool Equals(Movie x, Movie y)
        {
            return compareBy(x,y);
        }

        // provide your algorithm to get hash code value
        public int GetHashCode(Movie obj)
        {
            return 0;
        }
    }
}
