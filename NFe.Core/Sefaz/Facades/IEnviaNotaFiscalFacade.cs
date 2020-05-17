using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NFe.Core.Entitities;
using NFe.Core.NotasFiscais.Sefaz.NfeAutorizacao;
using NFe.Core.NotasFiscais.Sefaz.NfeRecepcaoEvento;
using NFe.Core.Interfaces;

namespace NFe.Core.NotasFiscais.Services
{
    public interface IEnviaNotaFiscalFacade
    {
        event NotaEmitidaEmContingenciaEventHandler NotaEmitidaEmContingenciaEvent;
        int EnviarNotaFiscal(NotaFiscal notaFiscal, string cscId, string csc);
    }
}