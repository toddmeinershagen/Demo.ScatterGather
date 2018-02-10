﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.ScatterGather.Core.Events;
using MassTransit;

namespace Demo.ScatterGather.WeightValidator
{
    public class PersonValidationRequestedConsumer : IConsumer<PersonValidationRequested>
    {
        public async Task Consume(ConsumeContext<PersonValidationRequested> context)
        {
            var errors = new List<string>();

            if (context.Message.Person.WeightInPounds < 100)
            {
                errors.Add($"Weight of {context.Message.Person.WeightInPounds} lbs. cannot be under 100.");
            }

            context.Respond(new PersonValidated { ValidatorName = typeof(PersonValidationRequestedConsumer).Namespace, Errors = errors.ToArray() });
        }
    }
}