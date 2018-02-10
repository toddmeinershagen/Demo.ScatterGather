using Automatonymous;

namespace Demo.ScatterGather.Aggregator
{
    class Program
    {
        static void Main(string[] args)
        {
        }
    }


    public class PersonValidation
    {
        public CompositeEventStatus CompositeStatus { get; set; }
        public State CurrentState { get; set; }
    }

    public sealed class PersonValidationStateMachine : AutomatonymousStateMachine<PersonValidation>
    {
        public PersonValidationStateMachine()
        {
            CompositeEvent(() => ValidationComplete, x => x.CompositeStatus, NameValidated, EyesValidated);

            Initially(
                When(Started)
                    .TransitionTo(Waiting));

            During(Waiting,
                When(ValidationComplete)
                    .Finalize());
        }

        public State Waiting { get; private set; }
        public State Complete { get; private set; }

        public Event Started { get; private set; }
        public Event NameValidated { get; private set; }
        public Event EyesValidated { get; private set; }  
        public Event ValidationComplete { get; private set; }    
    }
}
