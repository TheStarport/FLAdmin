using FlAdmin.Common.Models;
using LanguageExt;
using LibreLancer.Data.Ships;
using LibreLancer.Data.Universe;

namespace FlAdmin.Common.Services;

public interface IFreelancerDataService
{
    
    //Archetype Information
    
    
    public Either<ErrorResult,Ship> GetShip(Either<uint,string> id);
    
    public Either<ErrorResult,Base> GetBase(Either<uint,string> id);
    
    public Either<ErrorResult,StarSystem> GetSystem(Either<uint,string> id);
    
    public Either<ErrorResult,LibreLancer.Data.Universe.SystemObject> GetSystemObject(Either<uint,string> id, string systemNickname  = "none");
    
    public Either<ErrorResult,LibreLancer.Data.Equipment.Commodity> GetCommodity(Either<uint,string> id);
    public Either<ErrorResult,LibreLancer.Data.Equipment.Gun> GetGun(Either<uint,string> id);
    public Either<ErrorResult,LibreLancer.Data.Equipment.Thruster> GetThruster(Either<uint,string> id);
    public Either<ErrorResult,LibreLancer.Data.Equipment.Engine> GetEngine(Either<uint,string> id);
    public Either<ErrorResult,LibreLancer.Data.Equipment.Countermeasure> GetCountermeasure(Either<uint,string> id);
    public Either<ErrorResult,LibreLancer.Data.Equipment.CountermeasureDropper> GetCountermeasureDropper(Either<uint,string> id);
    public Either<ErrorResult,LibreLancer.Data.Equipment.Mine> GetMine(Either<uint,string> id);
    public Either<ErrorResult,LibreLancer.Data.Equipment.MineDropper> GetMineDropper(Either<uint,string> id);
    public Either<ErrorResult,LibreLancer.Data.Equipment.Munition> GetMunition(Either<uint,string> id);
    public Either<ErrorResult,LibreLancer.Data.Equipment.Motor> GetMotor(Either<uint,string> id);
    public Either<ErrorResult,LibreLancer.Data.Equipment.PowerCore> GetPowerCore(Either<uint,string> id);
    public Either<ErrorResult,LibreLancer.Data.Equipment.ShieldGenerator> GetShieldGenerator(Either<uint,string> id);
    public Either<ErrorResult,LibreLancer.Data.Equipment.Scanner> GetScanner(Either<uint,string> id);
    public Either<ErrorResult,LibreLancer.Data.Equipment.Tractor> GetTractor(Either<uint,string> id);
    public Either<ErrorResult,LibreLancer.Data.Equipment.ShieldBattery> GetShieldBattery(Either<uint,string> id);
    public Either<ErrorResult,LibreLancer.Data.Equipment.RepairKit> GetRepairKit(Either<uint,string> id);
    public Either<ErrorResult,LibreLancer.Data.Equipment.Armor> GetArmor(Either<uint,string> id);
    public Either<ErrorResult,LibreLancer.Data.Equipment.CargoPod> GetCargoPod(Either<uint,string> id);
    
    
    public Either<ErrorResult,LibreLancer.Data.Goods.BaseGood> GetBaseGood(Either<uint,string> id);
    public Either<ErrorResult,LibreLancer.Data.Goods.Good> GetGood(Either<uint,string> id);
    public Either<ErrorResult,LibreLancer.Data.Goods.MarketGood> GetMarketGood(Either<uint,string> id);
    public Either<ErrorResult,LibreLancer.Data.Goods.GoodAddon> GetGoodAddon(Either<uint,string> id);


    public Either<ErrorResult,string> GetInfocard(int infocardId);

}