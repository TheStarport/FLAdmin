using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Services;
using FlAdmin.Logic.Services;
using FlAdmin.Service.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace FlAdmin.Service.Controllers;

[ApiController]
[Route("api/freelancer")]
[AdminAuthorize(Role.User)]
public class FreelancerDataController(IFreelancerDataService _fldata) : ControllerBase
{
    /// <summary>
    /// Gets a ship archetype by its internal nickname, note this is case sensitive.
    /// </summary>
    /// <param name="nickname"></param>
    /// <returns></returns>
    [HttpGet("shiparch")]
    public IActionResult GetShipArchetype([FromQuery] string nickname)
    {
        var ret = _fldata.GetShip(nickname);

        return ret.Match(s => { return Ok(s); },
            err => err.ParseError(this));
    }

    /// <summary>
    /// Gets a base by its internal nickname.
    /// </summary>
    /// <param name="nickname"></param>
    /// <returns></returns>
    [HttpGet("base")]
    public IActionResult GetBase([FromQuery] string nickname)
    {
        var ret = _fldata.GetBase(nickname);
        return ret.Match(item => { return Ok(item); },
            err => err.ParseError(this));
    }

    /// <summary>
    /// Gets a system by its internal nickname
    /// </summary>
    /// <param name="nickname"></param>
    /// <returns></returns>
    [HttpGet("system")]
    public IActionResult GetSystem([FromQuery] string nickname)
    {
        var ret = _fldata.GetSystem(nickname);
        return ret.Match(item => { return Ok(item); },
            err => err.ParseError(this));
    }

    /// <summary>
    /// Gets a system object by its internal name.
    /// </summary>
    /// <param name="nickname"></param>
    /// <param name="systemNickname">Optional name of the system the object is in, will increase speed of the operation.</param>
    /// <returns></returns>
    [HttpGet("systemobject")]
    public IActionResult GetSystemObject([FromQuery] string nickname, string systemNickname)
    {
        var ret = _fldata.GetSystemObject(nickname, systemNickname);
        return ret.Match(item => { return Ok(item); },
            err => err.ParseError(this));
    }

    /// <summary>
    /// Gets a commodity by its internal nickname
    /// </summary>
    /// <param name="nickname"></param>
    /// <returns> On Success: Commodity, On Failure:Errors of either not found or wrong equipment type.</returns>
    [HttpGet("commodity")]
    public IActionResult GetCommodity([FromQuery] string nickname)
    {
        var ret = _fldata.GetCommodity(nickname);
        return ret.Match(item => { return Ok(item); },
            err => err.ParseError(this));
    }

    /// <summary>
    /// Gets a gun by its internal nickname
    /// </summary>
    /// <param name="nickname"></param>
    /// <returns>On Success: Gun , On Failure: Errors of either not found or wrong equipment type.</returns>
    [HttpGet("gun")]
    public IActionResult GetGun([FromQuery] string nickname)
    {
        var ret = _fldata.GetGun(nickname);
        return ret.Match(item => { return Ok(item); },
            err => err.ParseError(this));
    }

    /// <summary>
    /// Gets a thruster by its internal name
    /// </summary>
    /// <param name="nickname"></param>
    /// <returns>On Success: thruster , On Failure: Errors of either not found or wrong equipment type.</returns>
    [HttpGet("thruster")]
    public IActionResult GetThruster([FromQuery] string nickname)
    {
        var ret = _fldata.GetThruster(nickname);
        return ret.Match(item => { return Ok(item); },
            err => err.ParseError(this));
    }

    /// <summary>
    /// Gets an Engine by its internal name
    /// </summary>
    /// <param name="nickname"></param>
    /// <returns>On Success : Engine , On Failure: Errors of either not found or wrong equipment type.</returns>
    [HttpGet("engine")]
    public IActionResult GetEngine([FromQuery] string nickname)
    {
        var ret = _fldata.GetEngine(nickname);
        return ret.Match(item => { return Ok(item); },
            err => err.ParseError(this));
    }

    /// <summary>
    /// Gets a countermeasure by its internal nickname
    /// </summary>
    /// <param name="nickname"></param>
    /// <returns>On Success: countermeasure, On Failure: Errors of either not found or wrong equipment type.</returns>
    [HttpGet("countermeasure")]
    public IActionResult GetCountermeasure([FromQuery] string nickname)
    {
        var ret = _fldata.GetCountermeasure(nickname);
        return ret.Match(item => { return Ok(item); },
            err => err.ParseError(this));
    }

    /// <summary>
    /// Gets a countermeasure dropper by its internal nickname
    /// </summary>
    /// <param name="nickname"></param>
    /// <returns>On Success: Countermeasure Dropper, On Failure: Errors of either not found or wrong equipment type.</returns>
    [HttpGet("countermeasuredropper")]
    public IActionResult GetCountermeasuredropper([FromQuery] string nickname)
    {
        var ret = _fldata.GetCountermeasureDropper(nickname);
        return ret.Match(item => { return Ok(item); },
            err => err.ParseError(this));
    }

    /// <summary>
    /// Gets a mine by its internal nickname.
    /// </summary>
    /// <param name="nickname"></param>
    /// <returns>On Success: Mine, On Failure: Not Found</returns>
    [HttpGet("mine")]
    public IActionResult GetMine([FromQuery] string nickname)
    {
        var ret = _fldata.GetMine(nickname);
        return ret.Match(item => { return Ok(item); },
            err => err.ParseError(this));
    }

    /// <summary>
    /// Gets a mine dropper by its internal nickname
    /// </summary>
    /// <param name="nickname"></param>
    /// <returns>On Success: Mine Dropper, On Failure: Errors of either not found or wrong equipment type.</returns>
    [HttpGet("minedropper")]
    public IActionResult GetMinedropper([FromQuery] string nickname)
    {
        var ret = _fldata.GetMineDropper(nickname);
        return ret.Match(item => { return Ok(item); },
            err => err.ParseError(this));
    }

    /// <summary>
    /// Gets a munition by its internal nickname
    /// </summary>
    /// <param name="nickname"></param>
    /// <returns>On Success: Munition: On Failure: Not Found.</returns>
    [HttpGet("munition")]
    public IActionResult GetMunition([FromQuery] string nickname)
    {
        var ret = _fldata.GetMunition(nickname);
        return ret.Match(item => { return Ok(item); },
            err => err.ParseError(this));
    }

    /// <summary>
    /// Gets a motor by its internal nickname
    /// </summary>
    /// <param name="nickname"></param>
    /// <returns>On Success: Motor, On Failure: Not Found.</returns>
    [HttpGet("motor")]
    public IActionResult GetMotor([FromQuery] string nickname)
    {
        var ret = _fldata.GetMotor(nickname);
        return ret.Match(item => { return Ok(item); },
            err => err.ParseError(this));
    }

    /// <summary>
    /// Gets a powercore by its internal nickname
    /// </summary>
    /// <param name="nickname"></param>
    /// <returns>On Success: Power Core, On Failure: Either not found or wrong equipment type.</returns>
    [HttpGet("powercore")]
    public IActionResult GetPowerCore([FromQuery] string nickname)
    {
        var ret = _fldata.GetPowerCore(nickname);
        return ret.Match(item => { return Ok(item); },
            err => err.ParseError(this));
    }

    /// <summary>
    /// Gets a shield generator by its internal nickname
    /// </summary>
    /// <param name="nickname"></param>
    /// <returns>On Success: Shield Generator, On Failure: Either not found or wrong equipment type.</returns>
    [HttpGet("shieldgenerator")]
    public IActionResult GetShieldGenerator([FromQuery] string nickname)
    {
        var ret = _fldata.GetShieldGenerator(nickname);
        return ret.Match(item => { return Ok(item); },
            err => err.ParseError(this));
    }

    /// <summary>
    /// Gets a scanner by its internal nickname
    /// </summary>
    /// <param name="nickname"></param>
    /// <returns>On Success: Scanner, On Failure: Either not found or wrong equipment type.</returns>
    [HttpGet("scanner")]
    public IActionResult GetScanner([FromQuery] string nickname)
    {
        var ret = _fldata.GetScanner(nickname);
        return ret.Match(item => { return Ok(item); },
            err => err.ParseError(this));
    }

    /// <summary>
    /// Gets a tractor by its internal nickname
    /// </summary>
    /// <param name="nickname"></param>
    /// <returns>On Success: Tractor, On Failure: Either not found or wrong equipment type.</returns>
    [HttpGet("tractor")]
    public IActionResult GetTractor([FromQuery] string nickname)
    {
        var ret = _fldata.GetTractor(nickname);
        return ret.Match(item => { return Ok(item); },
            err => err.ParseError(this));
    }

    /// <summary>
    /// Gets an armor by its internal nickname
    /// </summary>
    /// <param name="nickname"></param>
    /// <returns>On Success: Armor, On Failure: Either not found or wrong equipment type.</returns>
    [HttpGet("armor")]
    public IActionResult GetArmor([FromQuery] string nickname)
    {
        var ret = _fldata.GetArmor(nickname);
        return ret.Match(item => { return Ok(item); },
            err => err.ParseError(this));
    }

    /// <summary>
    /// Gets a cargopod by its internal nickname
    /// </summary>
    /// <param name="nickname"></param>
    /// <returns>On Success: Armor, On Failure: Either not found or wrong equipment type.</returns>
    [HttpGet("cargopod")]
    public IActionResult GetCargopod([FromQuery] string nickname)
    {
        var ret = _fldata.GetCargoPod(nickname);
        return ret.Match(item => { return Ok(item); },
            err => err.ParseError(this));
    }

    /// <summary>
    /// Gets a basegood list of a base by its internal nickname
    /// </summary>
    /// <param name="nickname">internal nickname of the base to grab its basegood from</param>
    /// <returns>On Success: Base Good, On Failure: Not Found</returns>
    [HttpGet("basegood")]
    public IActionResult GetBaseGood([FromQuery] string nickname)
    {
        var ret = _fldata.GetBaseGood(nickname);
        return ret.Match(item => { return Ok(item); },
            err => err.ParseError(this));
    }

    /// <summary>
    /// Gets a good by its internal nickname
    /// </summary>
    /// <param name="nickname"></param>
    /// <returns>On Success: Good, On Failure: Not Found</returns>
    [HttpGet("good")]
    public IActionResult GetGood([FromQuery] string nickname)
    {
        var ret = _fldata.GetGood(nickname);
        return ret.Match(item => { return Ok(item); },
            err => err.ParseError(this));
    }
}