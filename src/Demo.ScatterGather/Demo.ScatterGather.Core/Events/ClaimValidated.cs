namespace Demo.ScatterGather.Core.Events
{
    public class ClaimValidated
    {
        public string ValidatorName { get; set; }
        public ClaimValidationError[] Errors { get; set; }
    }
}