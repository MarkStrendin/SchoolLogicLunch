using System;
using System.Dynamic;
using System.Xml.Serialization;

namespace SLData
{
    public class PurchasedMeal
    {
        public int MealID { get; set; }
        public int StudentID { get; set; }
        public int MealType { get; set; }
        public decimal Amount { get; set; } // Amount is NOT the count, it's the amount paid!
        public int SchoolID { get; set; }
        public DateTime DateAndTime { get; set; }
        
        // Stuff that shouldn't be serialized below here
        public string Date
        {
            get { return this.DateAndTime.ToLongDateString(); }
        }
        public string Time
        {
            get { return this.DateAndTime.ToLongTimeString(); }
        }
        public Student Student { get; set; }
        public MealType MealInfo { get; set; }
        public bool Voided { get; set; }
        public string Cost
        {
            get
            {
                if (this.MealInfo != null)
                {
                    return ((Decimal)this.MealInfo.FullAmount).ToString("C");
                }
                else
                {
                    return string.Empty;
                }
            }

        }

        public string Paid
        {
            get
            {
                return ((Decimal)this.Amount).ToString("C");
            }
        }
        
        public override string ToString()
        {
            return "{ PurchasedMeal: ID=" + this.MealID + ",MealType=" + this.MealType + ",Amount="+ this.Amount+",Student=" + this.StudentID + " }";
        }
        
        public bool IsValid()
        {
            return (this.StudentID > 0) &&
                   (this.MealType > 0);
        }
    }
}