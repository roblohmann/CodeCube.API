using System.Net;

namespace CodeCube.API;

public sealed class OperationInfo
{
    public int Status { get; set; } = (int)HttpStatusCode.OK;
    public string Url { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}