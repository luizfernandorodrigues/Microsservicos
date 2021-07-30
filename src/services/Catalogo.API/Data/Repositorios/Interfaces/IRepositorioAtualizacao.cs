namespace Catalogo.API.Data.Repositorios.Interfaces
{
    public interface IRepositorioAtualizacao : IRepositorio
    {
        object VerificaGuid(string guid);
        void ExecutaScript(string sql);
        void ArmazenaGuidExecutado(string guid);
        bool VerificaBaseDados();
        void CriaBaseDados();
    }
}
