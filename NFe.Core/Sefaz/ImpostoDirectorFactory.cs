using System;
using NFe.Core.NotasFiscais;

namespace NFe.Core.Sefaz
{
    public class ImpostoCreatorFactory
    {
        internal IImpostoCreator Create(Imposto impostoItem)
        {
            switch (impostoItem)
            {
                case Icms _:
                {
                    var icmsStrategy = new IcmsCreator();
                    return icmsStrategy;
                }
                case II _:
                {
                    var iiStrategy = new IiDirector();
                    return iiStrategy;
                }
                case Ipi _:
                {
                    var ipiStrategy = new IpiDirector();
                    return ipiStrategy;
                }
                case Issqn _:
                {
                    var issqnStrategy = new IssqnDirector();
                    return issqnStrategy;
                }
                case Pis _:
                {
                    var pisStrategy = new PisDirector();
                    return pisStrategy;
                }
                case CofinsBase _:
                {
                    var cofinsStrategy = new CofinsDirector();
                    return cofinsStrategy;
                }
                default:
                    throw new NotImplementedException();
            }
        }
    }
}