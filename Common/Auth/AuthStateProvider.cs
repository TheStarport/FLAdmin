using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Auth;
public abstract class AuthStateProvider : AuthenticationStateProvider
{
    public abstract bool Authenticate(string jwt);
    public abstract Task<string> RegenerateTokenAsync();
}
