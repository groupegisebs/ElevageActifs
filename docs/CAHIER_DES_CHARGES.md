# Cahier des charges — ElevageActifs

> Projet technique : `ElevageActifs.Web`  
> Famille produit : **FermeActifs** (écosystème GISEBS)  
> Type : **Identity-Based Application** · **RBAC** · multi-exploitation  
> Socle : fork de [`GISERBAC-TEMPLATE`](../../GISERBAC-TEMPLATE/docs/CAHIER_DES_CHARGES.md)  
> Références métier : isolation type ComptaDoc (`Company` → `Exploitation`), inventaire / équipements  
> Produit sœur : [`AgriActifs`](../../AgriActifs/docs/CAHIER_DES_CHARGES.md)

---

## 1. Objectif

Fournir une application **ASP.NET Core MVC** complète pour la **gestion des actifs d’une ferme d’élevage** (animaux, troupeaux, bâtiments d’élevage, matériel, stocks alimentaires / vétérinaires, santé, reproduction), avec :

- authentification Identity + RBAC GISEBS ;
- isolation des données par **exploitation** (ferme) ;
- registre animalier et registre matériel ;
- traçabilité sanitaire et mouvements (entrées / sorties / transferts) ;
- tableaux de bord troupeau / alertes ;
- comptes de démonstration prêts à l’emploi ;
- intégration optionnelle SecureMail / Pay Gateway / Support Hub.

```
Utilisateur → Rôles → Permissions → Ressources (scopées ExploitationId)
```

---

## 2. Périmètre

### 2.1 Inclus (MVP + V1)

| Domaine | Description |
|---------|-------------|
| Exploitation | Multi-fermes, espèces cibles, membres |
| Troupeaux & lots | Structure cheptel, lots, enclos |
| Animaux (actifs vivants) | Identification individuelle / lot |
| Santé & traitements | Protocoles, interventions vétérinaires, retraitements |
| Reproduction | Chaleurs, saillies / IA, gestations, naissances |
| Actifs matériels | Matériel d’élevage, bâtiments, clôtures |
| Stocks | Aliments, litière, médicaments, pièces |
| Maintenance | Sur matériel et bâtiments |
| Dashboard & rapports | Effectifs, alertes sanitaires, exports |
| Admin RBAC | Utilisateurs, rôles, audit, paramètres |

### 2.2 Hors périmètre (évolutions)

- Comptabilité complète (ComptaDoc)
- Abattoir / traçabilité abattoir nationale (ATQ / etc.) full EDI
- Capteurs IoT / balances connectées temps réel
- Application mobile native offline-first (V2)

---

## 3. Stack technique (alignée écosystème)

| Composant | Technologie |
|-----------|-------------|
| Framework | ASP.NET Core **10** MVC |
| Authentification | ASP.NET Core Identity (cookies) |
| Autorisation | Rôles + policies dynamiques `Permission:{code}` |
| ORM | Entity Framework Core |
| Base de données | **PostgreSQL** (Npgsql), schéma `elevageactifs` |
| UI Auth | Identity Razor Pages (`Areas/Identity`) |
| UI métier | Bootstrap **5.3** + Bootstrap Icons + thème `--gise-*` |
| Localisation | FR (défaut) / EN |
| Jobs (option V1.1) | Hangfire (rappels traitements, sevrage, vaccins) |
| Tests | Projet `ElevageActifs.Tests` (xUnit) |
| Déploiement | `deploy/` (systemd / GHA), `project.config.json` |

**AppCode** intégrations : `ELEVAGEACTIFS`

---

## 4. Acteurs et rôles

### 4.1 Rôles système (Identity — template GISEBS)

| Rôle | Usage |
|------|--------|
| SuperAdmin | Plateforme entière |
| Admin | Administration app |
| Manager | Pilotage multi-modules |
| User | Opérateur métier |
| Auditor | Lecture + audit |
| ReportViewer | Rapports / exports |

### 4.2 Rôles métier par exploitation

| Rôle exploitation | Responsabilités |
|-------------------|-----------------|
| Proprietaire | Tous droits |
| Gerant | Pilotage troupeau + validation |
| Zootechnicien | Animaux, reproduction, performances |
| VeterinaireExterne | Santé (lecture + saisie traitements) — compte limité |
| Technicien | Matériel, stocks, maintenance |
| Ouvrier | Soins quotidiens, mouvements, saisie limitée |
| Observateur | Lecture seule |

Isolation : `ExploitationId` sur toutes les entités métier.

---

## 5. Modules fonctionnels

### 5.0 Socle RBAC (hérité du template)

Identique à AgriActifs / GISERBAC-TEMPLATE : Identity, Admin, audit, settings, permissions dynamiques.

### 5.1 Exploitations

- CRUD exploitation (nom, adresse, espèces principales, capacité, site)
- Membres + rôle métier
- Contexte courant `ExploitationId`
- Paramètres : unités (kg, lb), fuseau, règles d’identification (boucle, RFID, tatouage)

**Area** : `Elevage`  
**Permissions** : `Exploitations.View`, `Exploitations.Manage`, `Exploitations.Members`

### 5.2 Troupeaux, lots et enclos

- Troupeau / bande (ex. « Vaches laitières 2026 », « Porcelets lot A »)
- Lot : regroupement opérationnel
- Enclos / bâtiment / loge : capacité, type, statut
- Transferts lot ↔ enclos

**Permissions** : `Troupeaux.View`, `Troupeaux.Manage`

### 5.3 Animaux — actifs vivants (cœur métier)

Modes de gestion :

| Mode | Usage |
|------|--------|
| Individuel | Bovins, équins, caprins de valeur — fiche par animal |
| Lot | Volailles, porcs engraissement — effectif + événements de lot |

Fiche animal (mode individuel) :

- Identifiant : boucle / RFID / numéro interne
- Espèce, race, sexe, date de naissance, origine
- Statut : `Present`, `Vendu`, `Mort`, `Transfere`, `Reforme`
- Poids / performances (saisies ponctuelles)
- Généalogie (mère / père) optionnelle
- Documents (certificats, photos)

Événements :

- Entrée (naissance, achat, transfert entrant)
- Sortie (vente, mortalité, transfert sortant, réforme)
- Pesée, observation

**Permissions** : `Animaux.View`, `Animaux.Create`, `Animaux.Edit`, `Animaux.Delete`, `Animaux.Move`, `Animaux.Export`

### 5.4 Santé & traitements

- Catalogue protocoles (vaccination, vermifuge, antibiotique…)
- Ordonnance / traitement : animal ou lot, produit, dose, délai d’attente (lait / viande)
- Calendrier sanitaire + alertes échéances
- Journal des symptômes / observations
- Stock médicaments lié (sortie stock à la saisie)

**Permissions** : `Sante.View`, `Sante.Manage`, `Sante.Administer`

### 5.5 Reproduction

- Détection chaleurs
- Saillie / IA (taureau / semence, date)
- Diagnostic gestation
- Mise bas / vêlage : nouveau-né(s) → création fiches animales
- Réforme reproduction

**Permissions** : `Reproduction.View`, `Reproduction.Manage`

### 5.6 Actifs matériels d’élevage

Catégories :

| Catégorie | Exemples |
|-----------|----------|
| BatimentElevage | Étable, porcherie, poulailler, nurserie |
| MaterielTraite | Robots / salles de traite |
| Alimentation | Mélangeuses, silos, auges |
| Contenue | Clôtures, portes, cages |
| MaterielRoulant | Chargeurs, quad, remorques bétail |
| EquipementSante | Contention, balance, matériel véto |
| Autre | Divers |

Même cycle de vie que AgriActifs : acquisition, statut, maintenance, documents, valeur.

**Permissions** : `Actifs.View`, `Actifs.Create`, `Actifs.Edit`, `Actifs.Delete`, `Actifs.Export`

### 5.7 Stocks d’élevage

- Aliments (concentrés, fourrages), litière, médicaments, pièces
- Mouvements + lots + dates d’expiration (médicaments)
- Alertes seuil + péremption
- Consommation liée aux traitements / rations (saisie simple)

**Permissions** : `Stocks.View`, `Stocks.Manage`, `Stocks.Adjust`

### 5.8 Maintenance

Identique conceptuellement à AgriActifs, ciblée matériel / bâtiments d’élevage.

**Permissions** : `Maintenance.View`, `Maintenance.Manage`, `Maintenance.Close`

### 5.9 Fournisseurs

- Vétérinaires, meuniers, marchands d’animaux, équipementiers
- Achats animaux / matériel (pièces jointes)

**Permissions** : `Fournisseurs.View`, `Fournisseurs.Manage`

### 5.10 Dashboard & alertes

KPI :

- Effectif présent par espèce / statut
- Naissances / mortalités période
- Traitements en attente / délais d’attente actifs
- Gestations en cours / mises bas prévues
- Stocks bas / péremptions
- Interventions maintenance ouvertes
- Valeur parc matériel

### 5.11 Rapports

| Rapport | Description |
|---------|-------------|
| Inventaire animalier | Effectifs + détail individuel / lot |
| Mouvements | Entrées / sorties période |
| Sanitaire | Traitements + délais d’attente |
| Reproduction | Taux gestation, naissances |
| Inventaire matériel | Actifs non vivants |
| Stocks | Niveaux + alertes |

**Permissions** : `Reports.View`, `Reports.Export`

### 5.12 Intégrations écosystème GISEBS

| Service | Usage | Identifiant |
|---------|-------|-------------|
| SecureMailGateway | Alertes sanitaires, invitations, rappels | `ELEVAGEACTIFS` |
| GiseBsPayGateway | Abonnement SaaS (option) | `X-App-Code: ELEVAGEACTIFS` |
| GiseSupportHub | Support embarqué | API Key client |

---

## 6. Modèle de données (simplifié)

```
ApplicationUser / ApplicationRole / UserProfile
AuditLog / SystemSettings / ReportDefinition

Exploitation
  ├── ExploitationUser
  ├── Troupeau
  │     └── Lot
  ├── Enclos
  ├── Animal
  │     ├── AnimalEvenement (entrée/sortie/pesée/…)
  │     ├── Traitement
  │     └── EvenementReproduction
  ├── ProtocoleSanitaire
  ├── ActifMateriel
  │     ├── ActifDocument
  │     └── InterventionMaintenance
  ├── StockArticle
  │     └── StockMouvement
  └── Fournisseur
```

Règles :

- Un `Traitement` référence soit `AnimalId`, soit `LotId`
- Délai d’attente : dates `WaitMilkUntil` / `WaitMeatUntil`
- Soft-delete / `IsActive` + audit des sorties animales (pas de suppression physique des historiques)

---

## 7. Matrice permissions métier (extrait)

| Permission | SuperAdmin | Admin | Proprietaire* | Gerant* | Zootechnicien* | Technicien* | Ouvrier* | Observateur* |
|------------|:----------:|:-----:|:-------------:|:-------:|:--------------:|:-----------:|:--------:|:------------:|
| Animaux.View | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Animaux.Create / Edit | ✅ | ✅ | ✅ | ✅ | ✅ | | ✅† | |
| Animaux.Move | ✅ | ✅ | ✅ | ✅ | ✅ | | ✅ | |
| Sante.Manage | ✅ | ✅ | ✅ | ✅ | ✅ | | | |
| Sante.Administer | ✅ | ✅ | ✅ | ✅ | ✅ | | ✅ | |
| Reproduction.Manage | ✅ | ✅ | ✅ | ✅ | ✅ | | | |
| Actifs.Edit | ✅ | ✅ | ✅ | ✅ | | ✅ | | |
| Stocks.Manage | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | | |
| Reports.Export | ✅ | ✅ | ✅ | ✅ | ✅ | | | ✅ |

\*Scopé exploitation. †Ouvrier : saisie événements / observations limitée selon grants.

---

## 8. Comptes de démonstration (seed)

### 8.1 Comptes plateforme

| Email | Mot de passe | Rôle |
|-------|--------------|------|
| `superadmin@elevageactifs.local` | `Elevage@Secure2026!` | SuperAdmin |
| `admin@elevageactifs.local` | `Elevage@Admin2026!` | Admin |

### 8.2 Exploitation démo — « Ranch Belle-Rivière »

Espèces : bovins laitiers + atelier complémentaire.

| Email | Mot de passe | Rôle exploitation |
|-------|--------------|-------------------|
| `gerant@belleriviere.demo` | `Demo@Elevage2026!` | Gerant |
| `zoo@belleriviere.demo` | `Demo@Elevage2026!` | Zootechnicien |
| `tech@belleriviere.demo` | `Demo@Elevage2026!` | Technicien |
| `ouvrier@belleriviere.demo` | `Demo@Elevage2026!` | Ouvrier |
| `lecture@belleriviere.demo` | `Demo@Elevage2026!` | Observateur |

### 8.3 Données démo minimales

- 1 exploitation bovine
- 3 troupeaux / lots + 4 enclos
- 25 animaux individuels (dont 3 gestantes, 2 délais d’attente actifs)
- 6 événements récents (naissance, vente, mortalité, pesées)
- 5 traitements (2 à échéance proche)
- 6 actifs matériels (étable, salle de traite, mélangeuse, clôtures…)
- 10 articles de stock (aliments + médicaments)
- 3 fournisseurs (mégo, vétérinaire, équipementier)

Flag : `Seed:IncludeDemoData=true`.

---

## 9. Structure projet

```
ElevageActifs/
├── ElevageActifs.slnx
├── README.md
├── docs/
│   ├── CAHIER_DES_CHARGES.md      ← ce document
│   ├── ARCHITECTURE_RBAC.md
│   └── SECUREMAIL_INTEGRATION.md
├── src/ElevageActifs.Web/
│   ├── Areas/
│   │   ├── Admin/
│   │   ├── Identity/
│   │   └── Elevage/               # modules métier
│   ├── Authorization/
│   ├── Constants/
│   ├── Controllers/
│   ├── Data/
│   ├── Extensions/
│   ├── Models/
│   ├── Services/
│   └── Views/
├── tests/ElevageActifs.Tests/
└── deploy/
```

---

## 10. Exigences non fonctionnelles

| Critère | Cible |
|---------|-------|
| Sécurité | CSRF, HTTPS, cookies Secure, MDP 12+, lockout, audit |
| Confidentialité sanitaire | Accès traitements selon permissions ; audit des exports |
| Performance | Listes paginées ; filtres espèce / statut indexés |
| Traçabilité | Historique immuable des mouvements animaux |
| Secrets | Hors code en prod |
| Qualité | Tests unitaires scoping + règles délai d’attente |

---

## 11. Critères d’acceptation (MVP)

- [ ] Projet compilable .NET 10, schéma `elevageactifs`
- [ ] Seed rôles + SuperAdmin + comptes démo
- [ ] Contexte exploitation + isolation
- [ ] CRUD troupeaux / animaux / mouvements
- [ ] Santé (traitements + alertes délai d’attente)
- [ ] Actifs matériels + stocks + maintenance
- [ ] Dashboard effectifs + alertes
- [ ] Area Admin RBAC
- [ ] Export inventaire animalier Excel
- [ ] UI Bootstrap 5 FR, responsive
- [ ] Tests scoping ExploitationId + délais d’attente

---

## 12. Phasage

| Phase | Contenu | Durée indicative |
|-------|---------|------------------|
| **P0** | Fork template, Identity, Exploitation | 1 itération |
| **P1** | Troupeaux + Animaux + Mouvements + Dashboard | 1–2 itérations |
| **P2** | Santé + Reproduction | 1–2 itérations |
| **P3** | Actifs matériels + Stocks + Maintenance | 1 itération |
| **P4** | Rapports, SecureMail, tests, polish | 1 itération |
| **P5** | Pay Gateway / Support Hub / Hangfire | optionnel |

---

## 13. Commandes utiles (cible)

```powershell
dotnet ef migrations add InitialCreate --project src/ElevageActifs.Web
dotnet ef database update --project src/ElevageActifs.Web
dotnet run --project src/ElevageActifs.Web
```

---

## 14. Évolutions prévues

1. Interopérabilité ATQ / registres nationaux (connecteurs)
2. Module lait (tank, qualité, collectes)
3. Lien ComptaDoc (ventes animaux / immobilisations)
4. API REST partenaires / vétérinaires
5. Application mobile soins quotidiens
6. Pack multi-espèces avancé (volaille, porc, ovins) avec workflows dédiés
