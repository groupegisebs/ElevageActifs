# ElevageActifs

Gestion des **actifs d’une ferme d’élevage** (animaux, santé, reproduction, matériel, stocks) — écosystème GISEBS.

## Stack

ASP.NET Core 10 MVC · Identity RBAC · EF Core · PostgreSQL (schéma `elevageactifs`) · Bootstrap 5

## Démarrage

```powershell
cd ElevageActifs
dotnet ef database update --project src/ElevageActifs.Web
dotnet run --project src/ElevageActifs.Web --launch-profile https
```

URL : https://localhost:7122 (http://localhost:5052)

## Comptes démo

| Email | Mot de passe | Rôle |
|-------|--------------|------|
| `superadmin@elevageactifs.local` | `Elevage@Secure2026!` | SuperAdmin |
| `admin@elevageactifs.local` | `Elevage@Admin2026!` | Admin |
| `gerant@belleriviere.demo` | `Demo@Elevage2026!` | Gérant — Ranch Belle-Rivière |
| `zoo@belleriviere.demo` | `Demo@Elevage2026!` | Zootechnicien |
| `tech@belleriviere.demo` | `Demo@Elevage2026!` | Technicien |
| `ouvrier@belleriviere.demo` | `Demo@Elevage2026!` | Ouvrier |
| `lecture@belleriviere.demo` | `Demo@Elevage2026!` | Observateur |

Désactiver le seed démo : `"Seed": { "IncludeDemoData": false }` dans `appsettings.json`.

## Configuration locale (secrets)

Ne pas committer les mots de passe. Copier :

```powershell
copy src\ElevageActifs.Web\appsettings.Development.local.json.example src\ElevageActifs.Web\appsettings.Development.local.json
```

Puis renseigner la chaîne PostgreSQL. Le fichier `*.local.json` est ignoré par Git.

## Dépôt GitHub (2 repos séparés)

```powershell
cd ElevageActifs
git add -A
git commit -m "Initial commit: ElevageActifs MVP (ferme d elevage)"
git remote add origin https://github.com/VOTRE_ORG/ElevageActifs.git
git push -u origin main
```

## Déploiement serveur (GitHub Actions)

Voir [`deploy/servers/ubuntu1.md`](deploy/servers/ubuntu1.md).

```bash
sudo mkdir -p /opt/apps/elevageactifs && sudo chown ubuntu:ubuntu /opt/apps/elevageactifs
```

Secrets : `UBUNTU1_APP_ROOT=/opt/apps/elevageactifs`, `UBUNTU1_SERVICE_NAME=elevageactifs`, `UBUNTU1_LISTEN_PORT=5052`, `UBUNTU1_CONNECTION_STRING=...`

## Modules

Area `Elevage` : Dashboard, Exploitations, Troupeaux/Lots/Enclos, Animaux (export CSV), Santé, Reproduction, Actifs matériels, Stocks, Maintenance, Fournisseurs.

## Documentation

- [Cahier des charges](docs/CAHIER_DES_CHARGES.md)
- [Vision famille FermeActifs](../FERMEACTIFS_VISION.md)
