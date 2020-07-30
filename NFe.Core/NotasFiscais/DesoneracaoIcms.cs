using System;

namespace NFe.Core
{
    internal class DesoneracaoIcms
    {
        public DesoneracaoIcms(decimal valorDesonerado, MotivoDesoneracao motivoDesoneracao)
        {
            if (valorDesonerado > 0 && motivoDesoneracao == MotivoDesoneracao.NaoPreenchido)
            {
                throw new ArgumentException("Motivo desoneração inválido.");
            }

            ValorDesonerado = valorDesonerado;
            MotivoDesoneracao = motivoDesoneracao;
        }

        public decimal ValorDesonerado { get; }
        public MotivoDesoneracao MotivoDesoneracao { get; }
    }
}