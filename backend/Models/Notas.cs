namespace backend.Models
{
    public class Notas
    {
        public int Id { get; set; }
        public string? Conteudo { get; set; }
        public DateTime Data_criacao { get; set; }
        public bool Concluido { get; set; }

    }
}