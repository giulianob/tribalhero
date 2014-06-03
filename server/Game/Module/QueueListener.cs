﻿using System;
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
            ConnectToRabbitMq();            
        }

        public void Stop()
        {
            logger.Info("Stopping QueueListener");

            isEnabled = false;
            DisposeConnection();
        }

        private bool ConnectToRabbitMq()
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
                    CreateModel();

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
                if (ConnectToRabbitMq())
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

                try
                {
                    queueCommandProcessor.Execute(command, data);
                }
                catch(Exception e)
                {
                    logger.Error("Unhandled queue receive error");
                    Engine.UnhandledExceptionHandler(e);
                }
            };
 
            consumer.Shutdown += (o, ev) =>
            {
                logger.Warn("Connection to RabbitMq lost");
                ReconnectToRabbitMq();
            };

            model.BasicConsume(queueName, true, consumer);            
        }
    }
}