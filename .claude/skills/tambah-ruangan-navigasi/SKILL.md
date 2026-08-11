---
name: tambah-ruangan-navigasi
description: Prosedur menambah, mengganti nama, atau menghapus titik ruangan tujuan di scene AR DarsiNavigasi. Wajib dipakai karena nama ruangan diduplikasi ke tiga tempat yang di-maintain manual (KI-02) dan salinannya sudah terbukti melenceng. Gunakan saat diminta menambah ruangan, rename ruangan, ruangan tidak muncul di daftar, filter Graha/Tower menyembunyikan ruangan, atau QR tidak menemukan titik awal.
---

# Menambah / mengubah ruangan navigasi

## Kenapa ada prosedur ini

Nama ruangan di repo ini **bukan** satu data dengan satu pemilik. Nama yang sama disalin
manual ke tiga tempat, dan tiga salinan sudah melenceng (lihat `docs/KNOWN-ISSUES.md`
KI-02). Perbaikan struktural **ditunda secara sadar** (ADR-N002), jadi prosedur ini
adalah mitigasinya.

**Ini meredakan gejala, bukan menyembuhkan penyebab.** Selama nama disalin manual,
salinannya akan melenceng lagi. Kalau kamu punya kesempatan mencabut duplikasinya
(Roadmap Fase 5.1), itu lebih baik daripada mengikuti prosedur ini selamanya.

## Tiga tempat itu

| # | Lokasi | Kapan wajib diubah |
|---|---|---|
| 1 | Nama GameObject di `Assets/Scenes/6_AR Navigasi.unity` | **selalu** — ini sumber sahnya |
| 2 | `daftarGraha` / `daftarTower` di `NavigationManager.cs:30-51` | kalau ruangan harus muncul saat filter gedung aktif |
| 3 | Tabel QR di `ScannerCameraController.cs:249-274` | **hanya** kalau ruangan itu jadi titik scan QR fisik |

## Prosedur menambah ruangan

### 1. Buat objek di scene

- Duplikat salah satu instance prefab `Navigation Target` yang sudah ada (dari
  `Assets/Samples/Immersal SDK/2.2.1/Core Samples/Prefabs/Navigation/Navigation Target.prefab`)
- Tempatkan di posisi fisik ruangan, di dalam hierarki tempat target lain berada
- **Beri nama GameObject persis seperti nama ruangan yang diinginkan** — ini jadi ID-nya
- **Biarkan field `targetName` KOSONG.** Semua 63 target existing mengosongkannya dan
  jatuh ke `gameObject.name`. Mengisinya di satu objek saja menciptakan inkonsistensi
  keempat.
- Set `navigationCategory` dan `icon` mengikuti target sejenis

### 2. Daftarkan ke filter gedung

Buka `NavigationManager.cs`, tambahkan nama **persis sama** (copy-paste, jangan diketik
ulang) ke `daftarGraha` atau `daftarTower`.

Kalau dilewati: ruangan tetap muncul di daftar "Semua Gedung", tapi **hilang** begitu
pengguna menekan filter Graha atau Tower.

### 3. Tabel QR — hanya kalau perlu

Kalau ruangan ini jadi titik tempel QR fisik, tambahkan ke `builtInMappings` di
`ScannerCameraController.cs`. Formatnya `{ "teks di QR", "Nama Ruangan" }`.

`InitialPositionManager.cs:40` mencocokkan dengan `==` (bukan substring), jadi nilai
kanan harus **identik huruf per huruf** dengan nama GameObject.

### 4. Ruangan wajib tekan bel

Kalau ruangan steril (ICU, OK, CSSD, ruang bersalin/anak), tambahkan juga ke
`doorbellRooms` di `DoorbellARPopup.cs:27-33`.

### 5. Verifikasi sebelum menyerahkan

Jalankan pengecekan ini — tidak ada test otomatis yang akan menangkap kesalahannya:

```bash
# nama harus muncul identik di semua tempat yang relevan
grep -rn "Nama Ruangan Baru" \
  "Assets/Samples/Immersal SDK/2.2.1/Core Samples/Scripts/Navigation/NavigationManager.cs" \
  Assets/Scripts/ScannerCameraController.cs \
  Assets/Scripts/DoorbellARPopup.cs
```

Lalu di Unity:

- [ ] Ruangan muncul di daftar saat filter **Semua Gedung**
- [ ] Ruangan **masih** muncul saat filter **Graha** / **Tower** yang sesuai
- [ ] Mengetik sebagian namanya di search bar memunculkan ruangan itu
- [ ] Memilihnya menggambar jalur (bukan garis lurus menembus tembok — kalau lurus,
      berarti tidak ada NavMesh di posisi itu)
- [ ] Kalau ada QR-nya: scan QR mengarahkan ke scene AR dengan posisi awal yang benar

## Prosedur mengganti nama ruangan

**Rename objek di Hierarchy berarti mengganti ID ruangan.** Urutannya:

1. Cari semua kemunculan nama lama:
   ```bash
   grep -rn "Nama Lama" --include="*.cs" Assets/Scripts "Assets/Samples/Immersal SDK"
   ```
2. Ganti di **semua** hasilnya, baru rename objeknya di scene
3. Jalankan checklist verifikasi di atas

## Prosedur menghapus ruangan

Kebalikannya: hapus dari `daftarGraha`/`daftarTower`, tabel QR, `doorbellRooms`, baru
hapus objeknya. Nama yang tertinggal di daftar tidak menyebabkan error — tapi jadi
sampah yang menyesatkan pembaca berikutnya.

## Jebakan yang sudah terbukti

| Gejala | Penyebab sebenarnya |
|---|---|
| Ruangan hilang saat filter gedung aktif | typo di `daftarGraha`/`daftarTower` — contoh nyata: `"Laboratium LT3"` vs `Laboratorium LT3` |
| QR tidak menemukan titik awal | nilai di tabel QR tidak identik dengan nama GameObject — contoh nyata: `"R. Rawat Intensif"` vs `R. Rawat Intensif LT4` |
| Ruangan salah yang terpilih | pencocokan substring di `GetRoomCoordinates()` menangkap nama lain. Hindari nama yang jadi awalan nama lain |
| Jalur digambar lurus menembus tembok | tidak ada permukaan NavMesh di posisi itu — tambahkan `NavFloor`/`Plane` lalu re-bake |

## Batasan

- **Selalu tanya dulu sebelum mengedit scene AR.** File 60 MB, 500 GameObject, tidak ada
  test apa pun yang menangkap kalau sesuatu rusak.
- Setelah menambah permukaan NavMesh, **NavMesh harus di-bake ulang** dan hasilnya
  (`Assets/Scenes/6_AR Navigasi/NavMesh-NavMesh.asset`) ikut ter-commit.
