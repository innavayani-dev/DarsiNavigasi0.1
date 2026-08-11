# CI/CD Pipeline Design — DarsiNavigasi (Unity 2022.3 LTS Android AR)

**Tanggal:** 2026-08-11  
**Status:** DRAFT — Menunggu Review Pengguna  
**Target Platform:** Unity `2022.3.62f2` (LTS) · Android AR (AR Foundation / Immersal VPS)  

---

## 1. Ringkasan & Tujuan Arsitektur

Pipeline CI/CD ini dirancang untuk mengubah repositori **DarsiNavigasi** menjadi repositori kelas produksi dan standar penelitian yang aman, otomatis, dan dapat direproduksi (*reproducible*).

### Masalah Utama yang Diselesaikan
1. **Pencegahan Token Plaintext Bocor ([KI-01](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/docs/KNOWN-ISSUES.md#--ki-01--developer-token-immersal-ter-commit-dan-52-peta-hanya-ada-di-cloud))**:  
   Developer token Immersal tidak pernah disimpan di repositori. Token disimpan di `GitHub Repository Secrets` dan disuntikkan secara dinamis ke `Assets/Resources/appconfig.json` pada runtime build CI/CD.
2. **Distribusi APK Resmi Tanpa Membebani Git ([KI-11](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/docs/KNOWN-ISSUES.md#--ki-11--repo-tidak-siap-didistribusikan))**:  
   File APK/AAB hasil build diunggah otomatis ke **GitHub Releases**, bukan di-commit ke Git.
3. **Optimasi Waktu Kompilasi**:  
   Menggunakan `actions/cache@v4` pada folder `Library/` Unity untuk mempercepat build dari 25-30 menit menjadi 5-8 menit pada eksekusi berulang.

---

## 2. Struktur Workflow `.github/workflows/`

Pipeline terdiri dari 3 alur kerja (*workflows*) terpisah:

```
.github/workflows/
├── lint-and-check.yml       ← Fast Guardrail: C# Syntax, Large Files, Secrets Check
├── unity-tests.yml          ← Test Runner: Unity EditMode & PlayMode Unit Tests
└── build-android-release.yml← Build & Release: Unity Android Build & GitHub Release
```

---

## 3. Spesifikasi Masing-Masing Workflow

### A. `lint-and-check.yml`
* **Pemicu (Triggers)**: `push` atau `pull_request` ke cabang `master` / `main`.
* **Langkah-langkah**:
  1. `actions/checkout@v4`
  2. Scan file terlarang (mencegah commit file `*.apk`, `*.aab`, atau secret key).
  3. Validasi struktur JSON pada `Assets/Resources/appconfig.example.json`.
  4. Pemeriksaan kompilasi C# script.

### B. `unity-tests.yml`
* **Pemicu (Triggers)**: `pull_request` ke cabang `master`.
* **Langkah-langkah**:
  1. Restore cache `Library/` Unity via `actions/cache@v4`.
  2. Jalankan Unity Test Runner via `game-ci/unity-test-runner@v4` (EditMode & PlayMode).
  3. Publikasi laporan hasil test (*Test Summary Artifact*).

### C. `build-android-release.yml`
* **Pemicu (Triggers)**: Git Tag baru `v*.*.*` atau pemicu manual `workflow_dispatch`.
* **Langkah-langkah**:
  1. Inject Token Immersal dari secret `${{ secrets.IMMERSAL_DEVELOPER_TOKEN }}` ke `Assets/Resources/appconfig.json`.
  2. Restore cache `Library/` Unity (`actions/cache@v4`).
  3. Eksekusi kompilasi Unity Android APK/AAB via `game-ci/unity-builder@v4` (Unity `2022.3.62f2`).
  4. Penandatanganan APK (*Sign APK*) menggunakan `ANDROID_KEYSTORE_BASE64`.
  5. Pembuatan GitHub Release dan upload artefak APK (`DarsiNavigasi-vX.Y.Z.apk`).

---

## 4. Konfigurasi GitHub Repository Secrets

Agar pipeline dapat berjalan, kredensial berikut wajib didaftarkan di **Settings → Secrets and variables → Actions**:

| Name | Description |
|---|---|
| `UNITY_LICENSE` | Isi Lisensi Unity untuk GameCI Builder |
| `UNITY_EMAIL` | Email akun Unity |
| `UNITY_PASSWORD` | Password akun Unity |
| `IMMERSAL_DEVELOPER_TOKEN` | Token Developer Immersal VPS |
| `ANDROID_KEYSTORE_BASE64` | File Keystore Android di-encode Base64 |
| `ANDROID_KEY_ALIAS` | Alias Keystore Android |
| `ANDROID_KEYSTORE_PASS` | Password Keystore Android |

---

## 5. Self-Review Checklist

- [x] **Placeholder scan**: Tidak ada TBD/TODO yang ambigu.
- [x] **Konsistensi Internal**: Sesuai dengan aturan `AGENTS.md`, `CLAUDE.md`, dan penyelesaian `KI-01` & `KI-11`.
- [x] **Lingkup Kerja**: Terfokus pada 3 file workflow CI/CD.
