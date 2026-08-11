# AGENTS.md — DarsiNavigasi (Unity Repo)

Aturan dan panduan ini **WAJIB dipatuhi oleh semua AI Agent (Antigravity/AGY, Gemini, dll.)** yang bekerja di repositori ini.

Baca [`docs/ARCHITECTURE.md`](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/docs/ARCHITECTURE.md), [`docs/FLOWS.md`](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/docs/FLOWS.md), [`docs/DECISIONS.md`](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/docs/DECISIONS.md), dan [`docs/KNOWN-ISSUES.md`](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/docs/KNOWN-ISSUES.md) lebih dulu sebelum melakukan pekerjaan apa pun. **Jangan pernah berasumsi dari training data** — repo ini diserahkan tanpa dokumentasi awal dan seluruh dokumen yang ada adalah hasil rekonstruksi dari kode pada 2026-08-08.

---

## Ringkasan Super Singkat

Aplikasi Android AR untuk memandu pengunjung **RS Islam Ahmad Yani Surabaya**.
- Lokalisasi menggunakan **Immersal VPS** (114 peta point cloud, gedung Graha + Tower LT1–13).
- Navigasi rute menggunakan **Unity NavMesh + NavMeshLink**.
- Versi Unity: **2022.3.62f2 (LTS)**.

Projek ini sedang difinalisasi untuk:
1. **Diserahkan ke klien (RS Islam Ahmad Yani)**.
2. **Menjadi struktur awal penelitian (publikasi ilmiah)** — sehingga standar akurasi, verifikasi, dan kejujurannya jauh lebih tinggi daripada prototipe biasa.

---

## Aturan WAJIB (Aturan Utama)

- **Otak navigasi ada di [`Assets/Samples/Immersal SDK/2.2.1/Core Samples/Scripts/Navigation/NavigationManager.cs`](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/Assets/Samples/Immersal%20SDK/2.2.1/Core%20Samples/Scripts/Navigation/NavigationManager.cs)**, bukan di `Assets/Scripts/`. File ini telah dimodifikasi berat dari versi asli SDK. **Jangan pernah menyarankan re-import Immersal Core Samples** — karena akan menimpanya tanpa peringatan.
- **Nama GameObject = ID Ruangan.** Field `targetName` kosong pada semua 63 target ruangan, sehingga jatuh ke `gameObject.name`. Rename objek di Hierarchy = mengubah ID ruangan tanpa peringatan ([ADR-R003](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/docs/DECISIONS.md#adr-r003-nama-gameobject-adalah-id-ruangan)).
- **A\* adalah kode mati.** [`NavigationGraphManager.cs`](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/Assets/Samples/Immersal%20SDK/2.2.1/Core%20Samples/Scripts/Navigation/NavigationGraphManager.cs) berisi 238 baris kode A* tetapi terdapat 0 `Waypoint` di scene. **Unity NavMesh** yang sebenarnya bekerja ([KI-09](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/docs/KNOWN-ISSUES.md#--ki-09--a-dan-tool-waypoint-adalah-kode-mati)). Jangan pernah menjelaskan sistem ini seolah pathfinding-nya A*.
- **`DoorbellARPopup.cs` terlihat mati tetapi HIDUP.** Membuat GameObject-nya sendiri via singleton getter dan dipanggil dari 6 tempat. Jangan usulkan untuk menghapusnya.
- **Ada tiga ruang koordinat**: World, XRSpace local, dan NavMesh. Transform `XRSpace` berubah setiap kali VPS localize. Kode yang lupa konversi akan menghasilkan angka yang salah ([KI-04](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/docs/KNOWN-ISSUES.md#--ki-04--arpathdistance-memakai-ruang-koordinat-yang-salah)).
- **Duplikasi nama ruangan sengaja DITUNDA** ([ADR-N002](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/docs/DECISIONS.md#adr-n002-perbaikan-duplikasi-nama-ruangan-ditunda)). Jika perlu menyentuh atau menambah ruangan, wajib mengikuti panduan di [`.agents/skills/tambah-ruangan-navigasi/`](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/.agents/skills/tambah-ruangan-navigasi/SKILL.md).
- **Belum ada instrumentasi sama sekali** ([KI-05](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/docs/KNOWN-ISSUES.md#--ki-05--tidak-ada-instrumentasi-sama-sekali)). Jangan pernah menyatakan angka performa tanpa adanya pengukuran empiris nyata.

---

## Sebelum Menyentuh Scene AR

File scene [`Assets/Scenes/6_AR Navigasi.unity`](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/Assets/Scenes/6_AR%20Navigasi.unity) berukuran **60 MB** dengan 500+ GameObject dan 114 peta.
- Perubahan kecil pun akan menghasilkan diff raksasa di version control.
- Tidak ada automated test yang menangkap jika sesuatu di scene rusak.
- **Selalu tanya dan minta persetujuan terlebih dahulu** sebelum mengedit file scene ini, serta sebutkan risikonya secara eksplisit.

Scene [`7_AR Navigasi.unity`](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/Assets/Scenes/7_AR%20Navigasi.unity) **TIDAK ikut build** (duplikat eksperimen). Jangan diedit tanpa alasan kuat.

---

## Standar Kejujuran & Integritas Akademik

Hasil dari projek ini akan digunakan pada publikasi ilmiah dengan pemilik projek sebagai salah satu penulis:
1. **Jangan menulis alasan desain seolah diketahui pasti.** Rekonstruksi pada [ADR Rekonstruksi (Bagian A `DECISIONS.md`)](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/docs/DECISIONS.md#bagian-a--adr-rekonstruksi) adalah kesimpulan berbasis bukti kode, bukan dari catatan penulis asli.
2. **Setiap klaim faktual wajib menyebutkan file dan barisnya.**
3. **Tandai `[belum terverifikasi]`** untuk hal-hal yang masih memerlukan pengujian langsung pada perangkat Android. Jangan menaikkan dugaan menjadi fakta tanpa data.
4. **Jika tidak tahu, katakan tidak tahu.**

---

## Solusi Teknis & Alur Kerja

- **Utamakan Best Practice**: Berikan usulan ideal terlebih dahulu sebelum menawarkan kompromi.
- **Bedakan "Meredakan Gejala" vs "Menyembuhkan Penyebab"**: Nyatakan sejujurnya jika suatu perbaikan hanya mengurangi frekuensi masalah.
- **Hindari Duplikasi Data Manual**: Setiap data hanya boleh memiliki satu *Single Source of Truth*.
- **Verifikasi Sebelum Mengusulkan**: Periksa kode dan scene sebelum mendiagnosis.
- **Konsultasi Sebelum Commit**: Jangan pernah melakukan commit tanpa persetujuan pemilik projek (Bagus).
- **Update Known Issues**: Setiap temuan bug terverifikasi baru WAJIB ditambahkan ke [`docs/KNOWN-ISSUES.md`](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/docs/KNOWN-ISSUES.md) lengkap dengan lokasi `file:baris`.

---

## File / Data yang DILARANG di-Commit

- File build / APK / AAB (seperti [`DarsiNavigasi0.1.1.apk`](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/DarsiNavigasi0.1.1.apk)).
- Developer Token, Kredensial Backend, atau Rahasia Kunci API.
- Folder temporary Unity: `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `MemoryCaptures/`.

---

## Aturan Git, Push & Identitas AI (DILARANG MASUK COLLABORATOR)

- **DILARANG PUSH REPO**: AI Agent **DILARANG KERAS** melakukan `git push` ke remote repository (GitHub/GitLab/dll). Push hanya boleh dilakukan secara manual oleh pemilik projek (Bagus).
- **DILARANG MENCANTUMKAN AI SEBAGAI COLLABORATOR / AUTHOR**: Jangan pernah mencantumkan identitas AI (misalnya `Antigravity`, `Gemini`, `Claude`, `Co-authored-by: ...`, dll.) sebagai author, committer, atau collaborator pada git commit, pull request, atau riwayat repositori.
- **COMMIT MEMBUTUHKAN CONSENT EKSPLISIT**: AI hanya boleh menjalankan/menyarankan `git commit` setelah mendapatkan persetujuan (*consent*) eksplisit dari pemilik projek, dan commit harus menggunakan identitas git pemilik projek.

