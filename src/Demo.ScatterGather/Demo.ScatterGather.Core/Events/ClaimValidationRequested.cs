using System;
using System.Collections.Generic;

namespace Demo.ScatterGather.Core.Events
{
    public class ClaimValidationRequested
    {
        public Guid RequestId { get; set; }
        public Claim Claim { get; set; }
    }

    public class Claim
    {
        public Patient Patient { get; set; }
        public List<Service> Services { get; set; }
    }

    public class Patient
    {
        public Gender Gender { get; set; }
        public DateTime Birthdate { get; set; }
    }

    public enum Gender
    {
        Male,
        Female,
        Unknown
    }

    public class Service
    {
        public DateTime ServiceDate { get; set; }
        public string Code { get; set; }
        public CodeType CodeType { get; set; }
    }

    public enum CodeType
    {
        Cpt,
        Icd9,
        Icd10
    }
}
