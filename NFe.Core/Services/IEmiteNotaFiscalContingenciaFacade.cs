using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using NFe.Core.Cadastro.Configuracoes;
using NFe.Core.Entitities;
using NFe.Core.Interfaces;

namespace NFe.Core.Sefaz.Facades
{
    public interface IEmiteNotaFiscalContingenciaFacade
    {
        Domain.NotaFiscal SaveNotaFiscalContingencia(X509Certificate2 certificado, ConfiguracaoEntity config, Domain.NotaFiscal notaFiscal, string cscId, string csc, string nFeNamespaceName);
        Task<List<string>> TransmitirNotasFiscalEmContingencia();
        void InutilizarCancelarNotasPendentesContingencia(NotaFiscalEntity notaParaCancelar, INotaFiscalRepository notaFiscalRepository);
    }
}