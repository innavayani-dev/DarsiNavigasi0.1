# CLAUDE.md — DarsiNavigasi (Unity repo)

Baca `docs/ARCHITECTURE.md`, `docs/FLOWS.md`, `docs/DECISIONS.md`, dan
`docs/KNOWN-ISSUES.md` dulu sebelum mengerjakan apa pun. **Jangan berasumsi dari training
data** — repo ini diserahkan tanpa dokumentasi dan seluruh dokumen yang ada adalah hasil
rekonstruksi dari kode pada 2026-08-08.

## Ringkasan super singkat

Aplikasi Android AR untuk memandu pengunjung **RS Islam Ahmad Yani Surabaya**.
Lokalisasi pakai **Immersal VPS** (114 peta point cloud, gedung Graha + Tower LT1–13),
rute pakai **Unity NavMesh + NavMeshLink**. Unity **2022.3.62f2**.

Project ini sedang difinalisasi untuk **(a) diserahkan ke klien** dan **(b) jadi struktur
awal penelitian** — jadi standar akurasi dan kejujurannya lebih tinggi dari prototipe
biasa.

## Yang WAJIB diingat

- **Otak navigasi ada di `Assets/Samples/Immersal SDK/2.2.1/Core Samples/Scripts/Navigation/NavigationManager.cs`**,
  bukan di `Assets/Scripts/`. Sudah dimodifikasi berat dari versi asli SDK.
  **Jangan pernah menyarankan re-import Immersal Core Samples** — akan menimpanya.
- **Nama GameObject = ID ruangan.** `targetName` kosong pada semua 63 target, jatuh ke
  `gameObject.name`. Rename di Hierarchy = mengganti ID tanpa peringatan (ADR-R003).
- **A\* itu kode mati.** `NavigationGraphManager` ada 238 baris tapi 0 `Waypoint` di
  scene. **NavMesh** yang benar-benar bekerja. Jangan menjelaskan sistem ini seolah
  pathfinding-nya A* (KI-09).
- **`DoorbellARPopup.cs` terlihat mati tapi HIDUP** — membuat GameObject-nya sendiri lewat
  singleton getter, dipanggil dari 6 tempat. Jangan usulkan menghapusnya.
- **Ada tiga ruang koordinat** — world, XRSpace local, NavMesh. Transform `XRSpace`
  berubah tiap VPS localize. Kode yang lupa konversi menghasilkan angka salah (KI-04).
- **Duplikasi nama ruangan sengaja DITUNDA** (ADR-N002), bukan terlewat. Kalau menyentuh
  ruangan, ikuti `.claude/skills/tambah-ruangan-navigasi/`.
- **Belum ada instrumentasi apa pun** (KI-05). Jangan pernah menyatakan angka performa —
  tidak ada yang pernah diukur.

## Sebelum menyentuh scene AR

`Assets/Scenes/6_AR Navigasi.unity` berukuran **60 MB** dengan 500 GameObject dan 114 peta.

- Perubahan kecil pun menghasilkan diff besar dan sulit di-review
- Tidak ada test apa pun yang akan menangkap kalau sesuatu rusak
- **Selalu tanya dulu** sebelum mengedit scene ini, dan sebutkan risikonya

`7_AR Navigasi.unity` **tidak ikut build** — duplikat eksperimen. Jangan diedit tanpa
alasan.

## Standar kejujuran (ini bukan project biasa)

Hasilnya akan masuk publikasi ilmiah dengan pemilik project sebagai salah satu penulis.
Karena itu:

- **Jangan menulis alasan desain penulis asli seolah diketahui.** Repo diserahkan tanpa
  catatan. Semua ADR di Bagian A `DECISIONS.md` adalah **rekonstruksi dari bukti kode**,
  dan harus tetap ditandai begitu.
- **Setiap klaim faktual sebutkan file dan barisnya** supaya bisa dicek ulang.
- **Tandai `[belum terverifikasi]`** untuk apa pun yang butuh pengujian perangkat.
  Jangan naikkan jadi fakta tanpa data.
- **Kalau tidak tahu, katakan tidak tahu.** Lebih murah daripada satu kalimat karangan
  yang lolos ke paper.

## Cari solusi yang BENAR, bukan yang paling gampang

- **Sebutkan best practice-nya lebih dulu**, baru kompromi kalau memang perlu.
- **Bedakan "meredakan gejala" vs "menyembuhkan penyebab".** Kalau usulanmu cuma bikin
  masalah lebih jarang muncul, katakan terus terang — persis seperti ADR-N002
  menyatakan prosedur manual itu meredakan, bukan menyembuhkan.
- **Duplikasi data yang di-maintain manual = anti-pattern.** Setiap data punya satu
  pemilik; sisanya diturunkan, bukan disalin. KI-02 adalah bukti hidupnya di repo ini.
- **Jangan menebak saat bisa diverifikasi.** Baca kodenya, cek scene-nya, panggil
  endpoint-nya. Akar masalah yang salah didiagnosis menghasilkan perbaikan yang percaya
  diri tapi keliru.
- **Akui kalau usulan sebelumnya keliru.** Lebih murah mengoreksi di tahap usulan.

## Gerbang yang belum terbuka

Jangan bangun apa pun yang mengunci arah teknis sampai dua hal ini terjawab
(lihat `docs/ROADMAP.md` Fase 0):

1. **Status lisensi Immersal** — 52 dari 114 peta hanya ada di cloud. Kalau akun tidak
   bisa dikuasai, seluruh basis teknis bisa berubah (KI-01, ADR-N003).
2. **Kalimat klaim penelitian** — menentukan apa yang harus diukur, dan instrumentasi
   harus dipasang sebelum uji lapangan.

## Alur kerja yang diharapkan

1. Task besar: baca `docs/DECISIONS.md` dulu — cek apakah sudah ada ADR yang relevan.
2. **Jangan commit tanpa persetujuan pemilik project (Bagus).**
3. Kalau menemukan kebutuhan yang memaksa keputusan arsitektur berubah, sampaikan
   eksplisit — jangan diam-diam menyimpang dari `ARCHITECTURE.md`.
4. Temuan baru yang terverifikasi → tambahkan ke `docs/KNOWN-ISSUES.md` dengan bukti
   file:baris, jangan cuma disebut di chat.

## Jangan commit / push & Aturan Identitas AI

- **Jangan pernah push**: AI DILARANG KERAS melakukan `git push` ke remote repo. Push hanya dilakukan manual oleh pemilik project (Bagus).
- **Jangan pernah masuk collaborator / co-author**: Dilarang mencantumkan nama/identitas AI (Antigravity, Gemini, Claude, Co-authored-by, dll.) pada git history, commit, atau PR.
- **Commit butuh persetujuan (consent)**: Jangan commit tanpa persetujuan eksplisit dari pemilik project (Bagus).
- APK / AAB / hasil build (sudah ada 425 MB di riwayat — jangan tambah).
- Developer token, kredensial backend, apa pun yang rahasia.
- `Library/`, `Temp/`, `Logs/`.

