#region

using Game.Comm;
using Game.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

#endregion

namespace Testing.Channel
{
    /// <summary>
    ///   Summary description for ChannelTest
    /// </summary>
    [TestClass]
    public class ChannelTest : TestBase
    {
        private readonly Packet msg1 = new Packet();
        private readonly Packet msg2 = new Packet();
        private Game.Util.Channel channel;
        private Mock<IChannel> session1;
        private Mock<IChannel> session2;

        [TestInitialize]
        public void TestInitialize()
        {
            channel = new Game.Util.Channel();
            session1 = new Mock<IChannel>();
            session2 = new Mock<IChannel>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void TestSinglePost()
        {
            var session = new Mock<IChannel>();

            channel.Subscribe(session.Object, "Channel1");
            channel.Post("Channel1", msg1);

            session.Verify(foo => foo.OnPost(msg1), Times.Once());
        }

        [TestMethod]
        public void TestPostingToProperChannel()
        {
            channel.Subscribe(session1.Object, "Channel1");
            channel.Subscribe(session1.Object, "Channel2");

            channel.Post("Channel1", msg1);

            session1.Verify(foo => foo.OnPost(msg1), Times.Once());
        }

        [TestMethod]
        public void TestPostingToProperChannel2()
        {
            channel.Subscribe(session1.Object, "Channel1");
            channel.Subscribe(session1.Object, "Channel2");

            channel.Post("Channel1", msg1);
            channel.Post("Channel2", msg2);

            session1.Verify(foo => foo.OnPost(msg1), Times.Once());
            session1.Verify(foo => foo.OnPost(msg2), Times.Once());
        }

        [TestMethod]
        public void TestPostingToProperSession()
        {
            channel.Subscribe(session1.Object, "Channel1");
            channel.Subscribe(session2.Object, "Channel2");

            channel.Post("Channel1", msg1);

            session1.Verify(foo => foo.OnPost(msg1), Times.Once());
            session2.Verify(foo => foo.OnPost(msg1), Times.Never());
        }

        [TestMethod]
        public void TestPostingToMultipleSessions()
        {
            channel.Subscribe(session1.Object, "Channel1");
            channel.Subscribe(session2.Object, "Channel1");

            channel.Post("Channel1", msg1);

            session1.Verify(foo => foo.OnPost(msg1), Times.Once());
            session2.Verify(foo => foo.OnPost(msg1), Times.Once());
        }

        [TestMethod]
        public void TestPostingToProperSessionAndChannel()
        {
            channel.Subscribe(session1.Object, "Channel1");
            channel.Subscribe(session2.Object, "Channel2");

            channel.Post("Channel1", msg1);
            channel.Post("Channel2", msg2);

            session1.Verify(foo => foo.OnPost(msg1), Times.Once());
            session1.Verify(foo => foo.OnPost(msg2), Times.Never());

            session2.Verify(foo => foo.OnPost(msg1), Times.Never());
            session2.Verify(foo => foo.OnPost(msg2), Times.Once());
        }

        [TestMethod]
        public void TestUnsubscribingSingleSessionFromChannel()
        {
            channel.Subscribe(session1.Object, "Channel1");
            channel.Unsubscribe(session1.Object, "Channel1");

            channel.Post("Channel1", msg1);

            session1.Verify(foo => foo.OnPost(msg1), Times.Never());
        }

        [TestMethod]
        public void TestUnsubscribingProperSessionFromChannel()
        {
            channel.Subscribe(session1.Object, "Channel1");
            channel.Subscribe(session2.Object, "Channel1");

            channel.Unsubscribe(session2.Object, "Channel1");

            channel.Post("Channel1", msg1);

            session1.Verify(foo => foo.OnPost(msg1), Times.Once());
            session2.Verify(foo => foo.OnPost(msg1), Times.Never());
        }

        [TestMethod]
        public void TestUnsubscribingSingleSessionFromAllChannel()
        {
            channel.Subscribe(session1.Object, "Channel1");
            channel.Unsubscribe(session1.Object);

            channel.Post("Channel1", msg1);

            session1.Verify(foo => foo.OnPost(msg1), Times.Never());
        }

        [TestMethod]
        public void TestUnsubscribingProperSessionFromAllChannel()
        {
            channel.Subscribe(session1.Object, "Channel1");
            channel.Subscribe(session2.Object, "Channel1");

            channel.Unsubscribe(session2.Object);

            channel.Post("Channel1", msg1);

            session1.Verify(foo => foo.OnPost(msg1), Times.Once());
            session2.Verify(foo => foo.OnPost(msg1), Times.Never());
        }

        // The channel class deletes the subscriber list if the channel becomes empty so we want to make sure it works
        [TestMethod]
        public void TestRecreateChannel()
        {
            channel.Subscribe(session1.Object, "Channel1");
            channel.Unsubscribe(session1.Object, "Channel1");
            channel.Subscribe(session1.Object, "Channel1");

            channel.Post("Channel1", msg1);

            session1.Verify(foo => foo.OnPost(msg1), Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(DuplicateSubscriptionException))]
        public void TestSubscribingDuplicates()
        {
            channel.Subscribe(session1.Object, "Channel1");
            channel.Subscribe(session1.Object, "Channel1");
        }

        [TestMethod]
        public void TestSubscribingDuplicatesThenPosting()
        {
            channel.Subscribe(session1.Object, "Channel1");

            try
            {
                channel.Subscribe(session1.Object, "Channel1");
            }
            catch(DuplicateSubscriptionException)
            {
            }

            channel.Post("Channel1", msg1);

            session1.Verify(foo => foo.OnPost(msg1), Times.Once());
        }

        [TestMethod]
        public void TestUnsubscribingDuplicates()
        {
            channel.Subscribe(session1.Object, "Channel1");

            channel.Unsubscribe(session1.Object, "Channel1");
            channel.Unsubscribe(session1.Object, "Channel2");

            channel.Post("Channel1", msg1);

            session1.Verify(foo => foo.OnPost(msg1), Times.Never());
        }

        [TestMethod]
        public void TestSubscribingToMultipleChannels()
        {
            channel.Subscribe(session1.Object, "Channel1");
            channel.Subscribe(session1.Object, "Channel2");

            channel.Post("Channel1", msg1);

            session1.Verify(foo => foo.OnPost(msg1), Times.Once());
        }

        [TestMethod]
        public void TestSubscribingMultipleSessionsToMultipleChannels()
        {
            channel.Subscribe(session1.Object, "Channel1");
            channel.Subscribe(session1.Object, "Channel2");

            channel.Subscribe(session2.Object, "Channel2");
            channel.Subscribe(session2.Object, "Channel3");

            channel.Post("Channel1", msg1);
            channel.Post("Channel3", msg1);

            session1.Verify(foo => foo.OnPost(msg1), Times.Once());
            session2.Verify(foo => foo.OnPost(msg1), Times.Once());
        }

        [TestMethod]
        public void TestSubscribingMultipleSessionsToMultipleChannels2()
        {
            channel.Subscribe(session1.Object, "Channel1");
            channel.Subscribe(session1.Object, "Channel2");

            channel.Subscribe(session2.Object, "Channel2");
            channel.Subscribe(session2.Object, "Channel3");

            channel.Post("Channel2", msg1);

            session1.Verify(foo => foo.OnPost(msg1), Times.Once());
            session2.Verify(foo => foo.OnPost(msg1), Times.Once());
        }
    }
}