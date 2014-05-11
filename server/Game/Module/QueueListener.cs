using System;
using System.Text;
using System.Threading;
using Common;
using Game.Comm;
using Game.Setup;
using Game.Util;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Game.Module
{
    class QueueListener : IQueueListener
    {
        private readonly IQueueCommandProcessor queueCommandProcessor;

        private readonly System.Timers.Timer reconnectTimer;

        private readonly ILogger logger = LoggerFactory.Current.GetLogger<QueueListener>();

        private IModel model;

        private IConnection connection;

        public QueueListener(IQueueCommandProcessor queueCommandProcessor)
        {
            this.queueCommandProcessor = queueCommandProcessor;

            reconnectTimer = new System.Timers.Timer(60000);
            reconnectTimer.AutoReset = false;
            reconnectTimer.Elapsed += (sender, args) => ConnectToRabbitMq();
        }

        public void Start(string hostname)
        {
            ConnectToRabbitMq();            
        }

        private bool ConnectToRabbitMq()
        {
            reconnectTimer.Enabled = false;

            int attempts = 0;
            
            while (attempts < 3)
            {
                attempts++;

                try
                {
                    var connectionFactory = new ConnectionFactory
                    {
                        HostName = Config.api_domain,
                        UserName = "guest",
                        Password = "guest",
                        RequestedHeartbeat = 60
                    };

                    connection = connectionFactory.CreateConnection();

                    // Create the model 
                    CreateModel();

                    return true;
                }
                catch(System.IO.EndOfStreamException ex)
                {
                    logger.Error(ex, "Failed to connect to RabbitMq");
                    return false;
                }
                catch(BrokerUnreachableException ex)
                {
                    logger.Error(ex, "Failed to connect to RabbitMq");
                    return false;
                }
                catch(Exception ex)
                {
                    logger.Warn(ex, "Failed to connect to RabbitMq");

                    Thread.Sleep(1000);
                }
            }

            if (connection != null)
            {
                connection.Dispose();
            }

            logger.Error("Failed to connect to RabbitMq after all attempts. Giving up for a few minutes.");

            reconnectTimer.Enabled = true;

            return false;
        }

        private void CreateModel()
        {
            model = connection.CreateModel();

            // When AutoClose is true, the last channel to close will also cause the connection to close. 
            // If it is set to true before any channel is created, the connection will close then and there.
            connection.AutoClose = true;

            // Configure the Quality of service for the model. Below is how what each setting means.
            // BasicQos(0="Dont send me a new message untill I’ve finshed",  1= "Send me one message at a time", false ="Apply to this Model only")
            model.BasicQos(0, 1, false);

            model.ExchangeDeclare("gameserver", ExchangeType.Fanout, false, false, null);
            string queueName = model.QueueDeclare();
            model.QueueBind(queueName, "gameserver", "", null);

              var consumer = new EventingBasicConsumer(model);
            
            consumer.Received += (o, queueEvent) =>
            {
                string command;
                dynamic data;

                try
                {
                    var queuedMessage = Encoding.ASCII.GetString(queueEvent.Body);
                    dynamic payload = JsonConvert.DeserializeObject(queuedMessage);
                    command = (string)payload.command;
                    data = payload.data;
                }
                catch(Exception e)
                {
                    logger.Warn(e, "Failed to execute queue command");
                    return;
                }

                queueCommandProcessor.Execute(command, data);
            };
 
            consumer.Shutdown += (o, e) => ConnectToRabbitMq();

            model.BasicConsume(queueName, true, consumer);
        }
    }
}