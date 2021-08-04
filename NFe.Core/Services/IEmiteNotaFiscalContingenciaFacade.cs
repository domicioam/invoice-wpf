using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Domain;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;

namespace NFe.Core.Sefaz.Facades
{
    public interface IEmiteNotaFiscalContingenciaFacade
    {
        Task<NotaFiscal> SaveNotaFiscalContingenciaAsync(X509Certificate2 certificado, ConfiguracaoEntity config, NotaFiscal notaFiscal, string cscId, string csc, string nFeNamespaceName);
        void InutilizarCancelarNotasPendentesContingencia(NotaFiscalEntity notaParaCancelar, INotaFiscalRepository notaFiscalRepository);
    }
}