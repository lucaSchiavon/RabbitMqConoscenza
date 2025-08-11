
namespace Producer.Services
{
    public interface IProdottoService
    {
        Task InviaProdottoAsync(Prodotto prodotto);
        ValueTask DisposeAsync();
    }
}