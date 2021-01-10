using System;
using NFe.Core.Cadastro.Certificado;

namespace DgSystems.NFe.ViewModels
{
    public class CertificadoModel : IEquatable<CertificadoModel>
    {
        private CertificadoModel(string nome, string caminho, string numeroSerial, string senha)
        {
            Nome = nome;
            Caminho = caminho;
            NumeroSerial = numeroSerial;
            Senha = senha;
        }

        private CertificadoModel()
        {

        }

        public int Id { get; }
        public string Nome { get; }
        public string Caminho { get; }
        public string NumeroSerial { get; }
        public string Senha { get; }

        public static explicit operator CertificadoModel(CertificadoEntity other)
        {
            if (other == null) return null;

            var certificadoModel = new CertificadoModel(other.Nome, other.Caminho, other.NumeroSerial, other.Senha);
            return certificadoModel;
        }

        public static CertificadoModel CreateWithoutParameters()
        {
            return new CertificadoModel();
        }

        public static CertificadoModel CreateCertificadoArquivoLocal(string nome, string caminho, string numeroSerial, string senha)
        {
            return new CertificadoModel(nome, caminho, numeroSerial, senha);
        }

        public static CertificadoModel CreateCertificadoInstalado(string nome, string numeroSerial, string senha)
        {
            return new CertificadoModel(nome, null, numeroSerial, senha);
        }

        public bool Equals(CertificadoModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && Nome == other.Nome && Caminho == other.Caminho && NumeroSerial == other.NumeroSerial && Senha == other.Senha;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CertificadoModel) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id;
                hashCode = (hashCode * 397) ^ (Nome != null ? Nome.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Caminho != null ? Caminho.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (NumeroSerial != null ? NumeroSerial.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Senha != null ? Senha.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
