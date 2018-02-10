using System;
using System.Collections.Generic;
using Demo.ScatterGather.Core.Events;

namespace Demo.ScatterGather.Aggregator.Service.Models
{
    public class PersonValidationResult
    {
        public PersonValidationResult()
        {
            Results = new List<PersonValidated>();
        }

        public Guid RequestId { get; set; }
        public List<PersonValidated> Results { get; set; }

    }
}