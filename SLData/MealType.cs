using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLData
{
    public class MealType
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal FullAmount { get; set; }
        public decimal ReducedAmount { get; set; }
        public decimal FreeAmount { get; set; }
        public string BarcodeValue { get; set; }
        public bool AllowedFree { get; set; }
        public int iSchoolID { get; set; }
        public int SortOrder { get; set; }
    }
}
