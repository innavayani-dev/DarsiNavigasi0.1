---
name: tambah-peta-immersal
description: Prosedur menambah, mengganti, atau mengaudit peta Immersal VPS di DarsiNavigasi. Gunakan saat diminta menambah area pemindaian baru, mengganti peta yang sudah tidak cocok karena renovasi, memeriksa peta mana yang hanya ada di cloud, atau saat lokalisasi gagal di satu area tertentu.
---

# Menambah / mengaudit peta Immersal

## Yang perlu diketahui lebih dulu

| | |
|---|---|
| Peta direferensikan scene `6_AR Navigasi` | **114** |
| Punya file lokal di `Assets/Map Data/` | **62** |
| **Hanya ada di cloud Immersal** | **52** (ID `145851`–`147585`) |

52 peta tanpa file lokal berarti aplikasi **wajib online** saat pertama kali masuk area
itu, dan **repo ini tidak cukup untuk membangun ulang sistemnya**. Ini blocker KI-01 —
baca `docs/KNOWN-ISSUES.md` sebelum melangkah.

## Struktur satu peta

Setiap peta lokal terdiri dari tiga file dengan pola `{id}-{nama}`:

```
Assets/Map Data/
├── 138147-Lobby1.bytes            ← peta biner (yang dipakai runtime)
├── 138147-Lobby1-sparse.ply       ← point cloud (visualisasi/diagnostik)
└── 138147-Lobby1-metadata.json    ← ID, GPS, timestamp, versi SDK
```

Isi metadata (berguna untuk mendokumentasikan cakupan pemetaan di paper):

```json
{ "id": 138147, "name": "Lobby1", "created": "2025-11-27 14:29:53",
  "version": "1.24.0-251111", "size": 87,
  "latitude": -7.3062017692613583, "longitude": 112.73508703983065,
  "altitude": 9.0960536235943437 }
```

## Pengelompokan di scene

Peta dikelompokkan ke 20 container di hierarki scene AR:

| Container | Cakupan |
|---|---|
| `XRMap`, `XRMap1`–`XRMap5` | gedung **Graha** |
| `XRMapT1`–`XRMapT13` | gedung **Tower**, satu container per lantai |
| `XRMapOutdoor` | area luar |

Objek peta di dalamnya bernama `XR Map {id}-{nama}` dan menyimpan `m_MapId`.

**Penempatan peta di container yang benar itu penting** — lantai ditentukan oleh posisi Y
objek peta, dan `MinimapFloorManager` serta deteksi lift bergantung pada Y itu.

## Menambah peta baru

### 1. Pindai dan proses di Immersal

Pemindaian dan pemrosesan peta terjadi **di luar repo ini**, lewat aplikasi/dashboard
Immersal. Hasilnya adalah peta dengan ID numerik di akun Immersal.

> **[belum terverifikasi]** Langkah persis di Editor (nama menu / window Map Manager
> Immersal 2.2.1) tidak bisa kupastikan dari repo ini — package Immersal ada di
> `Library/PackageCache` yang tidak ikut commit. **Cek langsung di Unity Editor** sebelum
> mengikuti instruksi dari sumber mana pun, termasuk aku.

### 2. Turunkan file peta ke repo

Simpan tiga file (`.bytes`, `-sparse.ply`, `-metadata.json`) ke `Assets/Map Data/` dengan
pola nama `{id}-{nama}`.

**Lakukan ini walau peta bisa diunduh runtime.** Menyimpan lokal adalah cara satu-satunya
membuat repo ini mandiri (KI-01).

### 3. Tempatkan di scene

- Buat objek `XR Map` di dalam container yang benar (Graha / Tower lantai ke-N / Outdoor)
- Set `m_MapId` ke ID peta
- Pastikan **posisi Y** objek sesuai lantainya — ini yang dipakai deteksi lantai

### 4. Verifikasi

```bash
# bandingkan peta yang direferensikan scene vs yang punya file lokal
grep -oE "m_MapId: [0-9]+" "Assets/Scenes/6_AR Navigasi.unity" \
  | awk '{print $2}' | sort -un > /tmp/scene_ids.txt
ls "Assets/Map Data/" | grep -oE "^[0-9]+" | sort -un > /tmp/local_ids.txt

echo "peta di scene TANPA file lokal:"
comm -23 /tmp/scene_ids.txt /tmp/local_ids.txt
```

Lalu di lapangan:

- [ ] Lokalisasi berhasil di area yang baru dipindai
- [ ] Objek ruangan di area itu muncul di posisi fisik yang benar
- [ ] Peta tidak "mencuri" lokalisasi dari area tetangga (gejalanya: posisi melompat)

## Mengaudit peta yang ada

### Peta mana yang hanya di cloud

Jalankan blok `comm` di atas. Keluarannya = daftar peta yang hilang kalau akun Immersal
tidak lagi bisa diakses.

### Cakupan geografis pemetaan

```bash
grep -h "latitude\|longitude\|name" "Assets/Map Data/"*-metadata.json
```

Berguna untuk tabel cakupan pemindaian di paper.

### Kapan peta dipindai

Field `created` di metadata. Peta yang dipindai sebelum renovasi area akan gagal
melokalisasi — ini penyebab paling umum "VPS tidak jalan di satu tempat saja".

## Saat lokalisasi gagal di satu area

Urutan diagnosis, dari yang paling murah:

1. **Peta area itu ada di scene?** Cek `m_MapId`-nya terdaftar
2. **Peta itu cloud-only?** Kalau ya, cek koneksi dan validitas token
3. **Peta sudah usang?** Bandingkan `created` di metadata dengan waktu renovasi terakhir
4. **Area kurang tekstur?** VPS visual gagal di dinding polos, koridor gelap, dan area
   yang tata letaknya berubah — ini batas teknologinya, bukan bug
5. **Point cloud sejajar?** Ini butuh toggle diagnostik yang **belum ada** —
   `PerformanceOptimizer.HidePointClouds()` menyembunyikan semuanya tiap detik tanpa cara
   mematikannya (ADR-R007, Roadmap 2.6)

## Batasan

- **Selalu tanya dulu sebelum mengedit scene AR** (60 MB, tanpa test).
- Menambah peta menaikkan ukuran scene dan waktu load. 114 peta sudah banyak — pastikan
  peta baru benar-benar menutup area yang belum tercakup, bukan menduplikasi.
- Jangan hapus peta lama tanpa memverifikasi tidak ada area yang jadi kosong.
