namespace DineFlow.Domain.Exceptions;

/// <summary>
/// Custom Exception — được ném khi không tìm thấy resource
/// 
/// [KIẾN THỨC] Tại sao cần Custom Exception?
/// 
/// Không nên ném Exception generic:
///   throw new Exception("Không tìm thấy MenuItem");  ← BAD
/// 
/// Vấn đề: Global Error Handler không biết đây là 404 hay 500
/// 
/// Giải pháp: Custom Exception types → middleware map sang HTTP status code đúng:
///   NotFoundException     → 404 Not Found
///   ValidationException   → 400 Bad Request
///   UnauthorizedException → 401 Unauthorized
///   ForbiddenException    → 403 Forbidden
///   ConflictException     → 409 Conflict
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object id)
        : base($"'{entityName}' với Id '{id}' không tìm thấy.")
    { }

    public NotFoundException(string message) : base(message) { }
}

public class ValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(string field, string error)
        : base("Dữ liệu không hợp lệ.")
    {
        Errors = new Dictionary<string, string[]> { { field, new[] { error } } };
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Dữ liệu không hợp lệ.")
    {
        Errors = new Dictionary<string, string[]>(errors);
    }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "Bạn không có quyền thực hiện thao tác này.")
        : base(message) { }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Xác thực thất bại.") : base(message) { }
}
