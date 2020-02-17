using System.Collections.Generic;
using NFe.Core.Entitities;

namespace NFe.Core.Interfaces
{
    public interface IGrupoImpostosRepository
    {
        int Salvar(GrupoImpostos grupoImpostos);
        void Excluir(GrupoImpostos grupoImpostos);
        List<GrupoImpostos> GetAll();
        GrupoImpostos GetById(int id);
    }
}