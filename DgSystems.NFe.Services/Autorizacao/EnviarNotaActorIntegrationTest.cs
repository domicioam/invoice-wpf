using Akka.TestKit.Xunit2;
using DgSystems.NFe.Core.UnitTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DgSystems.NFe.Services.UnitTests.Autorizacao
{
    public class EnviarNotaActorIntegrationTest : TestKit, IClassFixture<NotaFiscalFixture>
    {
        [Fact]
        public void Deve_enviar_nota_em_contingencia_quando_offline()
        {
            // ativa modo offline
            // envia nota para enviar nota actor
            // mensagem deve ser encaminhada para nota fiscal contingencia actor
        }
    }
}
