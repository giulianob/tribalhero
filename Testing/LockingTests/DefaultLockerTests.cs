using System.Collections.Generic;
using System.Linq;
using Common.Testing;
using FluentAssertions;
using Game.Data;
using Game.Data.Stronghold;
using Game.Data.Tribe;
using Game.Data.Troop;
using Game.Map;
using Game.Util.Locking;
using NSubstitute;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;

namespace Testing.LockingTests
{
    public class DefaultLockerTests
    {
        #region Lock(uint cityId, out ICity city)

        [Theory, AutoNSubstituteData]
        public void Lock_CityId_WhenCityIsFound_ShouldReturnLockedCity(
            [Frozen] IGameObjectLocator locator,
            ICity city,
            DefaultLocker locker)
        {
            ICity outCity;
            locator.TryGetObjects(1, out outCity).Returns(call =>
            {
                call[1] = city;
                return true;
            });

            ICity lockedCity;
            var multiObjLock = locker.Lock(1, out lockedCity);
            
            ((object)lockedCity).Should().Be(city);
            multiObjLock.Received(1)
                        .Lock(Arg.Is<ILockable[]>(itemsLocked => itemsLocked.SequenceEqual(new[] { city })));
        }        
        
        [Theory, AutoNSubstituteData]
        public void Lock_CityId_WhenCityIsNotFound_ShouldReturnEmptyLockObject(
            [Frozen] IGameObjectLocator locator,
            ICity city,
            DefaultLocker locker)
        {
            ICity outCity;
            locator.TryGetObjects(1, out outCity).Returns(false);

            ICity lockedCity;
            var multiObjLock = locker.Lock(1, out lockedCity);

            ((object)lockedCity).Should().BeNull();
            multiObjLock.Received(1)
                        .Lock(Arg.Is<ILockable[]>(itemsLocked => !itemsLocked.Any()));
        }

        #endregion

        #region Lock(uint playerId, out IPlayer player)

        [Theory, AutoNSubstituteData]
        public void Lock_PlayerId_WhenPlayerIsFound_ShouldReturnLockedPlayer(
            [Frozen] IGameObjectLocator locator,
            IPlayer player,
            DefaultLocker locker)
        {
            IPlayer outPlayer;
            locator.TryGetObjects(1, out outPlayer).Returns(call =>
            {
                call[1] = player;
                return true;
            });

            IPlayer lockedPlayer;
            var multiObjLock = locker.Lock(1, out lockedPlayer);
            
            lockedPlayer.Should().Be(player);
            multiObjLock.Received(1)
                        .Lock(Arg.Is<ILockable[]>(itemsLocked => itemsLocked.SequenceEqual(new[] { player })));
        }        
        
        [Theory, AutoNSubstituteData]
        public void Lock_PlayerId_WhenPlayerIsNotFound_ShouldReturnEmptyLockObject(
            [Frozen] IGameObjectLocator locator,
            IPlayer player,
            DefaultLocker locker)
        {
            IPlayer outPlayer;
            locator.TryGetObjects(1, out outPlayer).Returns(false);

            IPlayer lockedPlayer;
            var multiObjLock = locker.Lock(1, out lockedPlayer);

            lockedPlayer.Should().BeNull();
            multiObjLock.Received(1)
                        .Lock(Arg.Is<ILockable[]>(itemsLocked => !itemsLocked.Any()));
        }

        #endregion

        #region Lock(uint strongholdId, out IStronghold stronghold)

        [Theory, AutoNSubstituteData]
        public void Lock_StrongholdId_WhenStrongholdIsFound_ShouldReturnLockedStronghold(
            [Frozen] IGameObjectLocator locator,
            IStronghold stronghold,
            DefaultLocker locker)
        {
            IStronghold outStronghold;
            locator.TryGetObjects(1, out outStronghold).Returns(call =>
            {
                call[1] = stronghold;
                return true;
            });

            IStronghold lockedStronghold;
            var multiObjLock = locker.Lock(1, out lockedStronghold);

            lockedStronghold.Should().Be(stronghold);
            multiObjLock.Received(1)
                        .Lock(Arg.Is<ILockable[]>(itemsLocked => itemsLocked.SequenceEqual(new[] { stronghold })));
        }        
        
        [Theory, AutoNSubstituteData]
        public void Lock_StrongholdId_WhenStrongholdIsNotFound_ShouldReturnEmptyLockObject(
            [Frozen] IGameObjectLocator locator,
            IStronghold stronghold,
            DefaultLocker locker)
        {
            IStronghold outStronghold;
            locator.TryGetObjects(1, out outStronghold).Returns(false);

            IStronghold lockedStronghold;
            var multiObjLock = locker.Lock(1, out lockedStronghold);

            lockedStronghold.Should().BeNull();
            multiObjLock.Received(1)
                        .Lock(Arg.Is<ILockable[]>(itemsLocked => !itemsLocked.Any()));
        }
        
        #endregion

        #region Lock(uint cityId, uint structureId, out ICity city, out IStructure obj)

        [Theory, AutoNSubstituteData]
        public void Lock_CityIdStructureId_WhenCityIsFound_ShouldReturnLockedCity(
            [Frozen] IGameObjectLocator locator,            
            ICity city,
            IStructure structure,
            DefaultLocker locker)
        {
            ICity outCity;
            locator.TryGetObjects(1, out outCity).Returns(call =>
            {
                call[1] = city;
                return true;
            });

            IStructure outStructure;
            city.TryGetStructure(2, out outStructure).Returns(call =>
            {
                call[1] = structure;
                return true;
            });

            ICity lockedCity;
            IStructure lockedStructure;
            var multiObjLock = locker.Lock(1, 2, out lockedCity, out lockedStructure);
            
            ((object)lockedCity).Should().Be(city);
            lockedStructure.Should().Be(structure);
            multiObjLock.Received(1)
                        .Lock(Arg.Is<ILockable[]>(itemsLocked => itemsLocked.SequenceEqual(new[] { city })));
        }      
        
        [Theory, AutoNSubstituteData]
        public void Lock_CityIdStructureId_WhenStructureIsNotFound_ShouldReturnEmptyLockAndNullCity(
            [Frozen] IGameObjectLocator locator,            
            [FrozenMock] DefaultMultiObjectLock.Factory lockFactory,
            IMultiObjectLock cityLock,
            IMultiObjectLock emptyLock,
            ICity city,
            IStructure structure,
            DefaultLocker locker)
        {
            lockFactory().Returns(cityLock, emptyLock);

            ICity outCity;
            locator.TryGetObjects(1, out outCity).Returns(call =>
            {
                call[1] = city;
                return true;
            });

            IStructure outStructure;
            city.TryGetStructure(2, out outStructure).Returns(false);

            ICity lockedCity;
            IStructure lockedStructure;
            var multiObjLock = locker.Lock(1, 2, out lockedCity, out lockedStructure);
            
            ((object)lockedCity).Should().BeNull();
            lockedStructure.Should().BeNull();
            multiObjLock.Should().Be(emptyLock);
            emptyLock.Received(1)
                     .Lock(Arg.Is<ILockable[]>(itemsLocked => !itemsLocked.Any()));
            cityLock.Received(1)
                     .Lock(Arg.Is<ILockable[]>(itemsLocked => itemsLocked.SequenceEqual(new[] {city})));
            cityLock.Received(1).UnlockAll();
        }      

        [Theory, AutoNSubstituteData]
        public void Lock_CityIdStructureId_WhenCityIsNotFound_ShouldReturnEmptyLockObject(
            [Frozen] IGameObjectLocator locator,
            ICity city,
            DefaultLocker locker)
        {
            ICity outCity;
            locator.TryGetObjects(1, out outCity).Returns(false);

            ICity lockedCity;
            IStructure lockedStructure;
            var multiObjLock = locker.Lock(1, 2, out lockedCity, out lockedStructure);

            ((object)lockedCity).Should().BeNull();
            lockedStructure.Should().BeNull();
            multiObjLock.Received(1)
                        .Lock(Arg.Is<ILockable[]>(itemsLocked => !itemsLocked.Any()));
        }

        #endregion

        #region Lock(uint tribeId, out ITribe tribe)

        [Theory, AutoNSubstituteData]
        public void Lock_TribeId_WhenTribeIsFound_ShouldReturnLockedTribe(
            [Frozen] IGameObjectLocator locator,
            ITribe tribe,
            DefaultLocker locker)
        {
            ITribe outTribe;
            locator.TryGetObjects(1, out outTribe).Returns(call =>
            {
                call[1] = tribe;
                return true;
            });

            ITribe lockedTribe;
            var multiObjLock = locker.Lock(1, out lockedTribe);
            
            lockedTribe.Should().Be(tribe);
            multiObjLock.Received(1)
                        .Lock(Arg.Is<ILockable[]>(itemsLocked => itemsLocked.SequenceEqual(new[] { tribe })));
        }        
        
        [Theory, AutoNSubstituteData]
        public void Lock_TribeId_WhenTribeIsNotFound_ShouldReturnEmptyLockObject(
            [Frozen] IGameObjectLocator locator,
            ITribe tribe,
            DefaultLocker locker)
        {
            ITribe outTribe;
            locator.TryGetObjects(1, out outTribe).Returns(false);

            ITribe lockedTribe;
            var multiObjLock = locker.Lock(1, out lockedTribe);

            lockedTribe.Should().BeNull();
            multiObjLock.Received(1)
                        .Lock(Arg.Is<ILockable[]>(itemsLocked => !itemsLocked.Any()));
        }

        #endregion

        #region Lock(uint cityId, uint troopObjectId, out ICity city, out ITroop obj)

        [Theory, AutoNSubstituteData]
        public void Lock_CityIdTroopId_WhenCityIsFound_ShouldReturnLockedCity(
            [Frozen] IGameObjectLocator locator,            
            ICity city,
            ITroopObject troop,
            DefaultLocker locker)
        {
            ICity outCity;
            locator.TryGetObjects(1, out outCity).Returns(call =>
            {
                call[1] = city;
                return true;
            });

            ITroopObject outTroop;
            city.TryGetTroop(2, out outTroop).Returns(call =>
            {
                call[1] = troop;
                return true;
            });

            ICity lockedCity;
            ITroopObject lockedTroop;
            var multiObjLock = locker.Lock(1, 2, out lockedCity, out lockedTroop);
            
            ((object)lockedCity).Should().Be(city);
            lockedTroop.Should().Be(troop);
            multiObjLock.Received(1)
                        .Lock(Arg.Is<ILockable[]>(itemsLocked => itemsLocked.SequenceEqual(new[] { city })));
        }      
        
        [Theory, AutoNSubstituteData]
        public void Lock_CityIdTroopId_WhenTroopIsNotFound_ShouldReturnEmptyLockAndNullCity(
            [Frozen] IGameObjectLocator locator,            
            [FrozenMock] DefaultMultiObjectLock.Factory lockFactory,
            IMultiObjectLock cityLock,
            IMultiObjectLock emptyLock,
            ICity city,
            ITroopObject troop,
            DefaultLocker locker)
        {
            lockFactory().Returns(cityLock, emptyLock);

            ICity outCity;
            locator.TryGetObjects(1, out outCity).Returns(call =>
            {
                call[1] = city;
                return true;
            });

            ITroopObject outTroop;
            city.TryGetTroop(2, out outTroop).Returns(false);

            ICity lockedCity;
            ITroopObject lockedTroop;
            var multiObjLock = locker.Lock(1, 2, out lockedCity, out lockedTroop);
            
            ((object)lockedCity).Should().BeNull();
            lockedTroop.Should().BeNull();
            multiObjLock.Should().Be(emptyLock);
            emptyLock.Received(1)
                     .Lock(Arg.Is<ILockable[]>(itemsLocked => !itemsLocked.Any()));
            cityLock.Received(1)
                     .Lock(Arg.Is<ILockable[]>(itemsLocked => itemsLocked.SequenceEqual(new[] {city})));
            cityLock.Received(1).UnlockAll();
        }      

        [Theory, AutoNSubstituteData]
        public void Lock_CityIdTroopId_WhenCityIsNotFound_ShouldReturnEmptyLockObject(
            [Frozen] IGameObjectLocator locator,
            ICity city,
            DefaultLocker locker)
        {
            ICity outCity;
            locator.TryGetObjects(1, out outCity).Returns(false);

            ICity lockedCity;
            ITroopObject lockedTroop;
            var multiObjLock = locker.Lock(1, 2, out lockedCity, out lockedTroop);

            ((object)lockedCity).Should().BeNull();
            lockedTroop.Should().BeNull();
            multiObjLock.Received(1)
                        .Lock(Arg.Is<ILockable[]>(itemsLocked => !itemsLocked.Any()));
        }

        #endregion

        #region Lock(uint cityId, out ICity city, out ITribe tribe)
        
        [Theory, AutoNSubstituteData]
        public void Lock_CityIdTribe_WhenCityIsNotFound_ShouldReturnEmptyLockAndNullCityAndNullTribe(
            [Frozen] IGameObjectLocator locator,
            [Frozen] ICallbackLock callbackLock,
            ICity city,
            DefaultLocker locker)
        {
            ICity outCity;
            locator.TryGetObjects(1, out outCity).Returns(false);

            ICity lockedCity;
            ITribe lockedTribe;
            var multiObjLock = locker.Lock(1, out lockedCity, out lockedTribe);

            lockedTribe.Should().BeNull();
            ((object)lockedCity).Should().BeNull();
            multiObjLock.Received(1)
                        .Lock(Arg.Is<ILockable[]>(itemsLocked => !itemsLocked.Any()));
        }    

        [Theory, AutoNSubstituteData]
        public void Lock_CityIdTribe_WhenPlayerIsInTribe_ShouldReturnLockedCityAndTribe(
            [Frozen] IGameObjectLocator locator,
            [Frozen] ICallbackLock callbackLock,
            ICity city,
            DefaultLocker locker)
        {
            ILockable[] itemsLocked = {};
            var callbackLockResult = Substitute.For<IMultiObjectLock>();
            callbackLock.Lock(Arg.Do<CallbackLock.CallbackLockHandler>(callbackLockHandler =>
            {
                itemsLocked = callbackLockHandler(new object[] {city});
            }),
                              Arg.Is<object[]>(lockParams => lockParams.SequenceEqual(new[] {city})),
                              Arg.Is<ILockable[]>(baseLocks => baseLocks.SequenceEqual(new[] {city}))
                    ).Returns(callbackLockResult);

            ICity outCity;
            locator.TryGetObjects(1, out outCity).Returns(call =>
            {
                call[1] = city;
                return true;
            });

            var tribe = city.Owner.Tribesman.Tribe;
            city.Owner.IsInTribe.Returns(true);
            
            ICity lockedCity;
            ITribe lockedTribe;
            var multiObjLock = locker.Lock(1, out lockedCity, out lockedTribe);

            multiObjLock.Should().Be(callbackLockResult);
            lockedTribe.Should().Be(tribe);
            ((object)lockedCity).Should().Be(city);
            itemsLocked.Should().Equal(tribe);
        }    
        
        [Theory, AutoNSubstituteData]
        public void Lock_CityIdTribe_WhenPlayerIsNotInTribe_ShouldReturnEmptyLockAndNullCityAndTribe(
            [Frozen] IGameObjectLocator locator,
            [Frozen] ICallbackLock callbackLock,
            [Frozen] IMultiObjectLock multiObjectLock,
            ICity city,
            DefaultLocker locker)
        {
            ILockable[] itemsLocked = {};
            var callbackLockResult = Substitute.For<IMultiObjectLock>();
            callbackLock.Lock(Arg.Do<CallbackLock.CallbackLockHandler>(callbackLockHandler =>
            {
                itemsLocked = callbackLockHandler(new object[] {city});
            }),
                              Arg.Is<object[]>(lockParams => lockParams.SequenceEqual(new[] {city})),
                              Arg.Is<ILockable[]>(baseLocks => baseLocks.SequenceEqual(new[] {city}))
                    ).Returns(callbackLockResult);

            ICity outCity;
            locator.TryGetObjects(1, out outCity).Returns(call =>
            {
                call[1] = city;
                return true;
            });

            city.Owner.IsInTribe.Returns(false);
            
            ICity lockedCity;
            ITribe lockedTribe;
            var multiObjLock = locker.Lock(1, out lockedCity, out lockedTribe);

            callbackLockResult.Received(1).UnlockAll();
            multiObjLock.Should().Be(multiObjectLock);
            lockedTribe.Should().BeNull();
            ((object)lockedCity).Should().BeNull();
            itemsLocked.Should().BeEmpty();
        }    

        #endregion

        #region Lock(out Dictionary<uint, ICity> result, params uint[] cityIds)

        [Theory, AutoNSubstituteData]
        public void Lock_ListOfCityId_WhenCitiesAreFound_ShouldReturnLockedCities(
            [Frozen] IGameObjectLocator locator,
            ICity city1,
            ICity city2,
            ICity city3,
            DefaultLocker locker)
        {
            ICity outCity2;
            locator.TryGetObjects(Arg.Is<uint>(id => id >= 1 && id <= 3), out outCity2).ReturnsForAnyArgs(call =>
            {
                call[1] = city1;
                return true;
            }, call =>
            {
                call[1] = city2;
                return true;
            }, call =>
            {
                call[1] = city3;
                return true;
            });

            Dictionary<uint, ICity> lockedCities;
            var multiObjLock = locker.Lock(out lockedCities, 1, 2, 3);

            lockedCities.Should().Contain(new KeyValuePair<uint, ICity>(1, city1));
            lockedCities.Should().Contain(new KeyValuePair<uint, ICity>(2, city2));
            lockedCities.Should().Contain(new KeyValuePair<uint, ICity>(3, city3));
            multiObjLock.Received(1)
                        .Lock(Arg.Is<ILockable[]>(itemsLocked => itemsLocked.SequenceEqual(new[]
                        {
                            city1, city2, city3
                        })));
        }        

        [Theory, AutoNSubstituteData]
        public void Lock_ListOfCityId_WhenCitiesAreNotFound_ShouldReturnEmptyLockAndEmptyDictionary(
            [Frozen] IGameObjectLocator locator,
            ICity city1,
            ICity city2,
            ICity city3,
            DefaultLocker locker)
        {
            ICity outCity2;
            locator.TryGetObjects(Arg.Is<uint>(id => id >= 1 && id <= 2), out outCity2).ReturnsForAnyArgs(call =>
            {
                call[1] = city1;
                return true;
            }, call =>
            {
                call[1] = city2;
                return false;
            });

            Dictionary<uint, ICity> lockedCities;
            var multiObjLock = locker.Lock(out lockedCities, 1, 2);

            lockedCities.Should().BeNull();
            multiObjLock.Received(1)
                        .Lock(Arg.Is<ILockable[]>(itemsLocked => !itemsLocked.Any()));
        }        

        #endregion

        #region Lock(out Dictionary<uint, IPlayer> result, params uint[] playerIds)

        [Theory, AutoNSubstituteData]
        public void Lock_ListOfPlayerId_WhenPlayersAreFound_ShouldReturnLockedPlayers(
            [Frozen] IGameObjectLocator locator,
            IPlayer player1,
            IPlayer player2,
            IPlayer player3,
            DefaultLocker locker)
        {
            IPlayer outPlayer2;
            locator.TryGetObjects(Arg.Is<uint>(id => id >= 1 && id <= 3), out outPlayer2).ReturnsForAnyArgs(call =>
            {
                call[1] = player1;
                return true;
            }, call =>
            {
                call[1] = player2;
                return true;
            }, call =>
            {
                call[1] = player3;
                return true;
            });

            Dictionary<uint, IPlayer> lockedPlayers;
            var multiObjLock = locker.Lock(out lockedPlayers, 1, 2, 3);

            lockedPlayers.Should().Contain(new KeyValuePair<uint, IPlayer>(1, player1));
            lockedPlayers.Should().Contain(new KeyValuePair<uint, IPlayer>(2, player2));
            lockedPlayers.Should().Contain(new KeyValuePair<uint, IPlayer>(3, player3));
            multiObjLock.Received(1)
                        .Lock(Arg.Is<ILockable[]>(itemsLocked => itemsLocked.SequenceEqual(new[]
                        {
                            player1, player2, player3
                        })));
        }        

        [Theory, AutoNSubstituteData]
        public void Lock_ListOfPlayerId_WhenPlayersAreNotFound_ShouldReturnEmptyLockAndEmptyDictionary(
            [Frozen] IGameObjectLocator locator,
            IPlayer player1,
            IPlayer player2,
            IPlayer player3,
            DefaultLocker locker)
        {
            IPlayer outPlayer2;
            locator.TryGetObjects(Arg.Is<uint>(id => id >= 1 && id <= 2), out outPlayer2).ReturnsForAnyArgs(call =>
            {
                call[1] = player1;
                return true;
            }, call =>
            {
                call[1] = player2;
                return false;
            });

            Dictionary<uint, IPlayer> lockedPlayers;
            var multiObjLock = locker.Lock(out lockedPlayers, 1, 2);

            lockedPlayers.Should().BeNull();
            multiObjLock.Received(1)
                        .Lock(Arg.Is<ILockable[]>(itemsLocked => !itemsLocked.Any()));
        }        

        #endregion
    }
}