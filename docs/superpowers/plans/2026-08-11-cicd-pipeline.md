# CI/CD Pipeline Implementation Plan

Implement a 3-workflow GitHub Actions CI/CD pipeline using GameCI for Unity 2022.3 LTS Android build, automated testing, linting guardrails, and GitHub Releases artifact deployment.

## User Review Required

> [!IMPORTANT]
> The GitHub Actions workflows require setting up **Repository Secrets** on GitHub (`UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`, `IMMERSAL_DEVELOPER_TOKEN`, `ANDROID_KEYSTORE_BASE64`, `ANDROID_KEY_ALIAS`, `ANDROID_KEYSTORE_PASS`) for full remote cloud execution. The workflows will be committed locally to `.github/workflows/`.

- [ ] **Task 1: Create `.github/workflows/lint-and-check.yml`**
  - **Files**: `.github/workflows/lint-and-check.yml`
  - **Description**: Add guardrail workflow to scan for secret leakage, block `*.apk`/`*.aab` binary commits, and validate C# script syntax.
  - **Verification**: Verify workflow YAML syntax.

- [ ] **Task 2: Create `.github/workflows/unity-tests.yml`**
  - **Files**: `.github/workflows/unity-tests.yml`
  - **Description**: Add Unity Test Runner workflow using `game-ci/unity-test-runner@v4` with `actions/cache@v4` caching on Unity `Library/` directory.
  - **Verification**: Verify workflow YAML syntax.

- [ ] **Task 3: Create `.github/workflows/build-android-release.yml`**
  - **Files**: `.github/workflows/build-android-release.yml`
  - **Description**: Add Android build & release workflow using `game-ci/unity-builder@v4` with dynamic secret injection into `Assets/Resources/appconfig.json`, keystore signing, and GitHub Release drafting.
  - **Verification**: Verify workflow YAML syntax.

- [ ] **Task 4: Add Workflow Documentation & Verification**
  - **Files**: `docs/CICD.md`
  - **Description**: Create comprehensive operational guide for setting up GitHub Secrets, triggering builds, and downloading releases.
  - **Verification**: Verify markdown links and documentation structure.
