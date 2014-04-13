#region

using FluentAssertions;
using Game.Comm;
using Game.Util;
using NSubstitute;
using Xunit;

#endregion

namespace Testing.ChannelTests
{
    /// <summary>
    ///     Summary description for ChannelTest
    /// </summary>
    public class ChannelTest
    {
        private readonly IChannel channel;

        private readonly Packet msg1 = new Packet();

        private readonly Packet msg2 = new Packet();

        private readonly IChannelListener session1;

        private readonly IChannelListener session2;

        public ChannelTest()
        {
            channel = new Channel();
            session1 = Substitute.For<IChannelListener>();
            session2 = Substitute.For<IChannelListener>();
        }

        [Fact]
        public void TestSinglePost()
        {
            var session = Substitute.For<IChannelListener>();

            channel.Subscribe(session, "Channel1");
            channel.Post("Channel1", msg1);

            session.Received(1).OnPost(msg1);
        }

        [Fact]
        public void TestPostingToProperChannel()
        {
            channel.Subscribe(session1, "Channel1");
            channel.Subscribe(session1, "Channel2");

            channel.Post("Channel1", msg1);

            session1.Received(1).OnPost(msg1);
        }

        [Fact]
        public void TestPostingToProperChannel2()
        {
            channel.Subscribe(session1, "Channel1");
            channel.Subscribe(session1, "Channel2");

            channel.Post("Channel1", msg1);
            channel.Post("Channel2", msg2);

            session1.Received(1).OnPost(msg1);
            session1.Received(1).OnPost(msg2);
        }

        [Fact]
        public void TestPostingToProperSession()
        {
            channel.Subscribe(session1, "Channel1");
            channel.Subscribe(session2, "Channel2");

            channel.Post("Channel1", msg1);

            session1.Received(1).OnPost(msg1);
            session2.DidNotReceive().OnPost(msg1);
        }

        [Fact]
        public void TestPostingToMultipleSessions()
        {
            channel.Subscribe(session1, "Channel1");
            channel.Subscribe(session2, "Channel1");

            channel.Post("Channel1", msg1);

            session1.Received(1).OnPost(msg1);
            session2.Received(1).OnPost(msg1);
        }

        [Fact]
        public void TestPostingToProperSessionAndChannel()
        {
            channel.Subscribe(session1, "Channel1");
            channel.Subscribe(session2, "Channel2");

            channel.Post("Channel1", msg1);
            channel.Post("Channel2", msg2);

            session1.Received(1).OnPost(msg1);
            session1.DidNotReceive().OnPost(msg2);

            session2.DidNotReceive().OnPost(msg1);
            session2.Received(1).OnPost(msg2);
        }

        [Fact]
        public void TestUnsubscribingSingleSessionFromChannel()
        {
            channel.Subscribe(session1, "Channel1");
            channel.Unsubscribe(session1, "Channel1");

            channel.Post("Channel1", msg1);

            session1.DidNotReceive().OnPost(msg1);
        }

        [Fact]
        public void TestUnsubscribingProperSessionFromChannel()
        {
            channel.Subscribe(session1, "Channel1");
            channel.Subscribe(session2, "Channel1");

            channel.Unsubscribe(session2, "Channel1");

            channel.Post("Channel1", msg1);

            session1.Received(1).OnPost(msg1);
            session2.DidNotReceive().OnPost(msg1);
        }

        [Fact]
        public void TestUnsubscribingSingleSessionFromAllChannel()
        {
            channel.Subscribe(session1, "Channel1");
            channel.Unsubscribe(session1);

            channel.Post("Channel1", msg1);

            session1.DidNotReceive().OnPost(msg1);
        }

        [Fact]
        public void TestUnsubscribingProperSessionFromAllChannel()
        {
            channel.Subscribe(session1, "Channel1");
            channel.Subscribe(session2, "Channel1");

            channel.Unsubscribe(session2);

            channel.Post("Channel1", msg1);

            session1.Received(1).OnPost(msg1);
            session2.DidNotReceive().OnPost(msg1);
        }

        // The channel class deletes the subscriber list if the channel becomes empty so we want to make sure it works
        [Fact]
        public void TestRecreateChannel()
        {
            channel.Subscribe(session1, "Channel1");
            channel.Unsubscribe(session1, "Channel1");
            channel.Subscribe(session1, "Channel1");

            channel.Post("Channel1", msg1);

            session1.Received(1).OnPost(msg1);
        }

        [Fact]
        public void TestSubscribingDuplicatesThenPosting()
        {
            channel.Subscribe(session1, "Channel1");

            channel.Subscribe(session1, "Channel1");
            
            channel.Post("Channel1", msg1);

            session1.Received(1).OnPost(msg1);
        }

        [Fact]
        public void TestUnsubscribingDuplicates()
        {
            channel.Subscribe(session1, "Channel1");

            channel.Unsubscribe(session1, "Channel1");
            channel.Unsubscribe(session1, "Channel2");

            channel.Post("Channel1", msg1);

            session1.DidNotReceive().OnPost(msg1);
        }

        [Fact]
        public void TestSubscribingToMultipleChannels()
        {
            channel.Subscribe(session1, "Channel1");
            channel.Subscribe(session1, "Channel2");

            channel.Post("Channel1", msg1);

            session1.Received(1).OnPost(msg1);
        }

        [Fact]
        public void TestSubscribingMultipleSessionsToMultipleChannels()
        {
            channel.Subscribe(session1, "Channel1");
            channel.Subscribe(session1, "Channel2");

            channel.Subscribe(session2, "Channel2");
            channel.Subscribe(session2, "Channel3");

            channel.Post("Channel1", msg1);
            channel.Post("Channel3", msg1);

            session1.Received(1).OnPost(msg1);
            session2.Received(1).OnPost(msg1);
        }

        [Fact]
        public void TestSubscribingMultipleSessionsToMultipleChannels2()
        {
            channel.Subscribe(session1, "Channel1");
            channel.Subscribe(session1, "Channel2");

            channel.Subscribe(session2, "Channel2");
            channel.Subscribe(session2, "Channel3");

            channel.Post("Channel2", msg1);

            session1.Received(1).OnPost(msg1);
            session2.Received(1).OnPost(msg1);
        }

        [Fact]
        public void PostWithExpression_ShouldLoadExpressionOnlyOnce()
        {
            channel.Subscribe(session1, "Channel2");
            channel.Subscribe(session2, "Channel2");

            int called = 0;
            channel.Post("Channel2", () =>
                {
                    called++;
                    return msg1;
                });

            called.Should().Be(1);
            session1.Received(1).OnPost(msg1);
            session2.Received(1).OnPost(msg1);
        }
        
        [Fact]
        public void SubscriberCount_WhenChannelDoesntExist_ShouldReturn0()
        {
            channel.SubscriberCount("FakeChannel").Should().Be(0);
        }

        [Fact]
        public void SubscriberCount_WhenSubscribedMultipleTimes_ShouldReturnSubscriberCount()
        {
            channel.Subscribe(session1, "Channel1");
            channel.Subscribe(session1, "Channel1");

            channel.SubscriberCount("Channel1").Should().Be(1);
        }

        [Fact]
        public void SubscriberCount_WhenChannelExists_ShouldReturnSubscriberCount()
        {
            var session3 = Substitute.For<IChannelListener>();

            channel.Subscribe(session1, "Channel1");

            channel.Subscribe(session1, "Channel2");
            channel.Subscribe(session2, "Channel2");
            channel.Subscribe(session3, "Channel2");

            channel.SubscriberCount("Channel1").Should().Be(1);
            channel.SubscriberCount("Channel2").Should().Be(3);
        }
    }
}