# Déploiement ElevageActifs (serveur Linux)

Même modèle que ComptaDoc / AgriActifs.

## Fichiers

| Fichier | Commité ? | Rôle |
|---------|-----------|------|
| `project.config.example.json` | Oui | Modèle app |
| `project.config.json` | Non | Config locale deploy-all |
| `deploy-gha.sh` | Oui | Déploiement GitHub Actions |
| `gha-env.sh` | Oui | Sanitisation secrets GHA |
| `.github/workflows/deploy-production.yml` | Oui | Pipeline CI/CD |

## GitHub Actions (recommandé)

Guide : [`servers/ubuntu1.md`](servers/ubuntu1.md)

Secrets dépôt :

- `UBUNTU1_APP_ROOT` = `/opt/apps/elevageactifs`
- `UBUNTU1_SERVICE_NAME` = `elevageactifs`
- `UBUNTU1_LISTEN_PORT` = `5052`
- `UBUNTU1_CONNECTION_STRING` = chaîne PostgreSQL

## Déploiement local Windows → Ubuntu

```powershell
copy deploy\project.config.example.json deploy\project.config.json
copy deploy\deploy-all.config.example.json deploy\deploy-all.config.json
.\deploy\deploy-all.ps1
```

## Cibles serveur

| Paramètre | Valeur |
|-----------|--------|
| Service systemd | `elevageactifs` |
| Répertoire | `/opt/apps/elevageactifs` |
| Port | `5052` |
| Schéma PostgreSQL | `elevageactifs` |
| DLL | `ElevageActifs.Web.dll` |
