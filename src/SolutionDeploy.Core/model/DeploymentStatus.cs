namespace SolutionDeploy.Core
{
    public enum DeploymentStatus
    {
        Unknown,
        NotStarted,
        Queued,
        PendingApproval,
        InProgress,
        Succeeded,
        Failed,
        Cancelled
    }
}
