namespace NFe.Core.NotasFiscais
{
    public class Numeracao
    {
        public Numeracao(int serie, int numero)
        {
            Serie = serie;
            Numero = numero;
        }

        public int Serie { get; }
        public int Numero { get; }
    }
}
