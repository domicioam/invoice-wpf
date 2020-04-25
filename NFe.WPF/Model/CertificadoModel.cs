using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFe.Core.Cadastro.Certificado;

namespace EmissorNFe.Model
{
    public class CertificadoModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Caminho { get; set; }
        public string NumeroSerial { get; set; }
        public string Senha { get; internal set; }

        public static explicit operator CertificadoModel(CertificadoEntity certificadoEntity)
        {
            if (certificadoEntity == null) return null;

            var certificadoModel = new CertificadoModel();
            certificadoModel.Id = certificadoEntity.Id;
            certificadoModel.Caminho = certificadoEntity.Caminho;
            certificadoModel.Nome = certificadoEntity.Nome;
            certificadoModel.NumeroSerial = certificadoEntity.NumeroSerial;
            certificadoModel.Senha = certificadoEntity.Senha;

            return certificadoModel;
        }
    }
}
