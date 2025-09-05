namespace Papel.Integration.Common.Extensions;

using Model;

public static class ApiResponseExtensions
{
    public static ApiResponse<T> Success<T>(T data, string message = "Başarılı")
    {
        return new ApiResponse<T>
        {
            Data = data,
            Success = true,
            Message = message
        };
    }

    public static ApiResponse Success(string message = "Başarılı")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message
        };
    }

    public static ApiResponse<T> Failure<T>(string message, T? data = default)
    {
        return new ApiResponse<T>
        {
            Data = data,
            Success = false,
            Message = message
        };
    }

    public static ApiResponse Failure(string message)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message
        };
    }

    public static ApiResponse<T> ValidationError<T>(string message, T? data = default)
    {
        return new ApiResponse<T>
        {
            Data = data,
            Success = false,
            Message = message
        };
    }

    public static ApiResponse ValidationError(string message)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message
        };
    }

    public static ApiResponse<T> Conflict<T>(string message = "Bu kayıt daha önce işlenmiştir.", T? data = default)
    {
        return new ApiResponse<T>
        {
            Data = data,
            Success = false,
            Message = message
        };
    }

    public static ApiResponse Conflict(string message = "Bu kayıt daha önce işlenmiştir.")
    {
        return new ApiResponse
        {
            Success = false,
            Message = message
        };
    }
}