using NFe.Core.Domain;

namespace NFe.Core.Sefaz
{
    internal interface IImpostoCreator
    {
        object Create(IImposto impostoItem);
    }
}