using NFe.Core.Entitities;
using System;
using System.Collections.Generic;
using AutoFixture;

namespace DgSystems.NFe.Core.UnitTests.Services.Actors
{
    public class EmiteNFeContingenciaFixture
    {
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