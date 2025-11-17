using Max.Bot.Types;

namespace Max.Bot.Api;

/// <summary>
/// Interface for user-related API methods.
/// </summary>
public interface IUsersApi
{
    /// <summary>
    /// Gets information about a user by their identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user information.</returns>
    /// <exception cref="ArgumentException">Thrown when userId is less than or equal to zero.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxApiException">Thrown when the API returns an error response.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxNetworkException">Thrown when a network error occurs.</exception>
    /// <exception cref="Max.Bot.Exceptions.MaxUnauthorizedException">Thrown when authentication fails.</exception>
    /// <remarks>
    /// <para>WARNING: This endpoint (/users/{userId}) is not documented in the official MAX API specification and may not be supported by the API.</para>
    /// </remarks>
    [Obsolete("This endpoint (/users/{userId}) is not documented in the MAX API specification and may not be supported.")]
    Task<User> GetUserAsync(long userId, CancellationToken cancellationToken = default);
}

