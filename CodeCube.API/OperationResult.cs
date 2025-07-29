namespace CodeCube.API;

public sealed class OperationResult<TResult> where TResult : new()
{
    public OperationInfo OperationInfo { get; set; }
    
    public TResult Body { get; set; }
}