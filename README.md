# DarsiNavigasi — AR Indoor Navigation RS Islam A. Yani Surabaya

Aplikasi Android AR untuk memandu pengunjung RS Islam Ahmad Yani Surabaya ke ruangan
tujuan, memakai **visual positioning (Immersal VPS)** untuk mengetahui posisi pengguna
di dalam gedung, dan **Unity NavMesh** untuk menghitung rutenya.

Mencakup dua gedung: **Graha** (Ground–LT5) dan **Tower** (LT1–LT13), total **114 peta
VPS** dan **63 titik ruangan**.

> **Status: prototipe hasil tugas akhir, BELUM siap serah-terima.**
> Baca [`docs/KNOWN-ISSUES.md`](docs/KNOWN-ISSUES.md) sebelum menjanjikan apa pun ke
> klien. Ada blocker lisensi dan kredensial yang harus beres lebih dulu.

---

## Mulai dari mana

| Kamu ingin… | Baca ini |
|---|---|
| Paham sistemnya secara utuh | [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) |
| Paham urutan layar & alur runtime AR | [`docs/FLOWS.md`](docs/FLOWS.md) |
| Tahu kenapa dibangun begini | [`docs/DECISIONS.md`](docs/DECISIONS.md) |
| Tahu apa yang rusak / berisiko | [`docs/KNOWN-ISSUES.md`](docs/KNOWN-ISSUES.md) |
| Tahu urutan kerja menuju rilis | [`docs/ROADMAP.md`](docs/ROADMAP.md) |
| Menambah ruangan atau peta baru | [`.claude/skills/`](.claude/skills/) |

---

## Tech stack

| Komponen | Versi / catatan |
|---|---|
| Unity | **2022.3.62f2** (LTS) |
| VPS | **Immersal SDK 2.2.1** (`com.immersal.core`, git package) |
| AR runtime | AR Foundation 5.2 · ARCore 5.2 · ARKit 5.2 |
| Pathfinding | Unity NavMesh + NavMeshLink (lintas lantai) |
| QR | ZXing (`Assets/Plugins`) |
| UI | uGUI + TextMeshPro |
| Backend | HTTP JSON ke Vercel (lihat catatan di bawah) |
| Target | Android, minSdk **26**, IL2CPP |

---

## Menjalankan project

```bash
git lfs install
git clone <repo-url>
```

> Repo ini **±3 GB** (`.git` 1,5 GB + `Assets` 1,6 GB) dan memakai Git LFS untuk
> `*.png *.jpg *.jpeg *.mp4 *.apk *.aab *.unitypackage`. Clone butuh waktu.

1. Buka dengan **Unity 2022.3.62f2** (versi lain berisiko upgrade paksa pada scene 60 MB).
2. Scene utama: `Assets/Scenes/6_AR Navigasi.unity`.
3. Untuk uji di Editor tanpa HP: scene AR punya `WASDSimulator` — gerak pakai WASD.
4. Build Android: `File → Build Settings → Android → Build`.

### Kredensial yang dibutuhkan

Aplikasi **tidak akan melokalisasi** tanpa developer token Immersal yang valid.
Token saat ini tertanam di dalam scene `6_AR Navigasi.unity` (bukan di file konfigurasi
terpisah). Lihat **KI-01** di [`docs/KNOWN-ISSUES.md`](docs/KNOWN-ISSUES.md) — ini utang
keamanan yang harus dibereskan sebelum repo dibagikan ke pihak lain.

---

## Struktur repo

```
Assets/
├── Scenes/               7 scene aplikasi (6_AR Navigasi = inti, 60 MB)
│   └── 6_AR Navigasi/    NavMesh hasil bake
├── Map Data/             62 peta Immersal (.bytes + .ply + metadata)
├── GlbFiles/             model BIM per lantai (LTG, LT1–LT4, Tower 1–13, Outdoor)
├── DenahRsi/             denah Graha & Tower
├── Scripts/              26 script aplikasi  ← lihat catatan kode mati di ARCHITECTURE
├── Editor/               5 tool editor (generator UI & waypoint)
├── Samples/Immersal SDK/ sample SDK — NavigationManager.cs DIMODIFIKASI di sini
└── Plugins/              ZXing dll
docs/                     dokumentasi (ditulis 2026-08-08, bukan oleh penulis asli)
.claude/                  panduan & skill untuk AI coding assistant
```

> ⚠️ `NavigationManager.cs` — otak navigasi — berada di dalam folder **`Samples/`**
> SDK, bukan di `Assets/Scripts/`. File itu sudah diubah berat dari versi asli Immersal.
> Meng-import ulang sample SDK akan menimpanya. Lihat **KI-06**.

---

## Backend

Ada **dua** base URL berbeda di dalam kode yang sama:

| Dipakai oleh | URL | Status per 2026-08-08 |
|---|---|---|
| `AuthManager`, `ArrivalNotificationUI` | `https://darsi-nav.hcm-lab.id/api` | **404 — mati** |
| `NavigationManager` | `https://indoor-nav-backend.vercel.app` | hidup, balas `{"status":false}` |

Efeknya: **login, registrasi, reset password, dan riwayat perjalanan tidak berfungsi.**
Navigasi AR sendiri tetap jalan karena daftar ruangan diisi dari 63 objek di scene lebih
dulu, sebelum backend dipanggil. Detail di **KI-03**.

---

## Peringatan untuk kontributor baru

1. **Jangan import ulang Immersal Core Samples.** Akan menimpa `NavigationManager.cs`.
2. **Jangan rename objek ruangan di scene.** Nama GameObject *adalah* ID ruangan —
   dipakai oleh QR mapping, filter gedung, dan popup bel. Lihat **KI-02**.
3. **Jangan commit APK / build.** Sudah ada 425 MB APK di riwayat; jangan tambah lagi.
4. **Scene `7_AR Navigasi.unity` tidak ikut build.** Duplikat eksperimen dari scene 6.

---

## Lisensi & atribusi

Prototipe awal dikerjakan sebagai tugas akhir (PENS) oleh tim Innav; header
`NavigationManager.cs` mencantumkan nama pengembang aslinya. Immersal SDK adalah produk
komersial Hexagon — **status lisensi untuk penggunaan produksi belum diklarifikasi**
(lihat **KI-01**).
