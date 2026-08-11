# Panduan Operasional CI/CD — DarsiNavigasi

Dokumen ini menjelaskan cara menggunakan, mengonfigurasi, dan memicu workflow CI/CD otomatis untuk proyek **DarsiNavigasi** menggunakan GitHub Actions & GameCI.

---

## 1. Daftar Workflow

| Workflow | File | Pemicu (*Trigger*) | Tugas |
|---|---|---|---|
| **Lint & Guardrails** | `.github/workflows/lint-and-check.yml` | `push` / `pull_request` | Memeriksa format JSON, kompilasi C#, dan memblokir commit token plaintext atau file APK 400MB. |
| **Unity Unit Tests** | `.github/workflows/unity-tests.yml` | `pull_request` ke `master` | Menjalankan Unity Test Runner (EditMode & PlayMode) via GameCI. |
| **Android Build & Release** | `.github/workflows/build-android-release.yml` | Tag `v*.*.*` / Manual | Menyuntikkan token Immersal dari GitHub Secrets, membuild APK Android Unity `2022.3.62f2`, dan mengunggahnya ke GitHub Releases. |

---

## 2. Cara Mengonfigurasi GitHub Repository Secrets

Agar build otomatis di cloud GitHub Actions dapat berjalan, daftarkan secrets berikut pada halaman repositori GitHub Anda: **Settings → Secrets and variables → Actions → New repository secret**:

| Nama Secret | Contoh / Format | Deskripsi |
|---|---|---|
| `IMMERSAL_DEVELOPER_TOKEN` | `ea46d0e834b0...` | Token Developer Immersal VPS. Disuntikkan otomatis ke `appconfig.json` saat build. |
| `UNITY_LICENSE` | `<xml>...` | Isi file lisensi Unity (`.ulf` / `.lic`). |
| `UNITY_EMAIL` | `email@domain.com` | Email akun Unity. |
| `UNITY_PASSWORD` | `PasswordAnda` | Password akun Unity. |
| `ANDROID_KEY_ALIAS` | `darsinav_key` | Alias Keystore Android. |
| `ANDROID_KEYSTORE_PASS` | `PasswordKeystore` | Password Keystore & Key. |

---

## 3. Cara Memicu Release APK Baru

Untuk menghasilkan APK rilis resmi untuk pengujian di RS Islam Ahmad Yani Surabaya:

1. Pastikan seluruh perubahan kode sudah ter-commit di cabang `master`.
2. Buat git tag baru berbasis versi:
   ```bash
   git tag -a v1.0.0 -m "Release v1.0.0 untuk Uji Lapangan RS Islam A. Yani"
   git push origin v1.0.0
   ```
3. GitHub Actions akan secara otomatis menjalankan workflow **Build Android Release**, membuild APK, dan membuat halaman **GitHub Release** berlabel `v1.0.0` lengkap dengan file `.apk` yang siap diunduh!

---

## 4. Keuntungan Arsitektur Ini

* **Tidak Ada APK di Git History**: Memenuhi aturan [`KI-11`](KNOWN-ISSUES.md#--ki-11--repo-tidak-siap-didistribusikan).
* **Token Tersimpan Aman**: Memenuhi aturan [`KI-01`](KNOWN-ISSUES.md#--ki-01--developer-token-immersal-ter-commit-dan-52-peta-hanya-ada-di-cloud).
* **Waktu Build Cepat**: Menggunakan caching `Library/` Unity (`actions/cache@v4`) sehingga build kedua dan seterusnya hanya memakan waktu ~5–8 menit.
