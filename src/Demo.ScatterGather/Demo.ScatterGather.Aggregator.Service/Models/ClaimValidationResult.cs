using System;
using System.Collections.Generic;
using Demo.ScatterGather.Core.Events;

namespace Demo.ScatterGather.Aggregator.Service.Models
{
    public class ClaimValidationResult
    {
        public ClaimValidationResult()
        {
            Results = new List<ClaimValidated>();
        }

        public Guid RequestId { get; set; }
        public List<ClaimValidated> Results { get; set; }
    }
}