using System;
using System.Collections.Generic;
using System.Linq;
using WpfApp.Models;

namespace WpfApp.Services
{
    public class PedidoService
    {
        private readonly JsonRepository<Pedido> _repo;
        private List<Pedido> _cache;

        public PedidoService()
        {
            _repo = new JsonRepository<Pedido>("pedidos.json");
            _cache = _repo.Load();
        }

        public IEnumerable<Pedido> GetAll()
        {
            return _cache.OrderByDescending(p => p.DataVenda).ToList();
        }

        public IEnumerable<Pedido> GetByPessoa(int pessoaId)
        {
            return _cache.Where(p => p.PessoaId == pessoaId)
                         .OrderByDescending(p => p.DataVenda)
                         .ToList();
        }

        public IEnumerable<Pedido> FilterByStatus(int pessoaId, StatusPedido? status)
        {
            var query = _cache.Where(p => p.PessoaId == pessoaId);
            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);
            return query.OrderByDescending(p => p.DataVenda).ToList();
        }

        public Pedido Finalizar(Pedido pedido)
        {
            if (pedido == null) throw new ArgumentNullException("pedido");
            if (pedido.PessoaId <= 0)
                throw new ArgumentException("Selecione uma pessoa para o pedido.");
            if (pedido.Itens == null || !pedido.Itens.Any())
                throw new ArgumentException("Adicione ao menos um produto ao pedido.");
            if (pedido.Itens.Any(i => i.Quantidade <= 0))
                throw new ArgumentException("Quantidade dos itens deve ser maior que zero.");

            pedido.RecalcularTotal();
            pedido.Status = StatusPedido.Pendente;
            pedido.DataVenda = DateTime.Now;
            pedido.Id = _cache.Any() ? _cache.Max(p => p.Id) + 1 : 1;
            _cache.Add(pedido);
            _repo.Save(_cache);
            return pedido;
        }

        public void AtualizarStatus(int pedidoId, StatusPedido novoStatus)
        {
            var pedido = _cache.FirstOrDefault(p => p.Id == pedidoId);
            if (pedido == null) throw new InvalidOperationException("Pedido não encontrado.");
            pedido.Status = novoStatus;
            _repo.Save(_cache);
        }

        public DateTime MarcarComoPago(int pedidoId)
        {
            var pedido = _cache.FirstOrDefault(p => p.Id == pedidoId);
            if (pedido == null) throw new InvalidOperationException("Pedido não encontrado.");
            pedido.Status = StatusPedido.Pago;
            pedido.DataPagamento = DateTime.Now;
            _repo.Save(_cache);
            return pedido.DataPagamento.Value;
        }

        public void DesfazerPagamento(int pedidoId)
        {
            var pedido = _cache.FirstOrDefault(p => p.Id == pedidoId);
            if (pedido == null) throw new InvalidOperationException("Pedido não encontrado.");
            pedido.Status = StatusPedido.Pendente;
            pedido.DataPagamento = null;
            _repo.Save(_cache);
        }
    }
}
