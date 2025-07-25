using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Models.Payloads;
using FlAdmin.Logic.Services;
using FlAdmin.Service.Extensions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace FlAdmin.Service.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController(IDatabaseAccess database) : ControllerBase
{
    [HttpPost]
    [Route("startatabasesession")]
    [AdminAuthorize(Role.Database)]
    public async Task<IActionResult> StartDatabaseSession()
    {
        var res = await database.StartSession();

        return res.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: guid => Ok(guid));
    }

    [HttpPost]
    [Route("querydatabase")]
    [AdminAuthorize(Role.Database)]
    public async Task<IActionResult> DatabaseQuery([FromBody] CommandPayload command)
    {
        var res  = await database.SubmitQuery(command.Command, command.SessionId);

        return res.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: doc => Ok(doc)
        );
    }

    [HttpPost]
    [Route("enddatabasesession")]
    [AdminAuthorize(Role.Database)]
    public async Task<IActionResult> EndDatabaseSession([FromQuery] bool commit)
    {
        var res = await database.EndSession(commit);
        return res.Match<IActionResult>(
            Some: err => err.ParseError(this),
            None: () => Ok()
        );
    }
}