# GitFlow

We use a simplified **GitFlow branching model**.

## Branches

- `release` → always production-ready. Tagged with semantic versioning.
- `main` → integration branch for features.
- `feature/*` → new features, branched from `main`.
- `hotfix/*` → critical fixes for production, branched from `release`.

## Merge Policy

- Features → merged into `main` via PR.
- Releases → merged into `release` from `main`.
- Hotfixes → merged into `release` and cherry-picked into `main`.

## Versioning

- Semantic Versioning (SemVer): MAJOR.MINOR.PATCH.
- Tags are created when merging to `release`.
