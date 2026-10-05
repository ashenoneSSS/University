namespace AutoMarket.Api.Exceptions;

/// <summary>Базове бізнес-виключення. Перетворюється на HTTP-відповідь у <see cref="Middleware.ExceptionHandlingMiddleware"/>.</summary>
public class AppException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

/// <summary>404 — сутність не знайдена.</summary>
public class NotFoundException(string message) : AppException(message, StatusCodes.Status404NotFound);

/// <summary>400 — некоректні дані запиту.</summary>
public class BadRequestException(string message) : AppException(message, StatusCodes.Status400BadRequest);

/// <summary>403 — дія заборонена.</summary>
public class ForbiddenException(string message) : AppException(message, StatusCodes.Status403Forbidden);

/// <summary>401 — не авторизований.</summary>
public class UnauthorizedException(string message) : AppException(message, StatusCodes.Status401Unauthorized);

/// <summary>409 — конфлікт (дублікат тощо).</summary>
public class ConflictException(string message) : AppException(message, StatusCodes.Status409Conflict);
