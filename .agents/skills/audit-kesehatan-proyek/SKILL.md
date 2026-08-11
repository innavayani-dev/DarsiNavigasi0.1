---
name: audit-kesehatan-proyek
description: Prosedur mengaudit kesehatan repositori DarsiNavigasi, verifikasi status lisensi & token Immersal, deteksi file build / APK yang tidak boleh di-commit, pemeriksaan App ID, dan audit kebersihan kode sebelum serah-terima. Gunakan saat diminta melakukan audit, pemeriksaan sebelum rilis/handover, atau sanitasi repositori.
---

# Audit Kesehatan Proyek DarsiNavigasi

Dokumen ini menyediakan panduan audit berkala untuk memastikan repositori DarsiNavigasi berada dalam kondisi sehat, aman, dan siap untuk serah-terima (handover) ke RS Islam Ahmad Yani Surabaya.

## Item Audit Utama

### 1. Keamanan Kredensial & Token Immersal (KI-01)
- **Cek Token Plaintext di Scene**: Pastikan `developerToken` tidak terekspos secara bebas.
- **Peta Cloud vs Lokal**: Periksa 114 peta Immersal. Pastikan peta lokal di `Assets/Map Data/` mencakup semua peta yang dibutuhkan agar aplikasi tidak bergantung total pada server cloud.

```bash
# Periksa token di scene
grep -i "developerToken" "Assets/Scenes/6_AR Navigasi.unity"
```

### 2. File Terlarang & Kebersihan Git (KI-11)
- **Periksa File Build / APK**: File APK/AAB dilarang masuk commit baru.
- **Periksa Folder Temporary Unity**: Pastikan `Library/`, `Temp/`, `Logs/`, `UserSettings/` masuk dalam `.gitignore`.

```bash
# Cek ukuran file APK atau file besar di repo
ls -lh *.apk
git status --ignored
```

### 3. Konfigurasi App ID & Identitas Rilis
- **Application Identifier**: Cek `ProjectSettings/ProjectSettings.asset` — Ubah dari `com.DefaultCompany.DarsiNav` ke ID resmi rumah sakit / institusi sebelum build rilis.
- **Bundle Version**: Pastikan versi aplikasi tercatat dengan benar.

### 4. Sinkronisasi Nama Ruangan & Filter Gedung (KI-02)
- Bandingkan daftar nama GameObject di scene dengan `daftarGraha` & `daftarTower` di `NavigationManager.cs` dan tabel QR di `ScannerCameraController.cs`.
- Gunakan skill `tambah-ruangan-navigasi` jika ditemukan ketidakcocokan nama.

### 5. Verifikasi Kode Mati & Duplikasi Scene (KI-09, KI-10, KI-12)
- **Scene Cadangan**: Scene `7_AR Navigasi.unity` (56 MB) tidak masuk Build Settings. Pastikan tidak sengaja diubah atau membingungkan pengembang lain.
- **A* Pathfinding**: Pahami bahwa `NavigationGraphManager.cs` (A*) adalah kode mati, Unity NavMesh yang bekerja.
