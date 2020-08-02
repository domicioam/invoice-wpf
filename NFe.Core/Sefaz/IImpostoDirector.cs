using NFe.Core.NotasFiscais;

namespace NFe.Core.Sefaz
{
    internal interface IImpostoDirector
    {
        object FillInImpostoDetails(Imposto impostoItem);
    }
}