# 🚀 Comando: Deploy

## Descripción
Procedimiento para desplegar la aplicación a producción.

## Pre-requisitos
- Build exitoso sin errores
- Tests pasando
- Migraciones aplicadas

## Pasos

### 1. Verificar Build
```bash
dotnet build -c Release
```

### 2. Ejecutar Tests
```bash
dotnet test
```

### 3. Publicar
```bash
dotnet publish -c Release -o ./publish
```

### 4. Verificar Configuración
```markdown
- [ ] `appsettings.Production.json` configurado
- [ ] Connection strings correctos
- [ ] Variables de entorno definidas
- [ ] HTTPS habilitado
```

### 5. Deploy

#### IIS
```bash
# Copiar contenido de ./publish al sitio IIS
# Reiniciar Application Pool
```

#### Docker
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY ./publish .
ENTRYPOINT ["dotnet", "JSCHUB.dll"]
```

## Post-Deploy Checklist

```markdown
- [ ] Aplicación accesible
- [ ] Base de datos conectada
- [ ] Logs funcionando
- [ ] Health checks pasando
```
