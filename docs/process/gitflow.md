# GitFlow

We use a simplified **GitFlow branching model**.

## Branches
- `main` → always production-ready. Tagged with semantic versioning.  
- `develop` → integration branch for features.  
- `feature/*` → new features, branched from `develop`.  
- `release/*` → preparation for a release, bugfixes only.  
- `hotfix/*` → critical fixes for production, branched from `main`.

## Merge Policy
- Features → merged into `develop` via PR.  
- Releases → merged into `main` and `develop`.  
- Hotfixes → merged into `main` and cherry-picked into `develop`.  

## Versioning
- Semantic Versioning (SemVer): MAJOR.MINOR.PATCH.  
- Tags are created when merging to `main`.
