namespace Demo.ScatterGather.Core.Events
{
    public class PersonValidated
    {
        public string ValidatorName { get; set; }
        public string[] Errors { get; set; }
    }
}