using System;
using System.Collections.Generic;
using System.Linq;
using WpfApp.Models;

namespace WpfApp.Services
{
    public class ProdutoService : ServiceBase<Produto>
    {
        public ProdutoService() : base("produtos.json") { }

        public IEnumerable<Produto> GetAll()
        {
            return _cache.OrderBy(p => p.Nome).ToList();
        }

        public Produto GetById(int id)
        {
            return _cache.FirstOrDefault(p => p.Id == id);
        }

        public IEnumerable<Produto> Search(string nome, string codigo, decimal? valorMin, decimal? valorMax)
        {
            var query = _cache.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(nome))
                query = query.Where(p => p.Nome != null && p.Nome.IndexOf(nome, StringComparison.OrdinalIgnoreCase) >= 0);
            if (!string.IsNullOrWhiteSpace(codigo))
                query = query.Where(p => p.Codigo != null && p.Codigo.IndexOf(codigo, StringComparison.OrdinalIgnoreCase) >= 0);
            if (valorMin.HasValue)
                query = query.Where(p => p.Valor >= valorMin.Value);
            if (valorMax.HasValue)
                query = query.Where(p => p.Valor <= valorMax.Value);
            return query.OrderBy(p => p.Nome).ToList();
        }

        public void Add(Produto produto)
        {
            if (produto == null) throw new ArgumentNullException("produto");
            Validate(produto);
            EnsureUniqueCodigo(produto);
            produto.Id = _cache.Any() ? _cache.Max(p => p.Id) + 1 : 1;
            _cache.Add(produto);
            _repo.Save(_cache);
        }

        public void Update(Produto produto)
        {
            if (produto == null) throw new ArgumentNullException("produto");
            Validate(produto);
            EnsureUniqueCodigo(produto);
            var existing = _cache.FirstOrDefault(p => p.Id == produto.Id);
            if (existing == null) throw new InvalidOperationException("Produto não encontrado.");
            existing.Nome = produto.Nome;
            existing.Codigo = produto.Codigo;
            existing.Valor = produto.Valor;
            _repo.Save(_cache);
        }

        public void Delete(int id)
        {
            var produto = _cache.FirstOrDefault(p => p.Id == id);
            if (produto == null) return;
            _cache.Remove(produto);
            _repo.Save(_cache);
        }

        private void Validate(Produto produto)
        {
            if (string.IsNullOrWhiteSpace(produto.Nome))
                throw new ArgumentException("Nome do produto é obrigatório.");
            if (string.IsNullOrWhiteSpace(produto.Codigo))
                throw new ArgumentException("Código do produto é obrigatório.");
            if (produto.Valor <= 0)
                throw new ArgumentException("Valor deve ser maior que zero.");
            produto.Nome = produto.Nome.Trim();
            produto.Codigo = produto.Codigo.Trim();
        }

        private void EnsureUniqueCodigo(Produto produto)
        {
            var duplicate = _cache.FirstOrDefault(p =>
                p.Id != produto.Id &&
                string.Equals(p.Codigo, produto.Codigo, StringComparison.OrdinalIgnoreCase));
            if (duplicate != null)
                throw new ArgumentException("Já existe outro produto com este código.");
        }
    }
}
