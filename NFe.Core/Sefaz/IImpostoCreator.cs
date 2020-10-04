using NFe.Core.NotasFiscais;

namespace NFe.Core.Sefaz
{
    internal interface IImpostoCreator
    {
        object Create(Imposto impostoItem);
    }
}