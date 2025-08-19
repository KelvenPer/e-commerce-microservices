using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using EstoqueService.Data;
using System.Text;
using System.Text.Json;
using Shared;
using Microsoft.EntityFrameworkCore;

namespace EstoqueService.Services
{
    public class RabbitMqConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMqConsumer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "vendas-queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageString = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<VendaRealizadaMessage>(messageString);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<EstoqueDbContext>();
                    var produto = await context.Produtos.FirstOrDefaultAsync(p => p.Id == message.ProdutoId);
                    if (produto != null)
                    {
                        produto.Quantidade -= message.QuantidadeVendida;
                        await context.SaveChangesAsync();
                    }
                }
            };
            _channel.BasicConsume(queue: "vendas-queue", autoAck: true, consumer: consumer);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
            base.Dispose();
        }
    }
}