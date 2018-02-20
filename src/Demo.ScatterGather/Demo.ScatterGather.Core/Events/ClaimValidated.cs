namespace Demo.ScatterGather.Core.Events
{
    public class ClaimValidated
    {
        public string ValidatorName { get; set; }
        public string[] Errors { get; set; }
    }
}