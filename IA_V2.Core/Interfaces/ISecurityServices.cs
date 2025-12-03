using IA_V2.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IA_V2.Core.Interfaces
{
    public interface ISecurityServices
    {
        Task<Security> GetLoginByCredentials(UserLogin login);

        Task RegisterUser(Security security);
    }
}
