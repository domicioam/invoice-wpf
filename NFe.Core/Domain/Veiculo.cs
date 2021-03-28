namespace NFe.Core.Domain
{
    public class Veiculo
    {
        public Veiculo(string placa, string siglaUF, string registroAntt = null)
        {
            Placa = placa;
            SiglaUF = siglaUF;
            RegistroAntt = registroAntt;
        }

        public string Placa { get; set; }
        public string SiglaUF { get; set; }
        public string RegistroAntt { get; set; }
    }
}