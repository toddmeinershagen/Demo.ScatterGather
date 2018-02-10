using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.ScatterGather.Core.Events;
using MassTransit;

namespace Demo.ScatterGather.BirthDateValidator
{
    public class PersonValidationRequestedConsumer : IConsumer<PersonValidationRequested>
    {
        public async Task Consume(ConsumeContext<PersonValidationRequested> context)
        {
            var errors = new List<string>();

            if (context.Message.Person.BirthDate > new DateTime(1980, 1, 1))
            {
                errors.Add($"Birthdate of {context.Message.Person.BirthDate.ToShortDateString()} should be greater than 1/1/1980.");
            }

            context.Respond(new PersonValidated { ValidatorName = typeof(PersonValidationRequestedConsumer).Namespace, Errors = errors.ToArray() });
        }
    }
}