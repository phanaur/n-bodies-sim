# Configuración de Helix para C# - Resumen

## ✅ Estado Actual

**LSP Activo:** csharp-ls (estable y funcional)
**Debugger:** netcoredbg ✓
**Indent queries:** ✓
**Auto-format:** ✓

## 🎯 Funcionalidades Disponibles

### Con csharp-ls (actual)
- ✅ Autocomplete inteligente
- ✅ Go to definition (gd)
- ✅ Find references (gr)
- ✅ Hover documentation (Space+k)
- ✅ Rename symbols (Space+r)
- ✅ Format on save
- ✅ Inlay hints (tipos y parámetros)
- ⚠️  Code actions LIMITADAS (solo imports, básicas)

### Con OmniSharp (opcional, puede fallar)
- ✅ TODO lo anterior
- ✅ Refactorizaciones completas:
  - Extract method
  - Extract interface
  - Generate constructor
  - Implement interface
  - Add null checks
- ❌ **Problema:** Incompatibilidad de protocolo con Helix
- ❌ Error: "Parse error in ServerMessage"

## 🔧 Configuración

Ubicación: `~/.config/helix/languages.toml`

## 🔄 Cambiar entre LSP

```bash
# Usar csharp-ls (recomendado, estable)
helix-csharp-toggle csharp-ls

# Probar OmniSharp (si necesitas refactorings avanzadas)
helix-csharp-toggle omnisharp
```

## 🐛 Problema Identificado

**Tu proyecto `n-bodies-sim`:**
- Usa .NET 10.0 + C# 14 (muy nuevo)
- OmniSharp envía mensajes que Helix 25.07.1 no puede parsear
- **Solución:** Usar csharp-ls que funciona perfectamente

## 📝 Atajos Útiles en Helix

- `Space + a` → Code actions (las disponibles según LSP)
- `Space + r` → Rename symbol
- `gd` → Go to definition
- `gr` → Go to references  
- `Space + k` → Hover info/documentation
- `Space + s` → Symbol picker (buscar en archivo)
- `Space + S` → Workspace symbol picker
- `=` → Format/indent selection

## 💡 Recomendación Final

**Para trabajo diario:**
- Usa csharp-ls (configuración actual)
- Tiene todo lo esencial y es estable

**Para refactorings pesadas:**
- Usa Rider o VS Code temporalmente
- O espera a que Helix actualice el soporte de OmniSharp

## 📊 Logs

Si hay problemas, revisa:
```bash
tail -f ~/.cache/helix/helix.log
```

## ✨ Tu proyecto funciona

```bash
cd ~/github/n-bodies-sim/NBodiesSim
hx Program.cs
```

El LSP se inicializará en 3-5 segundos. Verás "LSP csharp-ls" en la barra de estado.
