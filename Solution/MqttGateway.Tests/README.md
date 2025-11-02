# 🧪 MQTT Gateway Server - Projeto de Testes

## 📋 Visão Geral

Este projeto contém uma suite completa de testes para o MQTT Gateway Server, seguindo as melhores práticas da indústria para garantir qualidade, confiabilidade e performance do sistema.

## 🏗️ Estrutura do Projeto

```
MqttGateway.Tests/
├── Unit/                    # Testes unitários
│   ├── Controllers/         # Testes dos controllers
│   ├── Services/           # Testes dos serviços
│   └── Hubs/               # Testes dos hubs SignalR
├── Integration/            # Testes de integração
├── Performance/            # Testes de performance
├── Fixtures/               # Fixtures e configurações de teste
├── Helpers/                # Classes auxiliares para testes
└── appsettings.Test.json   # Configurações específicas para testes
```

## 🔧 Tecnologias e Frameworks

### Frameworks de Teste
- **xUnit** - Framework principal de testes
- **FluentAssertions** - Assertions mais legíveis e expressivas
- **Moq** - Mocking framework para isolamento de dependências

### Testes de Integração
- **Microsoft.AspNetCore.Mvc.Testing** - Testes de API REST
- **Microsoft.AspNetCore.SignalR.Client** - Testes de SignalR
- **MQTTnet.TestMqttServer** - Servidor MQTT em memória para testes

### Ferramentas Auxiliares
- **Testcontainers** - Containers Docker para testes (se necessário)
- **Coverlet** - Análise de cobertura de código

## 🎯 Tipos de Testes

### 1. **Testes Unitários** (`Unit/`)
Testam componentes individuais em isolamento completo.

**Características:**
- ⚡ Execução rápida (< 100ms por teste)
- 🔒 Isolamento total com mocks
- 📊 Alta cobertura de código
- 🧩 Testa lógica de negócio específica

**Cobertura:**
- `SessionContextStore` - Armazenamento de contexto
- `SessionManagerService` - Gerenciamento de sessões
- `SignalRMessageRelay` - Relay de mensagens
- `MessageController` - Controller de mensagens

### 2. **Testes de Integração** (`Integration/`)
Testam a integração entre componentes reais.

**Características:**
- 🔄 Componentes reais trabalhando juntos
- 🌐 API REST e SignalR funcionais
- 📡 Comunicação real entre camadas
- ⚙️ Configuração próxima ao ambiente real

**Cenários:**
- API REST endpoints
- Conexões SignalR Hub
- Fluxo completo de mensagens
- Isolamento entre sessões

### 3. **Testes End-to-End** (`Integration/EndToEndIntegrationTests.cs`)
Testam o fluxo completo do sistema com MQTT real.

**Características:**
- 🔄 Fluxo completo: API → MQTT → SignalR
- 📊 Servidor MQTT real em memória
- 🔗 Múltiplas sessões e clientes
- 💾 Persistência de contexto

### 4. **Testes de Performance** (`Performance/`)
Validam requisitos não-funcionais e identificam gargalos.

**Métricas Validadas:**
- ⚡ Latência de API (< 100ms)
- 🚀 Throughput (> 20 req/s)
- 🔌 Tempo de conexão SignalR
- 💾 Uso de memória
- 📈 Performance sob carga

## 🚀 Executando os Testes

### Script PowerShell (Recomendado)

```powershell
# Executar todos os testes
.\run-tests.ps1

# Apenas testes unitários
.\run-tests.ps1 -TestType unit

# Testes de integração
.\run-tests.ps1 -TestType integration

# Testes com cobertura de código
.\run-tests.ps1 -TestType all -Coverage

# Testes de performance
.\run-tests.ps1 -TestType performance

# Testes específicos
.\run-tests.ps1 -TestType custom -Filter "ClassName~SessionManager"
```

### Comandos .NET CLI

```bash
# Restaurar pacotes
dotnet restore

# Executar todos os testes
dotnet test

# Testes unitários apenas
dotnet test --filter "FullyQualifiedName~Unit"

# Testes de integração
dotnet test --filter "FullyQualifiedName~Integration"

# Com cobertura de código
dotnet test --collect:"XPlat Code Coverage"

# Verboso para debugging
dotnet test --verbosity detailed
```

## 📊 Cobertura de Código

### Configuração
A cobertura é coletada automaticamente com o parâmetro `-Coverage`:

```powershell
.\run-tests.ps1 -TestType all -Coverage
```

### Relatórios HTML
Para gerar relatórios HTML, instale o ReportGenerator:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

O script executará automaticamente e gerará relatórios em `TestResults/CoverageReport/`.

### Metas de Cobertura
- **Serviços**: > 90%
- **Controllers**: > 85%
- **Hubs**: > 80%
- **Geral**: > 85%

## 🏷️ Convenções e Padrões

### Nomenclatura de Testes
```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    // Act  
    // Assert
}
```

### Estrutura AAA (Arrange-Act-Assert)
```csharp
[Fact]
public void CreateContext_WhenSessionDoesNotExist_ShouldReturnTrue()
{
    // Arrange - Configurar dados e mocks
    var sessionId = Guid.NewGuid();
    var startMessage = "Initial message";

    // Act - Executar ação sendo testada
    var result = _sessionContextStore.CreateContext(sessionId, startMessage);

    // Assert - Verificar resultados
    result.Should().BeTrue();
}
```

### Uso de Theory para Testes Parametrizados
```csharp
[Theory]
[InlineData("")]
[InlineData("Simple message")]
[InlineData("Message with special chars: àáâãäåæçèéêë")]
public void CreateContext_WithDifferentStartMessages_ShouldWork(string startMessage)
{
    // Test implementation
}
```

## 🛠️ Fixtures e Helpers

### WebApplicationFactory
```csharp
public class MqttGatewayWebApplicationFactory : WebApplicationFactory<Program>
{
    // Configuração customizada para testes
    // Mocks automáticos de serviços
    // Configuração de ambiente de teste
}
```

### SignalR Test Helper
```csharp
await using var signalRHelper = new SignalRTestHelper();
await signalRHelper.ConnectAsync(hubUrl, sessionId);
var messageReceived = await signalRHelper.WaitForMessageAsync(...);
```

### MQTT Test Client
```csharp
await using var mqttClient = new MqttTestClient();
await mqttClient.ConnectAsync("localhost", 1883);
await mqttClient.PublishAsync("topic", "message");
```

## 🐛 Debugging de Testes

### Logs Detalhados
```powershell
.\run-tests.ps1 -Verbose
```

### Debugging no Visual Studio
1. Colocar breakpoints nos testes
2. Clicar com botão direito → "Debug Test"
3. Usar "Test Explorer" para navegação

### Debugging de Testes Assíncronos
```csharp
[Fact]
public async Task TestMethod()
{
    // Use await adequadamente
    // Evite .Result ou .Wait()
    // Configure timeouts apropriados
}
```

## 📈 Métricas e Monitoramento

### Tempo de Execução
- **Unit**: < 5 segundos total
- **Integration**: < 30 segundos total  
- **Performance**: < 60 segundos total
- **E2E**: < 120 segundos total

### Thresholds de Performance
```csharp
// API Response Time
response.ElapsedMilliseconds.Should().BeLessThan(100);

// Throughput
throughput.Should().BeGreaterThan(20); // req/s

// Memory Usage
memoryIncrease.Should().BeLessThan(50 * 1024 * 1024); // 50MB
```

## 🔄 CI/CD Integration

### Pipeline de Testes
1. **Build** - Compilação do projeto
2. **Unit Tests** - Testes rápidos para feedback imediato
3. **Integration Tests** - Testes de integração
4. **Coverage Report** - Análise de cobertura
5. **Performance Tests** - Validação de SLAs (opcional)

### GitHub Actions (Exemplo)
```yaml
name: Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Run Unit Tests
        run: dotnet test --filter "FullyQualifiedName~Unit" --collect:"XPlat Code Coverage"
      
      - name: Run Integration Tests  
        run: dotnet test --filter "FullyQualifiedName~Integration"
```

## 🚨 Troubleshooting

### Problemas Comuns

#### ❌ "Port already in use"
**Solução:** Usar `TestMqttServerFixture` que encontra portas disponíveis automaticamente.

#### ❌ "SignalR connection timeout"
**Solução:** Verificar se `WebApplicationFactory` está configurada corretamente.

#### ❌ "MQTT connection failed"
**Solução:** Usar servidor MQTT em memória para testes de integração.

#### ❌ "Tests flaky/intermittent"
**Solução:** 
- Usar `WaitForMessageAsync` com timeouts adequados
- Evitar `Thread.Sleep`, usar `Task.Delay`
- Implementar retry logic para operações de rede

### Debugging Tips
```csharp
// Adicionar outputs para debugging
_output.WriteLine($"Message received: {message}");

// Usar timeouts generosos em debugging
var timeout = Debugger.IsAttached ? TimeSpan.FromMinutes(5) : TimeSpan.FromSeconds(5);

// Verificar estado antes de assertions
_output.WriteLine($"Received messages count: {signalRHelper.ReceivedMessages.Count}");
```

## 📚 Recursos Adicionais

### Documentação
- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [ASP.NET Core Testing](https://docs.microsoft.com/en-us/aspnet/core/test/)
- [SignalR Testing](https://docs.microsoft.com/en-us/aspnet/core/signalr/test)

### Boas Práticas
- **FIRST Principles**: Fast, Independent, Repeatable, Self-Validating, Timely
- **Test Pyramid**: Mais unit tests, menos integration tests, poucos E2E tests
- **Fail Fast**: Testes devem falhar rapidamente quando algo está errado
- **Clean Tests**: Testes legíveis são testes mantíveis

---

**🎯 Meta**: Manter alta qualidade de código através de testes abrangentes e automatizados.