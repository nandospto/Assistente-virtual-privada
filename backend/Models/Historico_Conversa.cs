using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Historico_Conversa
    {
        public int Id { get; set; }
        public string? Remetente { get; set; }
        public string? Conteudo { get; set; }
        public DateTime Data_envio { get; set; }

        [ForeignKey("Chat")]
        public int Id_chat { get; set; }
        public Chat? Chat { get; set; }

    }
}