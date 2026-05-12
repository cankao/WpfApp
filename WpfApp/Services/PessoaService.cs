using System;
using System.Collections.Generic;
using System.Linq;
using WpfApp.Models;

namespace WpfApp.Services
{
    public class PessoaService : ServiceBase<Pessoa>
    {
        public PessoaService() : base("pessoas.json") { }

        public IEnumerable<Pessoa> GetAll()
        {
            return _cache.OrderBy(p => p.Nome).ToList();
        }

        public Pessoa GetById(int id)
        {
            return _cache.FirstOrDefault(p => p.Id == id);
        }

        public IEnumerable<Pessoa> Search(string nome, string cpf)
        {
            var query = _cache.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(nome))
                query = query.Where(p => p.Nome != null && p.Nome.IndexOf(nome, StringComparison.OrdinalIgnoreCase) >= 0);
            if (!string.IsNullOrWhiteSpace(cpf))
            {
                var digits = CpfValidator.OnlyDigits(cpf);
                query = query.Where(p => CpfValidator.OnlyDigits(p.Cpf).Contains(digits));
            }
            return query.OrderBy(p => p.Nome).ToList();
        }

        public void Add(Pessoa pessoa)
        {
            if (pessoa == null) throw new ArgumentNullException("pessoa");
            ValidateAndNormalize(pessoa);
            EnsureUniqueCpf(pessoa);
            pessoa.Id = _cache.Any() ? _cache.Max(p => p.Id) + 1 : 1;
            _cache.Add(pessoa);
            _repo.Save(_cache);
        }

        public void Update(Pessoa pessoa)
        {
            if (pessoa == null) throw new ArgumentNullException("pessoa");
            ValidateAndNormalize(pessoa);
            EnsureUniqueCpf(pessoa);
            var existing = _cache.FirstOrDefault(p => p.Id == pessoa.Id);
            if (existing == null) throw new InvalidOperationException("Pessoa não encontrada.");
            existing.Nome = pessoa.Nome;
            existing.Cpf = pessoa.Cpf;
            existing.Endereco = pessoa.Endereco;
            _repo.Save(_cache);
        }

        public void Delete(int id)
        {
            var pessoa = _cache.FirstOrDefault(p => p.Id == id);
            if (pessoa == null) return;
            _cache.Remove(pessoa);
            _repo.Save(_cache);
        }

        private void ValidateAndNormalize(Pessoa pessoa)
        {
            if (string.IsNullOrWhiteSpace(pessoa.Nome))
                throw new ArgumentException("Nome é obrigatório.");
            if (string.IsNullOrWhiteSpace(pessoa.Cpf))
                throw new ArgumentException("CPF é obrigatório.");
            if (!CpfValidator.IsValid(pessoa.Cpf))
                throw new ArgumentException("CPF inválido.");
            pessoa.Cpf = CpfValidator.Format(pessoa.Cpf);
            pessoa.Nome = pessoa.Nome.Trim();
            if (pessoa.Endereco != null) pessoa.Endereco = pessoa.Endereco.Trim();
        }

        private void EnsureUniqueCpf(Pessoa pessoa)
        {
            var cpfDigits = CpfValidator.OnlyDigits(pessoa.Cpf);
            var duplicate = _cache.FirstOrDefault(p =>
                p.Id != pessoa.Id &&
                CpfValidator.OnlyDigits(p.Cpf) == cpfDigits);
            if (duplicate != null)
                throw new ArgumentException("Já existe outra pessoa cadastrada com este CPF.");
        }
    }
}
