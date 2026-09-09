namespace VehicleParkingManagementSystem.Services;

public class ServiceResult
{
    public bool Succeeded { get; init; }
    public string? Error { get; init; }

    public static ServiceResult Success() => new() { Succeeded = true };
    public static ServiceResult Failure(string error) => new() { Succeeded = false, Error = error };
}

public class ServiceResult<T>
{
    public bool Succeeded { get; init; }
    public string? Error { get; init; }
    public T? Value { get; init; }

    public static ServiceResult<T> Success(T value) => new() { Succeeded = true, Value = value };
    public static ServiceResult<T> Failure(string error) => new() { Succeeded = false, Error = error };
}
