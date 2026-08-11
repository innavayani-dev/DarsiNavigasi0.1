# Known Issues — DarsiNavigasi

Semua temuan di bawah **sudah diverifikasi langsung ke kode, file scene, atau endpoint
live** pada 2026-08-08. Yang berupa dugaan ditandai eksplisit.

Severity:
**⛔ BLOCKER** — harus beres sebelum serah-terima ke klien ·
**🔴 TINGGI** — merusak fitur atau data ·
**🟡 SEDANG** — utang yang akan menggigit ·
**⚪ RENDAH** — kebersihan

---

## ⛔ KI-01 · Developer token Immersal ter-commit, dan 52 peta hanya ada di cloud

**Bukti:**
- `Assets/Scenes/6_AR Navigasi.unity:54077-54078` — `developerToken` plaintext,
  di-override pada prefab instance `ImmersalSDK`.
- Sudah masuk riwayat git; menghapusnya dari file saja tidak cukup.
- 114 peta direferensikan scene, hanya 62 punya file di `Assets/Map Data/`.
  52 sisanya (ID `145851`–`147585`) diunduh runtime dari server Immersal.

**Dampak:**
1. Siapa pun yang punya akses repo bisa memakai kuota akun Immersal itu.
2. Kalau akun/lisensinya bukan milikmu atau kedaluwarsa, **separuh gedung berhenti
   berfungsi dan repo ini tidak cukup untuk membangunnya ulang**.
3. Immersal adalah produk komersial Hexagon — status lisensi untuk penggunaan
   produksi (bukan riset/akademik) **belum diklarifikasi**.

**Yang harus dilakukan sebelum apa pun:**
1. Pastikan siapa pemilik akun Immersal dan apakah lisensinya mencakup deployment ke
   klien.
2. Rotasi token.
3. Pindahkan token keluar dari scene, ke konfigurasi yang tidak ikut commit.
4. Unduh dan simpan lokal 52 peta yang hilang, atau terima secara sadar bahwa aplikasi
   wajib online.

---

## 🔴 KI-02 · Nama ruangan diduplikasi ke tiga tempat, dan salinannya sudah melenceng

> **Status: SENGAJA DITUNDA** (keputusan pemilik project, 2026-08-08) — perbaikan
> menyentuh scene 60 MB dan berisiko memutus hal lain. Didokumentasikan supaya tidak
> hilang, bukan diabaikan.

Nama ruangan hidup di **tiga tempat yang di-maintain manual**:

| # | Lokasi | Peran |
|---|---|---|
| 1 | Nama GameObject di scene (63 objek) | sumber sah — `IsNavigationTarget.targetName` kosong, jatuh ke `gameObject.name` |
| 2 | `daftarGraha` / `daftarTower`, `NavigationManager.cs:30-51` | filter gedung |
| 3 | Tabel QR, `ScannerCameraController.cs:249-274` | posisi awal |

**Yang sudah melenceng — terverifikasi:**

| Nama di scene | Salinan | Akibat |
|---|---|---|
| `Laboratorium LT3` | `"Laboratium LT3"` (daftarTower + tabel QR) | hilang dari daftar saat filter Tower aktif; QR "tower lantai 3" tidak menemukan titik awal |
| `Coffee Bean LT6` | `"Coffe Bean LT6"` (daftarTower) | hilang dari daftar saat filter Tower aktif |
| `R. Rawat Intensif LT4` | `"R. Rawat Intensif"` (tabel QR) | QR "tower lantai 4" gagal — `InitialPositionManager` pakai `==`, bukan substring |

**Kenapa `Contains()` menyelamatkan sebagian tapi tidak semuanya:** filter gedung pakai
`namaRuanganAsli.Contains(nama)` sehingga `"R. Rawat Intensif LT4".Contains("R. Rawat
Intensif")` lolos — tapi typo huruf (`Laboratium`) tetap gagal.
`InitialPositionManager.cs:40` pakai `==` sehingga tidak toleran sama sekali.

**Perbaikan yang benar (untuk nanti):** satu pemilik sah, sisanya diturunkan. Kategori
gedung sebaiknya jadi field di `IsNavigationTarget`, bukan daftar terpisah di kode.
Jangan menyelesaikannya dengan menyalin ulang nama — salinan pasti melenceng lagi.

**Mitigasi sementara:** ikuti prosedur di
[`.agents/skills/tambah-ruangan-navigasi/SKILL.md`](../.agents/skills/tambah-ruangan-navigasi/SKILL.md)
setiap kali menambah/mengubah ruangan.

---

## 🔴 KI-03 · Backend mati; login, registrasi, dan riwayat tidak berfungsi

**Bukti (dicek 2026-08-08):**

| URL | Dipakai oleh | Hasil |
|---|---|---|
| `https://darsi-nav.hcm-lab.id/api` | `AuthManager`, `ArrivalNotificationUI` | **HTTP 404** |
| `https://indoor-nav-backend.vercel.app/api/get-room-list` | `NavigationManager` | hidup, balas `{"status":false}` |

Dua base URL berbeda hidup berdampingan di satu aplikasi — jelas sisa migrasi yang tidak
selesai.

**Dampak:** login, registrasi, lupa password, dan pencatatan riwayat gagal semua.
**Navigasi AR tetap jalan** karena `FetchRoomsFromDatabase()` mengisi daftar dari 63
objek scene lebih dulu (`NavigationManager.cs:384`) sebelum memanggil backend.

**Catatan untuk klien:** aplikasi bisa didemokan tanpa backend, tapi tidak bisa
diserahkan sebagai produk berakun.

---

## 🔴 KI-05 · Tidak ada instrumentasi sama sekali

Dari 4.253 baris kode, **nol** pengukuran: tidak ada waktu localize, tingkat
keberhasilan, error posisi, panjang rute vs jarak tempuh nyata, atau log terstruktur.

**Dampak untuk penelitian:** tidak ada satu pun angka yang bisa dilaporkan. Sistem ini
tidak pernah mengukur dirinya sendiri, jadi klaim performa apa pun sekarang adalah
kesan, bukan data.

**Minimum yang perlu dipasang sebelum uji lapangan:**
- waktu dari sesi AR mulai → localize pertama berhasil
- jumlah percobaan localize vs yang berhasil, per peta
- selisih posisi hasil VPS terhadap titik referensi yang diketahui
- rasio panjang rute NavMesh terhadap jarak yang benar-benar ditempuh

---

## 🟡 KI-04 · `ARPathDistance` memakai ruang koordinat yang salah

`ARPathDistance.cs:40` memanggil `NavMesh.CalculatePath()` dengan posisi **world**,
sedangkan `NavigationManager.cs:719-720` mengonversi ke **XRSpace local** lebih dulu.
Karena transform `XRSpace` berubah tiap kali VPS localize, angka jarak yang dihasilkan
tidak berasal dari jalur yang digambar.

**Kenapa hanya SEDANG:** script ini **tidak terpasang di scene mana pun** — jadi bug-nya
belum pernah tampil ke pengguna. Tapi jika suatu saat dipasang untuk menampilkan sisa
jarak, bug ini langsung aktif. Perbaiki atau hapus, jangan biarkan menunggu.

---

## 🟡 KI-06 · Otak navigasi berada di dalam folder `Samples/` SDK

`NavigationManager.cs` (841 baris, inti seluruh navigasi) ada di
`Assets/Samples/Immersal SDK/2.2.1/Core Samples/Scripts/Navigation/`, dan sudah diubah
berat dari versi asli Immersal — termasuk daftar ruangan hardcoded, integrasi backend,
logika lift, dan search filter.

**Risiko:** meng-import ulang atau meng-update Immersal Core Samples akan
**menimpanya tanpa peringatan.** Header filenya sudah menandai ini file modifikasi
(`"REVISI TOTAL..."`), tapi lokasinya tetap salah.

**Perbaikan:** pindahkan ke `Assets/Scripts/`, atau minimal catat di README (sudah).

---

## 🟡 KI-07 · User ID di-hardcode; identitas login tidak pernah dipakai

`NavigationManager.cs:506` dan `:627`:

```csharp
SimpanRiwayat("Muchammad Alif", startRoom, roomId, ...);
```

Seluruh riwayat perjalanan tercatat atas nama satu orang — nama pengembang aslinya.
Padahal `AuthManager.cs:147` sudah menyimpan email pengguna yang login ke
`PlayerPrefs["LoggedInUser"]`, dan **tidak ada satu pun kode yang membacanya**.

**Dampak:** data riwayat yang terkumpul sejauh ini tidak bisa dipakai untuk analisis per
pengguna — semua tercampur jadi satu identitas. Kalau riwayat ini dimaksudkan jadi data
penelitian, datanya tidak valid.

---

## 🟡 KI-08 · Password disimpan plaintext di PlayerPrefs

`AuthManager.cs:150`:

```csharp
PlayerPrefs.SetString("SavedPassword", passClean);
```

PlayerPrefs di Android adalah XML biasa di direktori aplikasi — terbaca di perangkat
yang di-root atau lewat backup. Untuk aplikasi rumah sakit, ini tidak layak serah-terima.

**Perbaikan:** simpan token sesi dari server, bukan password. Kalau backend belum
mendukung, hapus fitur auto-fill password.

**Terkait:** JSON dirakit dengan konkatenasi string (`AuthManager.cs:127`) tanpa
escaping — email/password berisi `"` atau `\` akan merusak payload.

---

## ⚪ KI-09 · A* dan tool waypoint adalah kode mati

`NavigationGraphManager` (238 baris implementasi A* lengkap) dan editor tool
`WaypointAutoConnect` tidak pernah aktif: **0 komponen `Waypoint` di scene mana pun**.
Cabang pertama `DrawARPath()` selalu gugur ke NavMesh.

Bukan bug — tapi siapa pun yang membaca kode akan mengira A* dipakai. NavMesh yang
sebenarnya bekerja.

---

## ⚪ KI-10 · 11 script runtime tidak dipakai

Daftar lengkap dan alasannya ada di
[`ARCHITECTURE.md` §8](ARCHITECTURE.md#8-kode-mati).

⚠️ **`DoorbellARPopup.cs` terlihat mati tapi HIDUP** — ia membuat GameObject-nya sendiri
lewat singleton getter dan dipanggil dari 6 tempat. Jangan dihapus.

---

## ⚪ KI-11 · Repo tidak siap didistribusikan

| | |
|---|---|
| `.git` | 1,5 GB |
| `Assets` | 1,6 GB |
| APK ter-commit | 425 MB (`DarsiNavigasi0.1.1.apk`) |
| Scene AR | 60 MB per file, dua buah (scene 6 dan 7) |
| App ID | `com.DefaultCompany.DarsiNav` — **masih default Unity** |
| `bundleVersion` | `1.0` |

Git LFS baru dipasang di commit terakhir (`c81872e`), jadi riwayat lamanya tetap gemuk.
App ID default tidak bisa dipublikasikan dan menandakan project belum pernah disiapkan
untuk rilis.

---

## ⚪ KI-12 · `7_AR Navigasi.unity` menggandakan seluruh scene AR

56 MB, 114 peta, 63 target — hampir identik dengan scene 6, tapi tidak masuk Build
Settings. Bedanya hanya memakai `RoomSearchFilter.cs`.

Setiap perubahan pada scene 6 tidak akan tercermin di sini, sehingga keduanya akan makin
menyimpang. Putuskan: jadikan cadangan resmi (dan catat), atau hapus.

---

## Ringkasan prioritas

| Urutan | Item | Kenapa duluan |
|---|---|---|
| 1 | **KI-01** lisensi & token Immersal | menentukan apakah project ini bisa dilanjutkan sama sekali |
| 2 | **KI-05** instrumentasi | harus terpasang **sebelum** uji lapangan, bukan sesudah |
| 3 | **KI-03** backend | menentukan apakah fitur berakun masuk lingkup serah-terima |
| 4 | **KI-08**, **KI-07** keamanan & identitas | syarat kelayakan aplikasi rumah sakit |
| 5 | **KI-02** duplikasi nama | ditunda atas keputusan pemilik project |
| 6 | **KI-11** kebersihan repo | sebelum handover, bukan sebelum development |
