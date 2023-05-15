namespace ConsoleAppJSONSerializer
{
    public class Staffer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal SalaryPerHour { get; set; }
        public Staffer(int id, string firstName, string lastName, decimal salaryPerHour)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            SalaryPerHour = salaryPerHour;
        }

        public override string ToString() =>
            $"Id = {Id}, FirstName = {FirstName}, LastName = {LastName}, " +
            $"SalaryPerHour = {SalaryPerHour}";

        public override bool Equals(object? obj)
        {
            Staffer other = obj as Staffer;
            return Id == other?.Id &&
                FirstName == other.FirstName &&
                LastName == other.LastName &&
                SalaryPerHour == other.SalaryPerHour;
        }
    }
}
