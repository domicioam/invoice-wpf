using System;

namespace NFe.Core
{
    /// <summary>
    /// Classe para cálculo da desoneração do ICMS. Atualmente o valor é inserido manualmente pelo usuário. Ver a possibilidade de fazer os cálculos usando a base de cálculo simples e por fora.
    /// </summary>
    internal class DesoneracaoIcms
    {
        /// <summary>
        /// O valor desonerado deve ser calculado manualmente e inserido na aplicação.
        /// </summary>
        /// <param name="valorDesonerado"></param>
        /// <param name="motivoDesoneracao"></param>
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