namespace ReRender.Graph;

public class ResourceUsage
{
    public ResourceUsage(Resource res, ExecutionPlanStep step)
    {
        Resource = res;
        StepTaken = StepGiven = step;
    }

    public Resource Resource { get; }
    public ExecutionPlanStep StepTaken { get; }
    public ExecutionPlanStep StepGiven { get; set; }
    public ResourceAllocation? Allocation { get; set; }
}