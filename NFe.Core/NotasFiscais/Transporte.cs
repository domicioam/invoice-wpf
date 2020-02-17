namespace NFe.Core.NotasFiscais
{
    public enum ModalidadeFrete
    {
        Emitente,
        Destinatario,
        Terceiros,
        TransporteProprioRemetente,
        TransporteProprioDestinatario,
        SemFrete
    }

    public class Transporte
    {
        public Transporte(Modelo modeloNota, Transportadora transportadora, Veiculo veiculo)
        {
            if (modeloNota == Modelo.Modelo65)
            {
                ModalidadeFrete = ModalidadeFrete.SemFrete;
            }
            else
            {
                Transportadora = transportadora;
                Veiculo = veiculo;
                ModalidadeFrete = ModalidadeFrete.Destinatario; //Criar campo para preencher
            }
        }

        public ModalidadeFrete ModalidadeFrete { get; set; }
        public Transportadora Transportadora { get; set; }
        public Veiculo Veiculo { get; set; }
    }
}