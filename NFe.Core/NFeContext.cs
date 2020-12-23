using NFe.Core.Entitities;

namespace NFe.Core
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using NFe.Core.Cadastro.Certificado;
    using NFe.Core.Cadastro.Configuracoes;
    using NFe.Core.Cadastro.Destinatario;
    using NFe.Core.Cadastro.Ibpt;
    using NFe.Core.Cadastro.Imposto;

    public partial class NFeContext : DbContext
    {
        public NFeContext()
            : base("name=NotasFiscais")
        {
            Database.SetInitializer<NFeContext>(null);
        }

        public virtual DbSet<GrupoImpostos> GrupoImpostos { get; set; }
        public virtual DbSet<NotaFiscalEntity> NotaFiscal { get; set; }
        public virtual DbSet<ProdutoEntity> Produto { get; set; }
        public virtual DbSet<EmitenteEntity> Emitente { get; set; }
        public virtual DbSet<ConfiguracaoEntity> Configuracao { get; set; }
        public virtual DbSet<IbptEntity> Ibpt { get; set; }
        public virtual DbSet<NaturezaOperacaoEntity> NaturezaOperacao { get; set; }
        public virtual DbSet<CfopEntity> Cfop { get; set; }
        public virtual DbSet<CertificadoEntity> Certificado { get; set; }
        public virtual DbSet<EnderecoDestinatarioEntity> EnderecoDestinatario { get; set; }
        public virtual DbSet<DestinatarioEntity> Destinatario { get; set; }
        public virtual DbSet<EnderecoTransportadoraEntity> EnderecoTransportadora { get; set; }
        public virtual DbSet<TransportadoraEntity> Transportadora { get; set; }
        public virtual DbSet<EventoEntity> Evento { get; set; }
        public virtual DbSet<EstadoEntity> Estado { get; set; }
        public virtual DbSet<MunicipioEntity> Municipio { get; set; }
        public virtual DbSet<NotaInutilizadaEntity> NotaInutilizada { get; set; }
        public virtual DbSet<HistoricoEnvioContabilidade> HistoricoEnvioContabilidade { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GrupoImpostos>()
                .HasMany(e => e.Impostos)
                .WithRequired(e => e.GrupoImpostos)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<NotaFiscalEntity>()
                .Property(e => e.Modelo)
                .IsFixedLength();

            modelBuilder.Entity<NotaFiscalEntity>()
                .Property(e => e.Chave)
                .IsFixedLength();

            modelBuilder.Entity<NotaFiscalEntity>()
                .Property(e => e.Serie);

            modelBuilder.Entity<NotaFiscalEntity>()
                .Property(e => e.TipoEmissao)
                .IsFixedLength();

            modelBuilder.Entity<NotaFiscalEntity>()
                .Property(e => e.Protocolo);

            modelBuilder.Entity<ProdutoEntity>()
                .Property(e => e.UnidadeComercial)
                .IsFixedLength();

            modelBuilder.Entity<EmitenteEntity>()
                .Property(e => e.CNAE)
                .IsFixedLength();

            modelBuilder.Entity<EmitenteEntity>()
                .Property(e => e.CNPJ)
                .IsFixedLength();

            modelBuilder.Entity<EmitenteEntity>()
                .Property(e => e.InscricaoEstadual)
                .IsFixedLength();

            modelBuilder.Entity<EmitenteEntity>()
                .Property(e => e.InscricaoMunicipal)
                .IsFixedLength();

            modelBuilder.Entity<EmitenteEntity>()
                .Property(e => e.UF)
                .IsFixedLength();

            modelBuilder.Entity<DestinatarioEntity>()
                .HasOptional(e => e.Endereco)
                .WithRequired(e => e.Destinatario);

            modelBuilder.Entity<TransportadoraEntity>()
                .HasOptional(e => e.Endereco)
                .WithRequired(e => e.Transportadora)
                .WillCascadeOnDelete();
        }
    }
}
