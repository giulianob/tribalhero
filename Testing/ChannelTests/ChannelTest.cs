#region

using Game.Comm;
using Game.Util;
using Moq;
using Xunit;

#endregion

namespace Testing.ChannelTests
{
    /// <summary>
    ///     Summary description for ChannelTest
    /// </summary>
    public class ChannelTest
    {
        private readonly Game.Util.Channel channel;

        private readonly Packet msg1 = new Packet();

        private readonly Packet msg2 = new Packet();

        private readonly Mock<IChannel> session1;

        private readonly Mock<IChannel> session2;

        public ChannelTest()
        {
            channel = new Game.Util.Channel();
            session1 = new Mock<IChannel>();
            session2 = new Mock<IChannel>();
        }

        [Fact]
        public void TestSinglePost()
        {
            var session = new Mock<IChannel>();

            channel.Subscribe(session.Object, "Channel1");
            channel.Post("Channel1", msg1);

            session.Verify(foo => foo.OnPost(msg1), Times.Once());
        }

        [Fact]
        public void TestPostingToProperChannel()
        {
            channel.Subscribe(session1.Object, "Channel1");
            channel.Subscribe(session1.Object, "Channel2");

            channel.Post("Channel1", msg1);

            session1.Verify(foo => foo.OnPost(msg1), Times.Once());
        }

        [Fact]
        public void TestPostingToProperChannel2()
        {
            channel.Subscribe(session1.Object, "Channel1");
            channel.Subscribe(session1.Object, "Channel2");

            channel.Post("Channel1", msg1);
            channel.Post("Channel2", msg2);

            session1.Verify(foo => foo.OnPost(msg1), Times.Once());
            session1.Verify(foo => foo.OnPost(msg2), Times.Once());
        }

        [Fact]
        public void TestPostingToProperSession()
        {
            channel.Subscribe(session1.Object, "Channel1");
            channel.Subscribe(session2.Object, "Channel2");

            channel.Post("Channel1", msg1);

            session1.Verify(foo => foo.OnPost(msg1), Times.Once());
            session2.Verify(foo => foo.OnPost(msg1), Times.Never());
        }

        [Fact]
        public void TestPostingToMultipleSessions()
        {
            channel.Subscribe(session1.Object, "Channel1");
            channel.Subscribe(session2.Object, "Channel1");

            channel.Post("Channel1", msg1);

            session1.Verify(foo => foo.OnPost(msg1), Times.Once());
            session2.Verify(foo => foo.OnPost(msg1), Times.Once());
        }

        [Fact]
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

        [Fact]
        public void TestUnsubscribingSingleSessionFromChannel()
        {
            channel.Subscribe(session1.Object, "Channel1");
            channel.Unsubscribe(session1.Object, "Channel1");

            channel.Post("Channel1", msg1);

            session1.Verify(foo => foo.OnPost(msg1), Times.Never());
        }

        [Fact]
        public void TestUnsubscribingProperSessionFromChannel()
        {
            channel.Subscribe(session1.Object, "Channel1");
            channel.Subscribe(session2.Object, "Channel1");

            channel.Unsubscribe(session2.Object, "Channel1");

            channel.Post("Channel1", msg1);

            session1.Verify(foo => foo.OnPost(msg1), Times.Once());
            session2.Verify(foo => foo.OnPost(msg1), Times.Never());
        }

        [Fact]
        public void TestUnsubscribingSingleSessionFromAllChannel()
        {
            channel.Subscribe(session1.Object, "Channel1");
            channel.Unsubscribe(session1.Object);

            channel.Post("Channel1", msg1);

            session1.Verify(foo => foo.OnPost(msg1), Times.Never());
        }

        [Fact]
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
        [Fact]
        public void TestRecreateChannel()
        {
            channel.Subscribe(session1.Object, "Channel1");
            channel.Unsubscribe(session1.Object, "Channel1");
            channel.Subscribe(session1.Object, "Channel1");

            channel.Post("Channel1", msg1);

            session1.Verify(foo => foo.OnPost(msg1), Times.Once());
        }

        [Fact]
        public void TestSubscribingDuplicatesThenPosting()
        {
            channel.Subscribe(session1.Object, "Channel1");

            channel.Subscribe(session1.Object, "Channel1");
            
            channel.Post("Channel1", msg1);

            session1.Verify(foo => foo.OnPost(msg1), Times.Once());
        }

        [Fact]
        public void TestUnsubscribingDuplicates()
        {
            channel.Subscribe(session1.Object, "Channel1");

            channel.Unsubscribe(session1.Object, "Channel1");
            channel.Unsubscribe(session1.Object, "Channel2");

            channel.Post("Channel1", msg1);

            session1.Verify(foo => foo.OnPost(msg1), Times.Never());
        }

        [Fact]
        public void TestSubscribingToMultipleChannels()
        {
            channel.Subscribe(session1.Object, "Channel1");
            channel.Subscribe(session1.Object, "Channel2");

            channel.Post("Channel1", msg1);

            session1.Verify(foo => foo.OnPost(msg1), Times.Once());
        }

        [Fact]
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

        [Fact]
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