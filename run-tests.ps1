# MQTT Gateway Tests - PowerShell Test Runner
# ============================================

param(
    [string]$TestType = "all",
    [switch]$Coverage = $false,
    [switch]$Verbose = $false,
    [string]$Filter = ""
)

Write-Host "🧪 MQTT Gateway Server - Test Runner" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green
Write-Host ""

# Navegar para o diretório de testes
$TestsPath = Join-Path $PSScriptRoot "Solution\MqttGateway.Tests"
if (!(Test-Path $TestsPath)) {
    Write-Host "❌ Diretório de testes não encontrado: $TestsPath" -ForegroundColor Red
    exit 1
}

Set-Location $TestsPath

# Verificar se o .NET está disponível
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET encontrado: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET não encontrado. Instale o .NET 8 SDK." -ForegroundColor Red
    exit 1
}

# Função para executar testes
function Run-Tests {
    param(
        [string]$Category,
        [string]$FilterExpression,
        [string]$DisplayName
    )
    
    Write-Host ""
    Write-Host "🔬 Executando $DisplayName..." -ForegroundColor Cyan
    Write-Host "$(Get-Date -Format 'HH:mm:ss') - Iniciando testes" -ForegroundColor Gray
    
    $testCommand = "dotnet test"
    
    if ($Coverage) {
        $testCommand += " --collect:`"XPlat Code Coverage`""
    }
    
    if ($Verbose) {
        $testCommand += " --verbosity detailed"
    } else {
        $testCommand += " --verbosity normal"
    }
    
    if ($FilterExpression) {
        $testCommand += " --filter `"$FilterExpression`""
    }
    
    $testCommand += " --logger `"console;verbosity=normal`""
    
    Write-Host "Comando: $testCommand" -ForegroundColor Gray
    
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    
    Invoke-Expression $testCommand
    $testResult = $LASTEXITCODE
    
    $stopwatch.Stop()
    $duration = $stopwatch.Elapsed.ToString("mm\:ss")
    
    if ($testResult -eq 0) {
        Write-Host "✅ $DisplayName concluídos com sucesso em $duration" -ForegroundColor Green
    } else {
        Write-Host "❌ $DisplayName falharam (código: $testResult) após $duration" -ForegroundColor Red
    }
    
    return $testResult
}

# Restaurar pacotes NuGet
Write-Host "📦 Restaurando pacotes NuGet..." -ForegroundColor Yellow
dotnet restore --verbosity quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Falha ao restaurar pacotes NuGet" -ForegroundColor Red
    exit 1
}

# Compilar projeto de testes
Write-Host "🔨 Compilando projeto de testes..." -ForegroundColor Yellow
dotnet build --no-restore --verbosity quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Falha na compilação" -ForegroundColor Red
    exit 1
}

$overallResult = 0

# Executar testes baseado no tipo solicitado
switch ($TestType.ToLower()) {
    "unit" {
        $result = Run-Tests "Unit" "FullyQualifiedName~Unit" "Testes Unitários"
        $overallResult = [Math]::Max($overallResult, $result)
    }
    
    "integration" {
        $result = Run-Tests "Integration" "FullyQualifiedName~Integration" "Testes de Integração"
        $overallResult = [Math]::Max($overallResult, $result)
    }
    
    "performance" {
        $result = Run-Tests "Performance" "FullyQualifiedName~Performance" "Testes de Performance"
        $overallResult = [Math]::Max($overallResult, $result)
    }
    
    "e2e" {
        $result = Run-Tests "EndToEnd" "FullyQualifiedName~EndToEnd" "Testes End-to-End"
        $overallResult = [Math]::Max($overallResult, $result)
    }
    
    "smoke" {
        # Testes rápidos para verificação básica
        $smokeFilter = "FullyQualifiedName~Unit`&Category!=Slow"
        $result = Run-Tests "Smoke" $smokeFilter "Testes de Smoke (verificação rápida)"
        $overallResult = [Math]::Max($overallResult, $result)
    }
    
    "all" {
        Write-Host "🚀 Executando suite completa de testes..." -ForegroundColor Cyan
        
        # 1. Testes Unitários
        $result = Run-Tests "Unit" "FullyQualifiedName~Unit" "Testes Unitários"
        $overallResult = [Math]::Max($overallResult, $result)
        
        if ($result -eq 0) {
            # 2. Testes de Integração
            $result = Run-Tests "Integration" "FullyQualifiedName~Integration`&FullyQualifiedName!~EndToEnd`&FullyQualifiedName!~Performance" "Testes de Integração"
            $overallResult = [Math]::Max($overallResult, $result)
            
            if ($result -eq 0) {
                # 3. Testes End-to-End
                $result = Run-Tests "EndToEnd" "FullyQualifiedName~EndToEnd" "Testes End-to-End"
                $overallResult = [Math]::Max($overallResult, $result)
                
                # 4. Testes de Performance (opcionais, só se outros passaram)
                if ($result -eq 0) {
                    Write-Host ""
                    Write-Host "⚡ Testes de performance são opcionais. Executar? (Y/n)" -ForegroundColor Yellow
                    $runPerf = Read-Host
                    
                    if ($runPerf -eq "" -or $runPerf.ToLower() -eq "y") {
                        $result = Run-Tests "Performance" "FullyQualifiedName~Performance" "Testes de Performance"
                        # Performance tests não afetam o resultado geral (são informativos)
                    }
                }
            }
        }
    }
    }
    
    "custom" {
        if ([string]::IsNullOrEmpty($Filter)) {
            Write-Host "❌ Para testes customizados, use o parâmetro -Filter" -ForegroundColor Red
            Write-Host "Exemplo: .\run-tests.ps1 -TestType custom -Filter 'FullyQualifiedName~SessionManager'" -ForegroundColor Yellow
            exit 1
        }
        
        $result = Run-Tests "Custom" $Filter "Testes Customizados"
        $overallResult = [Math]::Max($overallResult, $result)
    }
    
    default {
        Write-Host "❌ Tipo de teste inválido: $TestType" -ForegroundColor Red
        Write-Host "Tipos válidos: unit, integration, performance, e2e, smoke, all, custom" -ForegroundColor Yellow
        exit 1
    }
}

# Processar cobertura se solicitada
if ($Coverage -and $overallResult -eq 0) {
    Write-Host ""
    Write-Host "📊 Processando relatório de cobertura..." -ForegroundColor Cyan
    
    # Procurar por arquivos de cobertura
    $coverageFiles = Get-ChildItem -Path "TestResults" -Filter "coverage.cobertura.xml" -Recurse -ErrorAction SilentlyContinue
    
    if ($coverageFiles.Count -gt 0) {
        Write-Host "✅ Arquivos de cobertura encontrados:" -ForegroundColor Green
        foreach ($file in $coverageFiles) {
            Write-Host "   📄 $($file.FullName)" -ForegroundColor Gray
        }
        
        # Tentar gerar relatório HTML se reportgenerator estiver disponível
        try {
            $reportGenerator = dotnet tool list -g | Select-String "dotnet-reportgenerator-globaltool"
            if ($reportGenerator) {
                $reportPath = Join-Path (Get-Location) "TestResults\CoverageReport"
                dotnet reportgenerator -reports:"TestResults\**\coverage.cobertura.xml" -targetdir:$reportPath -reporttypes:Html
                
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "📊 Relatório de cobertura gerado: $reportPath\index.html" -ForegroundColor Green
                }
            }
        } catch {
            Write-Host "⚠️  Para relatórios HTML, instale: dotnet tool install -g dotnet-reportgenerator-globaltool" -ForegroundColor Yellow
        }
    } else {
        Write-Host "⚠️  Nenhum arquivo de cobertura encontrado" -ForegroundColor Yellow
    }
}

# Resumo final
Write-Host ""
Write-Host "📋 Resumo da Execução" -ForegroundColor Cyan
Write-Host "===================" -ForegroundColor Cyan
Write-Host "Tipo de teste: $TestType" -ForegroundColor White
Write-Host "Cobertura: $(if ($Coverage) { 'Habilitada' } else { 'Desabilitada' })" -ForegroundColor White
Write-Host "Verbose: $(if ($Verbose) { 'Habilitado' } else { 'Desabilitado' })" -ForegroundColor White

if ($overallResult -eq 0) {
    Write-Host "🎉 Todos os testes executados com sucesso!" -ForegroundColor Green
} else {
    Write-Host "💥 Alguns testes falharam. Verifique a saída acima." -ForegroundColor Red
}

Write-Host ""
Write-Host "📚 Comandos úteis:" -ForegroundColor Cyan
Write-Host "  Unit tests:        .\run-tests.ps1 -TestType unit" -ForegroundColor Gray
Write-Host "  Integration:       .\run-tests.ps1 -TestType integration" -ForegroundColor Gray
Write-Host "  With coverage:     .\run-tests.ps1 -TestType all -Coverage" -ForegroundColor Gray
Write-Host "  Specific test:     .\run-tests.ps1 -TestType custom -Filter 'ClassName~SessionManager'" -ForegroundColor Gray

exit $overallResult