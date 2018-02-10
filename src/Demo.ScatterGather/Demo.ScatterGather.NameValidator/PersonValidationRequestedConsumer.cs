using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.ScatterGather.Core.Events;
using MassTransit;

namespace Demo.ScatterGather.NameValidator
{
    public class PersonValidationRequestedConsumer : IConsumer<PersonValidationRequested>
    {
        public async Task Consume(ConsumeContext<PersonValidationRequested> context)
        {
            var errors = new List<string>();

            if (context.Message.Person.FirstName.StartsWith("T", StringComparison.InvariantCultureIgnoreCase))
            {
                errors.Add($"First name '{context.Message.Person.FirstName}' cannot start with a 'T'.");
            }

            context.Respond(new PersonValidated {ValidatorName = typeof(PersonValidationRequestedConsumer).Namespace, Errors = errors.ToArray()});
        }
    }
}