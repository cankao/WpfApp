# WpfApp - Cadastro de Pessoas, Produtos e Pedidos

Aplicação desktop WPF (.NET Framework 4.6) para cadastro e manipulação de
**Pessoas**, **Produtos** e **Pedidos**, com persistência em arquivos JSON e
manipulação de dados via **LINQ**, seguindo o padrão **MVVM**.

## Stack

- C# / .NET Framework 4.6
- WPF (XAML)
- MVVM (sem framework externo, `INotifyPropertyChanged` + `ICommand`)
- Persistência: arquivos JSON (`Data/pessoas.json`, `Data/produtos.json`, `Data/pedidos.json`)
- LINQ para consultas e filtros

## Dependências

| Pacote           | Versão  | Origem |
|------------------|---------|--------|
| Newtonsoft.Json  | 13.0.3  | NuGet  |

A referência ao pacote está em `WpfApp/packages.config`. Na primeira build o
Visual Studio (ou `nuget restore`) baixará o pacote para `packages/Newtonsoft.Json.13.0.3`.

## Estrutura de pastas

```
WpfApp/
├── WpfApp.sln
├── README.md
└── WpfApp/
    ├── WpfApp.csproj
    ├── App.xaml / App.xaml.cs
    ├── App.config
    ├── packages.config
    ├── Properties/AssemblyInfo.cs
    ├── Models/
    │   ├── Pessoa.cs
    │   ├── Produto.cs
    │   ├── Pedido.cs
    │   ├── ItemPedido.cs
    │   ├── FormaPagamento.cs   (enum: Dinheiro, Cartao, Boleto)
    │   └── StatusPedido.cs     (enum: Pendente, Pago, Enviado, Recebido)
    ├── Services/
    │   ├── CpfValidator.cs     (validação e formatação de CPF)
    │   ├── JsonRepository.cs   (persistência genérica em JSON)
    │   ├── PessoaService.cs
    │   ├── ProdutoService.cs
    │   └── PedidoService.cs
    ├── ViewModels/
    │   ├── ViewModelBase.cs
    │   ├── RelayCommand.cs
    │   ├── MainViewModel.cs
    │   ├── PessoaViewModel.cs
    │   ├── ProdutoViewModel.cs
    │   └── PedidoViewModel.cs
    ├── Views/
    │   ├── MainWindow.xaml(.cs)
    │   ├── PessoaView.xaml(.cs)
    │   ├── ProdutoView.xaml(.cs)
    │   └── PedidoView.xaml(.cs)
    ├── Data/        (arquivos JSON gerados em runtime)
    └── Resources/   (ícones, imagens)
```

## Como compilar e executar

### Opção A — Visual Studio (recomendado)

1. Abra `WpfApp.sln` no Visual Studio 2017 ou superior.
2. Certifique-se de ter o **.NET Framework 4.6 Developer Pack** instalado.
3. No menu: **Build → Restore NuGet Packages**.
4. Tecle **F5** para compilar e executar.

### Opção B — Linha de comando

```powershell
# Na raiz da solução (onde está WpfApp.sln):
nuget restore WpfApp.sln
msbuild WpfApp.sln /p:Configuration=Debug
.\WpfApp\bin\Debug\WpfApp.exe
```

> O `msbuild` precisa estar no PATH (instalado com Visual Studio ou Build Tools).
> O `nuget.exe` pode ser baixado em https://www.nuget.org/downloads.

## Persistência

Os dados são gravados em JSON na pasta `WpfApp/bin/<Configuration>/Data/`:

- `pessoas.json`
- `produtos.json`
- `pedidos.json`

Os arquivos são criados automaticamente na primeira gravação. Para resetar
basta apagá-los.

## Regras de negócio

### Pessoa
- `Id` — gerado automaticamente, somente leitura.
- `Nome` — obrigatório, filtrável.
- `CPF` — obrigatório, validado (algoritmo dos dígitos verificadores) e único, filtrável.
- `Endereço` — opcional.

### Produto
- `Id` — gerado automaticamente.
- `Nome` — obrigatório, filtrável.
- `Código` — obrigatório, único, filtrável.
- `Valor` — obrigatório, maior que zero; filtro por faixa (mín/máx).

### Pedido
- `Id` — gerado automaticamente.
- `Pessoa` — obrigatória (seleção).
- `Produtos` — obrigatório (lista com quantidades); pelo menos um item.
- `Valor Total` — calculado automaticamente (`Σ valorUnitário × quantidade`).
- `Data da Venda` — preenchida automaticamente no ato da finalização.
- `Forma de Pagamento` — obrigatória (`Dinheiro`, `Cartao`, `Boleto`).
- `Status` — inicia como `Pendente` ao finalizar e pode evoluir para
  `Pago`, `Enviado` ou `Recebido` através dos botões de ação da grid de
  pedidos da pessoa.

**Fluxo de finalização**:
- Enquanto não finalizado, o pedido vive apenas em memória.
- Trocar de aba ou fechar a janela sem finalizar **descarta** o rascunho.
- Após finalizado, os campos do pedido ficam bloqueados — só o `Status`
  pode ser alterado (pelas ações da grid de pedidos da pessoa).

## Telas

### Pessoas (aba 1)
- Filtros: **Nome** e **CPF** (com debounce de 300ms).
- Grid com todas as pessoas; ao abrir a aba a **primeira pessoa é
  selecionada automaticamente**, já carregando seus pedidos abaixo.
- Mudar a seleção atualiza tanto o painel de edição quanto a lista de
  pedidos da pessoa.
- Ações: **Incluir**, **Editar**, **Salvar**, **Excluir**.
- Botão **Incluir Pedido**: navega para a aba *Pedidos* já com a pessoa
  selecionada.
- Filtros sobre os pedidos da pessoa: todos / apenas pendentes de
  pagamento / apenas pagos / apenas entregues (recebidos).
- Grid de pedidos da pessoa com colunas: ID, Data Venda, Valor Total,
  Forma Pagamento, Dt. Pagto, Status — e por linha as ações **Pago /
  Não pago**, **Enviado** e **Recebido**.
- Cores das linhas: verde quando pago, vermelho quando enviado/recebido
  sem pagamento registrado.

### Produtos (aba 2)
- Filtros: **Nome**, **Código** e **faixa de valor** (de / até).
- Grid com todos os produtos.
- Ações: **Incluir**, **Editar**, **Salvar**, **Excluir**.

### Pedidos (aba 3)
- Seleção de **Pessoa**.
- Adição de múltiplos produtos com quantidades (botão **Adicionar**);
  adicionar um produto já existente no pedido soma à quantidade existente.
- Botão **Remover** por item na grid de itens do pedido.
- Cálculo automático do **Valor Total** após cada inclusão/remoção.
- Seleção da **Forma de Pagamento**.
- Botão **Novo Pedido** descarta o rascunho atual e começa outro.
- Botão **Finalizar**: valida (pessoa selecionada, pelo menos um item,
  quantidades > 0), calcula total, salva e bloqueia a edição.
- Listagem à direita com todos os pedidos cadastrados (somente leitura),
  colunas: ID, Pessoa, Data, Total, Pagamento, Dt. Pagto, Status.

## Decisões técnicas

- **LINQ** é usado em todos os filtros e buscas (ver `*Service.cs`).
- **MVVM** sem framework externo — `ViewModelBase` (`INotifyPropertyChanged`)
  e `RelayCommand` (`ICommand`).
- Cada `Pedido` armazena snapshots de **nome/código/valor** do produto e do
  nome da pessoa, garantindo que edições/exclusões posteriores não corrompam
  pedidos históricos.
- Validação de **CPF** com algoritmo dos dígitos verificadores; duplicidade
  comparada pelos 11 dígitos (independente da formatação).
- Estilo global de `DataGrid` em `App.xaml` aplicado a todas as listas:
  colunas não reordenáveis, não redimensionáveis e não ordenáveis;
  barra de rolagem vertical sempre visível (horizontal sob demanda);
  cabeçalho fixo no topo durante rolagem (comportamento padrão do WPF).

## Limitações conhecidas

- Sem testes automatizados.
- Persistência síncrona em arquivos (adequada para volume baixo/médio).
- Sem tela de impressão/relatório.
