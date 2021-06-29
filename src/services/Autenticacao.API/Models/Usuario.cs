using System;

namespace Autenticacao.API.Models
{
    public class Usuario
    {
        public long? Id { get; set; }
        public string NomeUsuario { get; set; }
        public string NomeUsuarioNormalizado { get; set; }
        public string Email { get; set; }
        public string EmailNormalizado { get; set; }
        public bool EmailConfirmado { get; set; }
        public string SenhaCifrada { get; set; }
        public string Telefone { get; set; }
        public bool TelefoneConfirmado { get; set; }
        public bool AutenticacaoDoisFatores { get; set; }
        public DateTimeOffset? FimDoBloqueio { get; set; }
        public bool BloqueioHabilitado { get; set; }
        public int QuantidadeFalhasLogin { get; set; }
    }
}
