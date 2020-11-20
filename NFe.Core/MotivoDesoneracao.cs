using NFe.Core.Extensions;
using NFe.Core.XmlSchemas.NfeAutorizacao.Envio;
using System;

namespace NFe.Core
{
    public class MotivoDesoneracao : Enumeration
    {
        public static readonly MotivoDesoneracao NaoPreenchido = new MotivoDesoneracao(0, "NaoPreenchido");
        public static readonly MotivoDesoneracao Taxi = new MotivoDesoneracao(1, "Taxi");
        public static readonly MotivoDesoneracao ProdutorAgropecuario = new MotivoDesoneracao(3, "ProdutorAgropecuario");
        public static readonly MotivoDesoneracao FrotistaLocadora = new MotivoDesoneracao(4, "FrotistaLocadora");
        public static readonly MotivoDesoneracao DiplomaticoConsultar = new MotivoDesoneracao(5, "DiplomaticoConsultar");
        public static readonly MotivoDesoneracao UtilitariosAmazoniaAreasLivreComercio = new MotivoDesoneracao(6, "UtilitariosAmazoniaAreasLivreComercio");
        public static readonly MotivoDesoneracao Suframa = new MotivoDesoneracao(7, "Suframa");
        public static readonly MotivoDesoneracao VendaOrgaoPublico = new MotivoDesoneracao(8, "VendaOrgaoPublico");
        public static readonly MotivoDesoneracao Outros = new MotivoDesoneracao(9, "Outros");
        public static readonly MotivoDesoneracao DeficienteCondutor = new MotivoDesoneracao(10, "DeficienteCondutor");
        public static readonly MotivoDesoneracao DeficienteNaoCondutor = new MotivoDesoneracao(11, "DeficienteNaoCondutor");
        public static readonly MotivoDesoneracao OlimpiadasRio2016 = new MotivoDesoneracao(16, "OlimpiadasRio2016");

        public MotivoDesoneracao(int id, string name) : base(id, name)
        {
        }

        public static implicit operator TNFeInfNFeDetImpostoICMSICMS40MotDesICMS(MotivoDesoneracao motivoDesoneracao)
        {
            if (motivoDesoneracao == MotivoDesoneracao.Taxi)
                return TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item1;
            else if (motivoDesoneracao == MotivoDesoneracao.ProdutorAgropecuario)
                return TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item3;
            else if (motivoDesoneracao == MotivoDesoneracao.FrotistaLocadora)
                return TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item4;
            else if (motivoDesoneracao == MotivoDesoneracao.DiplomaticoConsultar)
                return TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item5;
            else if (motivoDesoneracao == MotivoDesoneracao.UtilitariosAmazoniaAreasLivreComercio)
                return TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item6;
            else if (motivoDesoneracao == MotivoDesoneracao.Suframa)
                return TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item7;
            else if (motivoDesoneracao == MotivoDesoneracao.VendaOrgaoPublico)
                return TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item8;
            else if (motivoDesoneracao == MotivoDesoneracao.Outros)
                return TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item9;
            else if (motivoDesoneracao == MotivoDesoneracao.DeficienteCondutor)
                return TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item10;
            else if (motivoDesoneracao == MotivoDesoneracao.DeficienteNaoCondutor)
                return TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item11;
            else if (motivoDesoneracao == MotivoDesoneracao.OlimpiadasRio2016)
                return TNFeInfNFeDetImpostoICMSICMS40MotDesICMS.Item16;
            else
                throw new InvalidOperationException();
        }
    }
}