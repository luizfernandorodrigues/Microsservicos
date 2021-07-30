namespace Catalogo.API.Data.Repositorios
{
    public interface IRepositorio
    {
        void AbrirConexaoComTransacao();
        void AbrirConexaoSemTransacao();
        void Commit();
        void FecharConexao();
        void Rollback();
    }
}
