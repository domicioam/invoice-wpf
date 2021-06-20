using System;
using NFe.Core.Entitities;
using NFe.Interfaces;

namespace NFe.Core.NotasFiscais
{
    public class NotaInutilizada : IXmlFileWritable
    {
        public int Id { get; set; }
        public string Serie { get; set; }
        public string Numero { get; set; }
        public int Modelo { get; set; }
        public DateTime DataInutilizacao { get; set; }
        public string IdInutilizacao { get; set; }
        public string Protocolo { get; set; }
        public string Motivo { get; set; }
        public string XmlPath { get; set; }

        public string FileName
        {
            get { return IdInutilizacao + "-procInutNFe.xml"; }
        }

        public static explicit operator NotaInutilizada(NotaInutilizadaEntity emitenteEntity)
        {
            if (emitenteEntity == null) return null;

            return new NotaInutilizada
            {
                Id = emitenteEntity.Id,
                Serie = emitenteEntity.Serie,
                Numero = emitenteEntity.Numero,
                DataInutilizacao = emitenteEntity.DataInutilizacao,
                IdInutilizacao = emitenteEntity.IdInutilizacao,
                Modelo = emitenteEntity.Modelo,
                Motivo = emitenteEntity.Motivo,
                Protocolo = emitenteEntity.Protocolo,
                XmlPath = emitenteEntity.XmlPath
            };
        }

        public static explicit operator NotaInutilizadaEntity(NotaInutilizada emitenteEntity)
        {
            return new NotaInutilizadaEntity
            {
                Id = emitenteEntity.Id,
                Serie = emitenteEntity.Serie,
                Numero = emitenteEntity.Numero,
                DataInutilizacao = emitenteEntity.DataInutilizacao,
                IdInutilizacao = emitenteEntity.IdInutilizacao,
                Modelo = emitenteEntity.Modelo,
                Motivo = emitenteEntity.Motivo,
                Protocolo = emitenteEntity.Protocolo,
                XmlPath = emitenteEntity.XmlPath
            };
        }
    }
}