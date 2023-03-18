using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.RequestFeatures
{
    //Base class'dır.Book parametreleri vs..
    public abstract class RequestParameters
    {
        //Maksimum 50 kayıt okuyabilir en fazla.
        const int maxPageSize = 50;
        // Auto-implemented property
        public int PageNumber { get; set; }

        //Full-property
        private int _pageSize;

        public int PageSize
        {
            get { return _pageSize; }
            //50 kayıttan fazla kayıt isterse 50 kayıt geriye dön, 50den küçük ise istenilen değeri dön
            set { _pageSize = value > maxPageSize ? maxPageSize : value; }
        }

        public string? OrderBy { get; set; }
        public string? Fields { get; set; }
    }
}
