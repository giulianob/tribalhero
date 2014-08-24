using Game.Data;
using Game.Data.BarbarianTribe;
using Game.Data.Troop;
using Game.Logic.Actions;
using Game.Logic.Formulas;
using Game.Map;
using Game.Setup;
using System.Linq;
using Game.Util.Locking;

namespace Game.Logic.Procedures
{
    public class CityProcedure
    {
        private readonly Formula formula;
        private readonly IWorld world;
        private readonly IObjectTypeFactory objectTypeFactory;
        private readonly Procedure procedure;

        public CityProcedure(Formula formula, IWorld world, IObjectTypeFactory objectTypeFactory, Procedure procedure)
        {
            this.formula = formula;
            this.world = world;
            this.objectTypeFactory = objectTypeFactory;
            this.procedure = procedure;
        }

        public virtual Error CityNameCheck(string name)
        {
            if (world.CityNameTaken(name))
            {
                return Error.CityNameTaken;
            }

            if (!CityManager.IsNameValid(name))
            {
                return Error.CityNameInvalid;
            }
            return Error.Ok;
        }

        public virtual Error PositionCheckForNewCity(Position position)
        {
            if (!objectTypeFactory.IsTileType("CityStartTile", world.Regions.GetTileType(position.X, position.Y)))
            {
                return Error.TileMismatch;
            }

            // check if tile is occupied
            if (world.Regions.GetObjectsInTile(position.X, position.Y).Any(obj => obj is IStructure))
            {
                return Error.StructureExists;
            }
            return Error.Ok;
        }

        public virtual Error CanCityBeRemoved(ICity city)
        {
            if (city == null)
                return Error.CityNotFound;

            if (city.Battle != null)
                return Error.CityInBattle;

            if (city.Notifications.Count > 0)
                return Error.CityIncomingAttack;

            if (city.Troops.StationedHere().Any())
                return Error.TroopStationedInCity;

            if (city.Troops.MyStubs().Count() > 1)
                return Error.TroopOutstanding;

            var error = city.AllActionsCancelable();
            return error != Error.Ok ? error : Error.Ok;
        }
        /// <summary>
        ///     Creates a city under the specified player with initial troop and main building
        /// </summary>
        public virtual void CreateCity(ICityFactory cityFactory, IPlayer player, string cityName, byte beginningLevel, Position cityPosition, IBarbarianTribeManager barbarianTribeManager, out ICity city)
        {
            city = cityFactory.CreateCity(world.Cities.GetNextCityId(),
                            player,
                            cityName,
                            cityPosition,
                            formula.GetInitialCityResources(),
                            formula.GetInitialCityRadius(),
                            formula.GetInitialAp(),
                            Theme.DEFAULT_THEME_ID,
                            Theme.DEFAULT_THEME_ID,
                            Theme.DEFAULT_THEME_ID);

            var mainBuildingPosition = cityPosition.Left();
            IStructure mainBuilding = city.CreateStructure(2000, beginningLevel, mainBuildingPosition.X, mainBuildingPosition.Y);

            player.Add(city);

            world.Cities.Add(city);

            city.BeginUpdate();

            mainBuilding.BeginUpdate();
            world.Regions.Add(mainBuilding);
            mainBuilding.EndUpdate();

            var defaultTroop = city.CreateTroopStub();
            defaultTroop.BeginUpdate();
            defaultTroop.AddFormation(FormationType.Normal);
            defaultTroop.AddFormation(FormationType.Garrison);
            defaultTroop.AddFormation(FormationType.InBattle);
            defaultTroop.EndUpdate();

            procedure.RecalculateCityResourceRates(city);
            procedure.SetResourceCap(city);

            city.EndUpdate();

            if (player.GetCityCount() == 1)
            {
                barbarianTribeManager.CreateBarbarianTribeNear(1, 10, city.PrimaryPosition.X, city.PrimaryPosition.Y, 10);
            }
        }

        public virtual void InitCity(ICity city, CallbackProcedure callbackProcedure, IActionFactory actionFactory)
        {
            var mainBuilding = city.MainBuilding;
            callbackProcedure.OnStructureConvert(mainBuilding);       
            city.Worker.DoPassive(city, actionFactory.CreateCityPassiveAction(city.Id), false);
        }
    }
}
