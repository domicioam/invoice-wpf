using NFe.Core.Entitities;
using System;
using System.Collections.Generic;
using AutoFixture;
using NFe.Core.Interfaces;
using Moq;
using NFe.Core.NotasFiscais.Sefaz.NfeConsulta2;
using NFe.Core.NotasFiscais;
using NFe.Core.Cadastro.Certificado;
using NFe.Core.Sefaz;

namespace DgSystems.NFe.Core.UnitTests.Services.Actors
{
    public class EmiteNFeContingenciaFixture
    {
        public Mock<INotaFiscalRepository> notaFiscalRepositoryMock = new Mock<INotaFiscalRepository>();
        public Mock<IEmitenteRepository> emissorServiceMock = new Mock<IEmitenteRepository>();
        public Mock<IConsultarNotaFiscalService> nfeConsultaMock = new Mock<IConsultarNotaFiscalService>();
        public Mock<IServiceFactory> serviceFactoryMock = new Mock<IServiceFactory>();
        public Mock<CertificadoService> certificadoServiceMock = new Mock<CertificadoService>();
        public Mock<SefazSettings> sefazSettingsMock = new Mock<SefazSettings>();


        public List<NotaFiscalEntity> NotasContingencia
        {
            get
            {
                return new List<NotaFiscalEntity> { 
                    new NotaFiscalEntity { Modelo = "55" },
                    new NotaFiscalEntity { Modelo = "65" }
                };
            }
        }
    }
}