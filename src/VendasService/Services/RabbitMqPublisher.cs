using RabbitMQ.Client;
using Shared;
using System.Text;
using System.Text.Json;

namespace VendasService.Services
{
    public class RabbitMqPublisher
    {
        public void PublishVendaRealizada(int produtoId, int quantidade)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "vendas-queue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var message = new VendaRealizadaMessage
                {
                    ProdutoId = produtoId,
                    QuantidadeVendida = quantidade
                };
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                channel.BasicPublish(exchange: "",
                                     routingKey: "vendas-queue",
                                     basicProperties: null,
                                     body: body);
            }
        }
    }
}