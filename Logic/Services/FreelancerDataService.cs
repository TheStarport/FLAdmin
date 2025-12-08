using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models;
using FlAdmin.Common.Models.Error;
using FlAdmin.Common.Services;
using LanguageExt;
using LibreLancer.Data;
using LibreLancer.Data.Equipment;
using LibreLancer.Data.Goods;
using LibreLancer.Data.Ships;
using LibreLancer.Data.Universe;
using Microsoft.Extensions.Logging;

namespace FlAdmin.Logic.Services;

public class FreelancerDataService : IFreelancerDataService
{
    private ILogger<FreelancerDataService> _logger;
    private readonly FreelancerData _freelancerData;


    public FreelancerDataService(ILogger<FreelancerDataService> logger, IFreelancerDataProvider flData)
    {
        _logger = logger;

        try
        {
            if (flData.Loaded())
                _freelancerData = flData.GetFreelancerData() ?? throw new InvalidOperationException();
            else throw new Exception("FreelancerDataService: FreelancerDataService failed to load");

            if (_freelancerData is null)
                throw new Exception("FreelancerDataService: FreelancerDataService failed to load");
        }
        catch (Exception e)
        {
            _logger.LogCritical("{}", e.Message);
        }
    }

    public Either<ErrorResult, Ship> GetShip(Either<uint, string> id)
    {
        return id.Match<Either<ErrorResult, Ship>>(s =>
            {
                var ship = _freelancerData.Ships.Ships.Find(ship => ship.Nickname == s);
                if (ship is not null)
                {
                    return ship;
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A shipArch entry with the nickname {s}");
                }
            },
            u =>
            {
                var ship = _freelancerData.Ships.Ships.Find(ship => LibreLancer.FLHash.CreateID(ship.Nickname) == u);
                if (ship is not null)
                {
                    return ship;
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A shipArch entry with an id of {u}");
                }
            });
    }

    public Either<ErrorResult, Base> GetBase(Either<uint, string> id)
    {
        return id.Match<Either<ErrorResult, Base>>(s =>
            {
                var b = _freelancerData.Universe.Bases.Find(b => b.Nickname == s);
                if (b is not null)
                {
                    return b;
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A base entry with the nickname {s} was not found");
                }
            },
            u =>
            {
                var b = _freelancerData.Universe.Bases.Find(b => LibreLancer.FLHash.CreateID(b.Nickname) == u);
                if (b is not null)
                {
                    return b;
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A base entry with an id of {u} was not found");
                }
            });
    }

    public Either<ErrorResult, StarSystem> GetSystem(Either<uint, string> id)
    {
        return id.Match<Either<ErrorResult, StarSystem>>(s =>
            {
                var system = _freelancerData.Universe.Systems.Find(system => system.Nickname == s);
                if (system is not null)
                {
                    return system;
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A system with the nickname {s} was not found");
                }
            },
            u =>
            {
                var system =
                    _freelancerData.Universe.Systems.Find(system => LibreLancer.FLHash.CreateID(system.Nickname) == u);
                if (system is not null)
                {
                    return system;
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A system with an id of {u} was not found");
                }
            });
    }

    public Either<ErrorResult, SystemObject> GetSystemObject(Either<uint, string> id, string systemNickname)
    {
        if (systemNickname != "none")
        {
            var system = _freelancerData.Universe.Systems.Find(system => system.Nickname == systemNickname);
            if (system is null)
            {
                return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                    $"Provided system nickname of {systemNickname} does not exist.");
            }

            return id.Match<Either<ErrorResult, SystemObject>>(s =>
                {
                    var sysobj = system.Objects.Find(sysobj => sysobj.Nickname == s);
                    if (sysobj is not null)
                    {
                        return sysobj;
                    }

                    {
                        return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                            $"A system object with the nickname {s} was not found");
                    }
                },
                u =>
                {
                    var sysobj =
                        system.Objects.Find(sysobj =>
                            LibreLancer.FLHash.CreateID(sysobj.Nickname) == u);
                    if (sysobj is not null)
                    {
                        return sysobj;
                    }

                    {
                        return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                            $"A system object with an id of {u} was not found");
                    }
                });
        }
        else
        {
            return id.Match<Either<ErrorResult, SystemObject>>(s =>
                {
                    foreach (var sys in _freelancerData.Universe.Systems)
                    {
                        var sysobj = sys.Objects.Find(sysobj => sysobj.Nickname == s);
                        if (sysobj is not null)
                        {
                            return sysobj;
                        }
                    }

                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A system object with a nickname of {s} was not found");
                },
                u =>
                {
                    foreach (var sys in _freelancerData.Universe.Systems)
                    {
                        var sysobj = sys.Objects.Find(sysobj => LibreLancer.FLHash.CreateID(sysobj.Nickname) == u);
                        if (sysobj is not null)
                        {
                            return sysobj;
                        }
                    }

                    {
                        return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                            $"A system with an id of {u} was not found");
                    }
                });
        }
    }

    public Either<ErrorResult, Commodity> GetCommodity(Either<uint, string> id)
    {
        return id.Match<Either<ErrorResult, Commodity>>(s =>
            {
                var commodity = _freelancerData.Equipment.Equip.Find(equip => equip.Nickname == s);
                if (commodity != null && commodity.GetType() == typeof(Commodity))
                {
                    return (Commodity)commodity;
                }

                if (commodity != null && commodity.GetType() != typeof(Commodity))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided name of {s} was not of type Commodity but of type {commodity.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A commodity with the name of {s} was not found.");
                }
            },
            u =>
            {
                var commodity =
                    _freelancerData.Equipment.Equip.Find(equip => LibreLancer.FLHash.CreateID(equip.Nickname) == u);
                if (commodity != null && commodity.GetType() == typeof(Commodity))
                {
                    return (Commodity)commodity;
                }

                if (commodity != null && commodity.GetType() != typeof(Commodity))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided id of {u} was not of type Commodity but of type {commodity.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A commodity with the id of {u} was not found");
                }
            });
    }

    public Either<ErrorResult, Gun> GetGun(Either<uint, string> id)
    {
        return id.Match<Either<ErrorResult, Gun>>(s =>
            {
                var gun = _freelancerData.Equipment.Equip.Find(equip => equip.Nickname == s);
                if (gun != null && gun.GetType() == typeof(Gun))
                {
                    return (Gun)gun;
                }

                if (gun != null && gun.GetType() != typeof(Gun))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided name of {s} was not of type gun but of type {gun.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A gun with the name of {s} was not found.");
                }
            },
            u =>
            {
                var gun =
                    _freelancerData.Equipment.Equip.Find(equip => LibreLancer.FLHash.CreateID(equip.Nickname) == u);
                if (gun != null && gun.GetType() == typeof(Gun))
                {
                    return (Gun)gun;
                }

                if (gun != null && gun.GetType() != typeof(Gun))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided id of {u} was not of type gun but of type {gun.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A gun with the id of {u} was not found");
                }
            });
    }

    public Either<ErrorResult, Thruster> GetThruster(Either<uint, string> id)
    {
        return id.Match<Either<ErrorResult, Thruster>>(s =>
            {
                var thruster = _freelancerData.Equipment.Equip.Find(equip => equip.Nickname == s);
                if (thruster != null && thruster.GetType() == typeof(Thruster))
                {
                    return (Thruster)thruster;
                }

                if (thruster != null && thruster.GetType() != typeof(Thruster))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided name of {s} was not of type thruster but of type {thruster.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A thruster with the name of {s} was not found.");
                }
            },
            u =>
            {
                var thruster =
                    _freelancerData.Equipment.Equip.Find(equip => LibreLancer.FLHash.CreateID(equip.Nickname) == u);
                if (thruster != null && thruster.GetType() == typeof(Thruster))
                {
                    return (Thruster)thruster;
                }

                if (thruster != null && thruster.GetType() != typeof(Thruster))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided id of {u} was not of type thruster but of type {thruster.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A thruster with the id of {u} was not found");
                }
            });
    }

    public Either<ErrorResult, Engine> GetEngine(Either<uint, string> id)
    {
        return id.Match<Either<ErrorResult, Engine>>(s =>
            {
                var engine = _freelancerData.Equipment.Equip.Find(equip => equip.Nickname == s);
                if (engine != null && engine.GetType() == typeof(Engine))
                {
                    return (Engine)engine;
                }

                if (engine != null && engine.GetType() != typeof(Engine))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided name of {s} was not of type thruster but of type {engine.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A thruster with the name of {s} was not found.");
                }
            },
            u =>
            {
                var engine =
                    _freelancerData.Equipment.Equip.Find(equip => LibreLancer.FLHash.CreateID(equip.Nickname) == u);
                if (engine != null && engine.GetType() == typeof(Thruster))
                {
                    return (Engine)engine;
                }

                if (engine != null && engine.GetType() != typeof(Engine))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided id of {u} was not of type engine but of type {engine.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A engine with the id of {u} was not found");
                }
            });
    }

    public Either<ErrorResult, Countermeasure> GetCountermeasure(Either<uint, string> id)
    {
        return id.Match<Either<ErrorResult, Countermeasure>>(s =>
            {
                var counterMeasure = _freelancerData.Equipment.Equip.Find(equip => equip.Nickname == s);
                if (counterMeasure != null && counterMeasure.GetType() == typeof(Countermeasure))
                {
                    return (Countermeasure)counterMeasure;
                }

                if (counterMeasure != null && counterMeasure.GetType() != typeof(Countermeasure))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided name of {s} was not of type Counter Measure but of type {counterMeasure.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A Counter Measure Dropper with the name of {s} was not found.");
                }
            },
            u =>
            {
                var counterMeasure =
                    _freelancerData.Equipment.Equip.Find(equip => LibreLancer.FLHash.CreateID(equip.Nickname) == u);
                if (counterMeasure != null && counterMeasure.GetType() == typeof(Countermeasure))
                {
                    return (Countermeasure)counterMeasure;
                }

                if (counterMeasure != null && counterMeasure.GetType() != typeof(Countermeasure))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided id of {u} was not of type Counter Measure but of type {counterMeasure.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A Counter Measure with the id of {u} was not found");
                }
            });
    }

    public Either<ErrorResult, CountermeasureDropper> GetCountermeasureDropper(Either<uint, string> id)
    {
        return id.Match<Either<ErrorResult, CountermeasureDropper>>(s =>
            {
                var counterMeasureDropper = _freelancerData.Equipment.Equip.Find(equip => equip.Nickname == s);
                if (counterMeasureDropper != null && counterMeasureDropper.GetType() == typeof(CountermeasureDropper))
                {
                    return (CountermeasureDropper)counterMeasureDropper;
                }

                if (counterMeasureDropper != null && counterMeasureDropper.GetType() != typeof(CountermeasureDropper))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided name of {s} was not of type Counter Measure Dropper but of type {counterMeasureDropper.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A Counter Measure Dropper with the name of {s} was not found.");
                }
            },
            u =>
            {
                var counterMeasureDropper =
                    _freelancerData.Equipment.Equip.Find(equip => LibreLancer.FLHash.CreateID(equip.Nickname) == u);
                if (counterMeasureDropper != null && counterMeasureDropper.GetType() == typeof(CountermeasureDropper))
                {
                    return (CountermeasureDropper)counterMeasureDropper;
                }

                if (counterMeasureDropper != null && counterMeasureDropper.GetType() != typeof(CountermeasureDropper))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided id of {u} was not of type Counter Measure Dropper but of type {counterMeasureDropper.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A Counter Measure Dropper with the id of {u} was not found");
                }
            });
    }

    public Either<ErrorResult, Mine> GetMine(Either<uint, string> id)
    {
        return id.Match<Either<ErrorResult, Mine>>(s =>
            {
                var mine = _freelancerData.Equipment.Mines.Find(mine => mine.Nickname == s);
                if (mine != null)
                {
                    return mine;
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A munition with the name of {s} was not found.");
                }
            },
            u =>
            {
                var mine = _freelancerData.Equipment.Mines.Find(mine =>
                    LibreLancer.FLHash.CreateID(mine.Nickname) == u);
                if (mine != null)
                {
                    return mine;
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A munition with the name of {u} was not found.");
                }
            });
    }

    public Either<ErrorResult, MineDropper> GetMineDropper(Either<uint, string> id)
    {
        return id.Match<Either<ErrorResult, MineDropper>>(s =>
            {
                var mineDropper = _freelancerData.Equipment.Equip.Find(equip => equip.Nickname == s);
                if (mineDropper != null && mineDropper.GetType() == typeof(MineDropper))
                {
                    return (MineDropper)mineDropper;
                }

                if (mineDropper != null && mineDropper.GetType() != typeof(MineDropper))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided name of {s} was not of type Mine Dropper but of type {mineDropper.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A Mine Dropper with the name of {s} was not found.");
                }
            },
            u =>
            {
                var mineDropper =
                    _freelancerData.Equipment.Equip.Find(equip => LibreLancer.FLHash.CreateID(equip.Nickname) == u);
                if (mineDropper != null && mineDropper.GetType() == typeof(MineDropper))
                {
                    return (MineDropper)mineDropper;
                }

                if (mineDropper != null && mineDropper.GetType() != typeof(CountermeasureDropper))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided id of {u} was not of type Mine Dropper but of type {mineDropper.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A Mine Dropper with the id of {u} was not found");
                }
            });
    }

    public Either<ErrorResult, Munition> GetMunition(Either<uint, string> id)
    {
        return id.Match<Either<ErrorResult, Munition>>(s =>
            {
                var munition = _freelancerData.Equipment.Munitions.Find(munition => munition.Nickname == s);
                if (munition != null)
                {
                    return munition;
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A munition with the name of {s} was not found.");
                }
            },
            u =>
            {
                var munition = _freelancerData.Equipment.Munitions.Find(motor =>
                    LibreLancer.FLHash.CreateID(motor.Nickname) == u);
                if (munition != null)
                {
                    return munition;
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A munition with the name of {u} was not found.");
                }
            });
    }

    public Either<ErrorResult, Motor> GetMotor(Either<uint, string> id)
    {
        return id.Match<Either<ErrorResult, Motor>>(s =>
            {
                var motor = _freelancerData.Equipment.Motors.Find(motor => motor.Nickname == s);
                if (motor != null)
                {
                    return motor;
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A motor with the name of {s} was not found.");
                }
            },
            u =>
            {
                var motor = _freelancerData.Equipment.Motors.Find(motor =>
                    LibreLancer.FLHash.CreateID(motor.Nickname) == u);
                if (motor != null)
                {
                    return motor;
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A motor with the name of {u} was not found.");
                }
            });
    }

    public Either<ErrorResult, PowerCore> GetPowerCore(Either<uint, string> id)
    {
        return id.Match<Either<ErrorResult, PowerCore>>(s =>
            {
                var powerCore = _freelancerData.Equipment.Equip.Find(equip => equip.Nickname == s);
                if (powerCore != null && powerCore.GetType() == typeof(PowerCore))
                {
                    return (PowerCore)powerCore;
                }

                if (powerCore != null && powerCore.GetType() != typeof(PowerCore))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided name of {s} was not of type Power Core but of type {powerCore.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A Power Core with the name of {s} was not found.");
                }
            },
            u =>
            {
                var powerCore =
                    _freelancerData.Equipment.Equip.Find(equip => LibreLancer.FLHash.CreateID(equip.Nickname) == u);
                if (powerCore != null && powerCore.GetType() == typeof(PowerCore))
                {
                    return (PowerCore)powerCore;
                }

                if (powerCore != null && powerCore.GetType() != typeof(PowerCore))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided id of {u} was not of type Power Core but of type {powerCore.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A Power Core with the id of {u} was not found");
                }
            });
    }

    public Either<ErrorResult, ShieldGenerator> GetShieldGenerator(Either<uint, string> id)
    {
        return id.Match<Either<ErrorResult, ShieldGenerator>>(s =>
            {
                var shieldGenerator = _freelancerData.Equipment.Equip.Find(equip => equip.Nickname == s);
                if (shieldGenerator != null && shieldGenerator.GetType() == typeof(ShieldGenerator))
                {
                    return (ShieldGenerator)shieldGenerator;
                }

                if (shieldGenerator != null && shieldGenerator.GetType() != typeof(ShieldGenerator))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided name of {s} was not of type Shield Generator but of type {shieldGenerator.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A shield generator with the name of {s} was not found.");
                }
            },
            u =>
            {
                var shieldGenerator =
                    _freelancerData.Equipment.Equip.Find(equip => LibreLancer.FLHash.CreateID(equip.Nickname) == u);
                if (shieldGenerator != null && shieldGenerator.GetType() == typeof(ShieldGenerator))
                {
                    return (ShieldGenerator)shieldGenerator;
                }

                if (shieldGenerator != null && shieldGenerator.GetType() != typeof(ShieldGenerator))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided id of {u} was not of type Shield Generator but of type {shieldGenerator.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A Shield Generator with the id of {u} was not found");
                }
            });
    }

    public Either<ErrorResult, Scanner> GetScanner(Either<uint, string> id)
    {
        return id.Match<Either<ErrorResult, Scanner>>(s =>
            {
                var scanner = _freelancerData.Equipment.Equip.Find(equip => equip.Nickname == s);
                if (scanner != null && scanner.GetType() == typeof(Scanner))
                {
                    return (Scanner)scanner;
                }

                if (scanner != null && scanner.GetType() != typeof(Scanner))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided name of {s} was not of type scanner but of type {scanner.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A scanner with the name of {s} was not found.");
                }
            },
            u =>
            {
                var scanner =
                    _freelancerData.Equipment.Equip.Find(equip => LibreLancer.FLHash.CreateID(equip.Nickname) == u);
                if (scanner != null && scanner.GetType() == typeof(Scanner))
                {
                    return (Scanner)scanner;
                }

                if (scanner != null && scanner.GetType() != typeof(Scanner))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided id of {u} was not of type scanner but of type {scanner.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A scanner with the id of {u} was not found");
                }
            });
    }

    public Either<ErrorResult, Tractor> GetTractor(Either<uint, string> id)
    {
        return id.Match<Either<ErrorResult, Tractor>>(s =>
            {
                var tractor = _freelancerData.Equipment.Equip.Find(equip => equip.Nickname == s);
                if (tractor != null && tractor.GetType() == typeof(Tractor))
                {
                    return (Tractor)tractor;
                }

                if (tractor != null && tractor.GetType() != typeof(Tractor))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided name of {s} was not of type tractor but of type {tractor.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A tractor with the name of {s} was not found.");
                }
            },
            u =>
            {
                var tractor =
                    _freelancerData.Equipment.Equip.Find(equip => LibreLancer.FLHash.CreateID(equip.Nickname) == u);
                if (tractor != null && tractor.GetType() == typeof(Tractor))
                {
                    return (Tractor)tractor;
                }

                if (tractor != null && tractor.GetType() != typeof(Tractor))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided id of {u} was not of type tractor but of type {tractor.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A tractor with the id of {u} was not found");
                }
            });
    }

    public Either<ErrorResult, ShieldBattery> GetShieldBattery(Either<uint, string> id)
    {
        throw new NotImplementedException();
    }

    public Either<ErrorResult, RepairKit> GetRepairKit(Either<uint, string> id)
    {
        throw new NotImplementedException();
    }

    public Either<ErrorResult, Armor> GetArmor(Either<uint, string> id)
    {
        return id.Match<Either<ErrorResult, Armor>>(s =>
            {
                var armor = _freelancerData.Equipment.Equip.Find(equip => equip.Nickname == s);
                if (armor != null && armor.GetType() == typeof(Armor))
                {
                    return (Armor)armor;
                }

                if (armor != null && armor.GetType() != typeof(Armor))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided name of {s} was not of type armor but of type {armor.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A armor with the name of {s} was not found.");
                }
            },
            u =>
            {
                var armor =
                    _freelancerData.Equipment.Equip.Find(equip => LibreLancer.FLHash.CreateID(equip.Nickname) == u);
                if (armor != null && armor.GetType() == typeof(Armor))
                {
                    return (Armor)armor;
                }

                if (armor != null && armor.GetType() != typeof(Armor))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided id of {u} was not of type armor but of type {armor.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A armor with the id of {u} was not found");
                }
            });
    }

    public Either<ErrorResult, CargoPod> GetCargoPod(Either<uint, string> id)
    {
        return id.Match<Either<ErrorResult, CargoPod>>(s =>
            {
                var cargoPod = _freelancerData.Equipment.Equip.Find(equip => equip.Nickname == s);
                if (cargoPod != null && cargoPod.GetType() == typeof(CargoPod))
                {
                    return (CargoPod)cargoPod;
                }

                if (cargoPod != null && cargoPod.GetType() != typeof(CargoPod))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided name of {s} was not of type cargo pod but of type {cargoPod.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A cargo pod with the name of {s} was not found.");
                }
            },
            u =>
            {
                var cargoPod =
                    _freelancerData.Equipment.Equip.Find(equip => LibreLancer.FLHash.CreateID(equip.Nickname) == u);
                if (cargoPod != null && cargoPod.GetType() == typeof(CargoPod))
                {
                    return (CargoPod)cargoPod;
                }

                if (cargoPod != null && cargoPod.GetType() != typeof(CargoPod))
                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniTypeMismatch,
                        $"The provided id of {u} was not of type cargo pod but of type {cargoPod.GetType()}");
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A cargo pod with the id of {u} was not found");
                }
            });
    }

    public Either<ErrorResult, BaseGood> GetBaseGood(Either<uint, string> id)
    {
        return id.Match<Either<ErrorResult, BaseGood>>(s =>
        {
            var baseGood = _freelancerData.Markets.BaseGoods.Find(b => b.Base == s);
            if (baseGood is not null)
            {
                return baseGood;
            }
            else
            {
                return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                    $"The provided base name of {s}has no market good associated with it;");
            }
        }, u =>
        {
            var baseGood = _freelancerData.Markets.BaseGoods.Find(b => LibreLancer.FLHash.CreateID(b.Base) == u);
            if (baseGood is not null)
            {
                return baseGood;
            }
            else
            {
                return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                    $"The provided base id of {u}has no market good associated with it;");
            }
        });
    }

    public Either<ErrorResult, Good> GetGood(Either<uint, string> id)
    {
        return id.Match<Either<ErrorResult, Good>>(s =>
            {
                var good = _freelancerData.Goods.Goods.Find(good => good.Nickname == s);
                if (good != null)
                {
                    return good;
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A cargo pod with the name of {s} was not found.");
                }
            },
            u =>
            {
                var good = _freelancerData.Goods.Goods.Find(good => LibreLancer.FLHash.CreateID(good.Nickname) == u);
                if (good != null)
                {
                    return good;
                }

                {
                    return new ErrorResult(FLAdminErrorCode.FreelancerIniEntryNotFound,
                        $"A cargo pod with the id of {u} was not found");
                }
            });
    }
    public Either<ErrorResult, string> GetInfocard(int infocardId)
    {
        throw new NotImplementedException();
    }
}