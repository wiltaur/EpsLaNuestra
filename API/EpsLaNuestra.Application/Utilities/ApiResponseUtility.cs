namespace EpsLaNuestra.Application.Utilities;

/// <summary>
/// Generic responses for all requests.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ApiResponseUtility<T>(T data)
{
    public T Data { get; set; } = data;
    public bool IsSuccess { get; set; } = true;
    public string ReturnMessage { get; set; } = string.Empty;
}