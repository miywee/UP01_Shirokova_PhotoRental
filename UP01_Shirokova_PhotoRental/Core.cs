using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace UP01_Shirokova_PhotoRental
{
    internal class Core
    {
        public static DB_Shirokova_PhotoRentalEntities context =
            new DB_Shirokova_PhotoRentalEntities();

        public static int? CurrentUserId { get; set; }
    }
}

