namespace imcd_ticket_sync
{
    public class Incident
    {
        public string Number { get; set; }
        public string BriefDescription { get; set; }
        public string Description { get; set; }
        public Operator? Operator { get; set; }
        public OperatorGroup? OperatorGroup { get; set; }
        public ProcessingStatus? ProcessingStatus { get; set; }
    }

    public class Operator
    {
        public string Name { get; set; }
    }

    public class OperatorGroup
    {
        public string Name { get; set; }
    }

    public class ProcessingStatus
    {
        public string Name { get; set; }
    }
}
