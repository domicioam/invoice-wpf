using NFe.Core.NotaFiscal;

namespace NFe.Core.Sefaz
{
    internal interface IImpostoCreator
    {
        object Create(NotaFiscal.Interface.Imposto impostoItem);
    }
}