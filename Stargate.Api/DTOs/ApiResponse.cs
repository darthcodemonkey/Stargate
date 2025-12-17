using System.Net;

namespace StargateAPI.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = "Successful";
    public int ResponseCode { get; set; } = (int)HttpStatusCode.OK;
    public T? Data { get; set; }
}

public class ApiResponse : ApiResponse<object>
{
}

