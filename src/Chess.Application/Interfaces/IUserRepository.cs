using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Application.Interfaces
{
    /// <summary>
    /// Provides persistence operations for <see cref="ApplicationUser"/>s.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Retrieves the username associated with a <see cref="ApplicationUser"/>'s unique identifier.
        /// </summary>
        /// <param name="userId">The unique identifier of the <see cref="ApplicationUser"/>.</param>
        /// <returns>
        /// The username associated with the specified <see cref="ApplicationUser"/>, or 
        /// <see langword="null"/> if the user does not exist.
        /// </returns>
        Task<string?> GetUsernameByIdAsync(string userId);
    }
}
