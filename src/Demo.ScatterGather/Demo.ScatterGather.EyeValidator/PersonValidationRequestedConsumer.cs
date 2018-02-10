using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Demo.ScatterGather.Core.Events;
using MassTransit;

namespace Demo.ScatterGather.EyeValidator
{
    public class PersonValidationRequestedConsumer : IConsumer<PersonValidationRequested>
    {
        public async Task Consume(ConsumeContext<PersonValidationRequested> context)
        {
            var errors = new List<string>();
            var acceptableEyeColors = new EyeColor[] {EyeColor.Blue, EyeColor.Brown};

            if (acceptableEyeColors.Contains(context.Message.Person.EyeColor) == false)
            {
                errors.Add($"Eye color '{context.Message.Person.EyeColor}' is not an acceptable color.");
            }

            context.Respond(new PersonValidated { ValidatorName = typeof(PersonValidationRequestedConsumer).Namespace, Errors = errors.ToArray() });
        }
    }
}