using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Module
{
    class QueueListener
    {
        public void Start(string hostname)
        {
            /*
            Console.WriteLine("Listener");
 
            const string EXCHANGE_NAME = "EXCHANGE3";
            ConnectionFactory factory = new ConnectionFactory();
 
            using (IConnection connection = factory.CreateConnection())
            {
                using (IModel channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(EXCHANGE_NAME, ExchangeType.Topic, false, true, null);
 
                    string queueName = channel.QueueDeclare();
 
                    EventingBasicConsumer consumer = new EventingBasicConsumer();
                    consumer.Received += (o, e) =>
                                             {
                                                 string data = Encoding.ASCII.GetString(e.Body);
                                                 Console.WriteLine(data);
                                             };
 
                    string consumerTag = channel.BasicConsume(queueName, true, consumer);
 
                    channel.QueueBind(queueName, EXCHANGE_NAME, "myTopic");
 
                    Console.WriteLine("Listening press ENTER to quit");
                    Console.ReadLine();
 
                    channel.QueueUnbind(queueName, EXCHANGE_NAME, "myTopic", null);
                }
            }
            */
        }
    }
}
