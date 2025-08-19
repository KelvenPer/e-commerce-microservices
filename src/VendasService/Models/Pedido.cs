namespace VendasService.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public DateTime DataPedido { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Aguardando Confirmação";
    }
}