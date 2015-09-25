using System;
using System.Xml.Serialization;

namespace SLData
{
    public class PurchasedMeal
    {
        public int MealID { get; set; }
        public int StudentID { get; set; }
        public int MealType { get; set; }
        public decimal Amount { get; set; }
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
        
        public override string ToString()
        {
            return "{ PurchasedMeal: ID=" + this.MealID + ",MealType=" + this.MealType + ",Amount="+ this.Amount+",Student=" + this.StudentID + " }";
        }
        
        public bool IsValid()
        {
            return (this.Amount > -2) &&
                   (this.Amount != 0) &&
                   (this.StudentID > 0) &&
                   (this.MealType > 0);
        }
    }
}