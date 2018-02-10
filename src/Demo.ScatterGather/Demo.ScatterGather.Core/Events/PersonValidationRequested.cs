using System;

namespace Demo.ScatterGather.Core.Events
{
    public class PersonValidationRequested
    {
        public Guid RequestId { get; set; }
        public Person Person { get; set; }
    }

    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public EyeColor EyeColor { get; set; }
        public decimal WeightInPounds { get; set; }
        public DateTime BirthDate { get; set; }
    }

    public enum EyeColor
    {
        Blue = 1,
        Brown = 2,
        Gray = 3,
        Green = 4
    }
}
