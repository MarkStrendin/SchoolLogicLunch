namespace SLData
{
    public class Student
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MedicalNotes { get; set; }
        public string StudentNumber { get; set; }
        public decimal Bank { get; set; }

        public string DisplayName
        {
            get { return this.FirstName + " " + this.LastName + " (" + this.StudentNumber + ")"; }
        }

        public override string ToString()
        {
            return "{ ID:" + this.ID + ", StudentNumber:" + this.StudentNumber + ", FirstName:" + this.FirstName +
                   ", LastName:" + this.LastName + " }";
        }

        public Student() { }
    }
}
