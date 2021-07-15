using System.Collections.Generic;

namespace Catalogo.API.AtualizacaoBanco
{
    public interface IModeloAtualizacao
    {
        string Guid { get; set; }
        int Numero { get; set; }
        string Script { get; set; }
        IList<Script> ListaScripts { get; set; }
    }
}
