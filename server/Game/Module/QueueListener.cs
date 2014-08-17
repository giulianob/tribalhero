using System;
using System.Collections.Generic;
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

        private bool isEnabled;

        public QueueListener(IQueueCommandProcessor queueCommandProcessor)
        {
            this.queueCommandProcessor = queueCommandProcessor;

            reconnectTimer = new System.Timers.Timer(15000);
            reconnectTimer.AutoReset = false;
            reconnectTimer.Elapsed += (sender, args) => ReconnectToRabbitMq();
        }

        public void Start(string hostname)
        {
            logger.Info("Starting QueueListener");
            
            isEnabled = true;
            ConnectToRabbitMq(true);
        }

        public void Stop()
        {
            logger.Info("Stopping QueueListener");

            isEnabled = false;
            DisposeConnection();
        }

        private bool ConnectToRabbitMq(bool purge)
        {
            logger.Info("Connecting to RabbitMq");

            reconnectTimer.Stop();

            int attempts = 0;
            
            while (attempts < 3)
            {
                attempts++;

                try
                {
                    var connectionFactory = new ConnectionFactory
                    {
                        HostName = Config.api_domain,
                        UserName = Config.queue_username,
                        Password = Config.queue_password,
                        RequestedHeartbeat = 60,                        
                    };

                    connection = connectionFactory.CreateConnection();

                    // Create the model 
                    CreateModel(purge);

                    return true;
                }
                catch(System.IO.EndOfStreamException ex)
                {
                    logger.Error(ex, "Failed to connect to RabbitMq");                    
                }
                catch(BrokerUnreachableException ex)
                {
                    logger.Error(ex, "Failed to connect to RabbitMq");                    
                }
                catch(Exception ex)
                {
                    logger.Error(ex, "Failed to connect to RabbitMq");                    
                }

                DisposeConnection();
                Thread.Sleep(1000);
            }
            
            logger.Error("Failed to connect to RabbitMq after all attempts. Trying later.");

            reconnectTimer.Start();            

            return false;
        }

        private void DisposeConnection()
        {
            if (connection == null)
            {
                return;
            }

            try
            {
                if (connection.IsOpen)
                {
                    connection.Close(500);
                }                                
            }
            catch { }

            connection = null;
        }

        private void ReconnectToRabbitMq()
        {
            if (!isEnabled)
            {
                return;
            }

            try
            {                
                if (ConnectToRabbitMq(false))
                {
                    logger.Info("Reconnected to RabbitMq");
                }
            }
            catch(Exception e)
            {
                logger.Error("Unhandled exception while reconnecting to Rabbit");
                Engine.UnhandledExceptionHandler(e);
            }
        }

        private void CreateModel(bool purge)
        {
            model = connection.CreateModel();

            // When AutoClose is true, the last channel to close will also cause the connection to close. 
            // If it is set to true before any channel is created, the connection will close then and there.
            connection.AutoClose = true;

            // Configure the Quality of service for the model. Below is how what each setting means.
            // BasicQos(0="Dont send me a new message untill I’ve finshed",  1= "Send me one message at a time", false ="Apply to this Model only")
            model.BasicQos(0, 1, false);

            model.ExchangeDeclare("gameserver", ExchangeType.Fanout, durable: true, autoDelete: false, arguments: null);

            var queueArguments = new Dictionary<string, object>
            {
                {"x-message-ttl", 900000}
            };

            var queueDeclareResult = model.QueueDeclare(string.Format("gameserver.{0}", Config.api_id), durable: true, exclusive: false, autoDelete: false, arguments: queueArguments);
            var queueName = queueDeclareResult.QueueName;

            if (purge)
            {
                model.QueuePurge(queueName);
            }

            model.QueueBind(queueName, exchange: "gameserver", routingKey: "", arguments: null);

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

                try
                {
                    queueCommandProcessor.Execute(command, data);
                }
                catch(Exception e)
                {
                    logger.Error("Unhandled queue receive error");
                    Engine.UnhandledExceptionHandler(e);
                }

                try
                {
                    model.BasicAck(queueEvent.DeliveryTag, false);
                }
                catch(Exception e)
                {
                    logger.Error(e, "Unable to ACK message with delivery tag {0}", queueEvent.DeliveryTag);
                }
            };
            
            consumer.Shutdown += (o, ev) =>
            {
                logger.Warn("Connection to RabbitMq lost");
                ReconnectToRabbitMq();
            };

            model.BasicConsume(string.Format("gameserver.{0}", Config.api_id), consumer: consumer, noAck: false);
        }
    }
}