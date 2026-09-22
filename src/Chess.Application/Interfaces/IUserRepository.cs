using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<string?> GetUsernameByIdAsync(string userId);
    }
}
