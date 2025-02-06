# Variáveis
$downloadUrl = "https://github.com/ffbinaries/ffbinaries-prebuilt/releases/download/v6.1/ffmpeg-6.1-win-64.zip"
$targetDir = ".\Windows"
$zipFile = Join-Path $targetDir "ffmpeg-6.1-win-64.zip"

# Cria o diretório de destino, se não existir
if (-Not (Test-Path -Path $targetDir)) {
    Write-Host "Criando o diretório de destino: $targetDir"
    New-Item -ItemType Directory -Path $targetDir | Out-Null
}

# Faz o download do arquivo
Write-Host "Fazendo download de $downloadUrl..."
Invoke-WebRequest -Uri $downloadUrl -OutFile $zipFile -UseBasicParsing

# Verifica se o download foi bem-sucedido
if (-Not (Test-Path -Path $zipFile)) {
    Write-Error "Erro: Não foi possível fazer o download do arquivo."
    exit 1
}

Write-Host "Download concluído: $zipFile"

# Descompacta o arquivo na pasta de destino
Write-Host "Descompactando o arquivo para $targetDir..."
Expand-Archive -Path $zipFile -DestinationPath $targetDir -Force

# Verifica se a descompactação foi bem-sucedida
if ($?) {
    Write-Host "Arquivo descompactado com sucesso!"
} else {
    Write-Error "Erro: Não foi possível descompactar o arquivo."
    exit 1
}

# Remove o arquivo ZIP após a extração (opciona
Write-Host "Removendo o arquivo ZIP..."
Remove-Item -Path $zipFile -Force

if (-Not (Test-Path -Path $zipFile)) {
    Write-Host "Arquivo ZIP removido com sucesso!"
} else {
    Write-Error "Erro: Não foi possível remover o arquivo ZIP."
    exit 1
}