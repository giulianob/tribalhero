using System;
using Common.Testing;
using FluentAssertions;
using Game.Data;
using Game.Data.Events;
using NSubstitute;
using Xunit.Extensions;

namespace Testing.GameObjectTests
{
    public class SimpleGameObjectTests : IDisposable
    {
        public SimpleGameObjectTests()
        {
            Global.Current = Substitute.For<IGlobal>();
            Global.Current.FireEvents.Returns(true);
        }

        public void Dispose()
        {
            Global.Current = null;
        }

        [Theory, AutoNSubstituteData]
        public void PrimaryPosition_WhenXSetAndObjIsNotYetInWorld_OriginalPositionXIsSaved(SimpleGameObjectStub gameObject)
        {
            gameObject.MonitorEvents();

            gameObject.BeginUpdate();
            gameObject.InWorld = false;
            gameObject.PrimaryPosition.X = 10;
            gameObject.EndUpdate();

            gameObject.ShouldRaise("ObjectUpdated")
                      .WithSender(gameObject)
                      .WithArgs<SimpleGameObjectArgs>(p => p.OriginalX == 10);
        }

        [Theory, AutoNSubstituteData]
        public void PrimaryPosition_WhenYSetAndObjIsNotYetInWorld_OriginalPositionYIsSaved(SimpleGameObjectStub gameObject)
        {
            gameObject.MonitorEvents();

            gameObject.BeginUpdate();
            gameObject.InWorld = false;
            gameObject.PrimaryPosition.Y = 10;
            gameObject.EndUpdate();

            gameObject.ShouldRaise("ObjectUpdated")
                      .WithSender(gameObject)
                      .WithArgs<SimpleGameObjectArgs>(p => p.OriginalY == 10);
        }

        public class SimpleGameObjectStub : SimpleGameObject 
        {
            public SimpleGameObjectStub(uint objectId, uint x, uint y) : base(objectId, x, y)
            {
            }

            public override byte Size
            {
                get
                {
                    return 2;
                }
            }

            public override ushort Type
            {
                get
                {
                    return 101;
                }
            }

            public override uint GroupId
            {
                get
                {
                    return 202;
                }
            }

            protected override void CheckUpdateMode()
            {
            }

            public override int Hash
            {
                get
                {
                    return (int)ObjectId;
                }
            }

            public override object Lock
            {
                get
                {
                    return this;
                }
            }
        }

    }
}