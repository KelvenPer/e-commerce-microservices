using Microsoft.AspNetCore.Mvc;
using VendasService.Models;
using VendasService.Data;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using Shared;
using VendasService.Services;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class PedidosController : ControllerBase
{
    private readonly VendasDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly RabbitMqPublisher _rabbitMqPublisher;

    public PedidosController(VendasDbContext context, IHttpClientFactory httpClientFactory, RabbitMqPublisher rabbitMqPublisher)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient();
        _rabbitMqPublisher = rabbitMqPublisher;
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Pedido>> GetPedido(int id)
    {
        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido == null)
        {
            return NotFound();
        }
        return pedido;
    }

    [HttpPost]
    public async Task<ActionResult<Pedido>> PostPedido(Pedido pedido)
    {
        try
        {
            // 1. Validação de estoque (chamada síncrona)
            var estoqueCheck = await _httpClient.GetAsync($"http://localhost:5001/api/produtos/{pedido.ProdutoId}");
            estoqueCheck.EnsureSuccessStatusCode();

            var produtoJson = await estoqueCheck.Content.ReadAsStringAsync();
            var produtoEstoque = JsonSerializer.Deserialize<ProdutoDto>(produtoJson);

            if (produtoEstoque.Quantidade < pedido.Quantidade)
            {
                return BadRequest("Estoque insuficiente.");
            }

            // 2. Criar o pedido (salvar no banco de dados)
            pedido.Status = "Confirmado";
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();
            
            // 3. Notificar o Estoque (chamada assíncrona)
            _rabbitMqPublisher.PublishVendaRealizada(pedido.ProdutoId, pedido.Quantidade);

            return CreatedAtAction("GetPedido", new { id = pedido.Id }, pedido);
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, "O serviço de estoque está indisponível.");
        }
    }
}

public class ProdutoDto // DTO para a resposta do EstoqueService
{
    public int Id { get; set; }
    public int Quantidade { get; set; }
}