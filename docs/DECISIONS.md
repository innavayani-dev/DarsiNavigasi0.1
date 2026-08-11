# Keputusan Arsitektur — DarsiNavigasi

Dokumen ini punya **dua bagian yang harus dibedakan dengan tegas**, terutama karena
sebagian isinya akan dikutip di publikasi ilmiah:

- **Bagian A — ADR Rekonstruksi (`ADR-R###`).** Keputusan yang *terbaca dari kode*, tapi
  **alasan penulis aslinya tidak diketahui** — repo diserahkan tanpa dokumentasi. Yang
  tertulis di bagian "Alasan" adalah **rekonstruksi dari bukti kode**, bukan pernyataan
  penulisnya. Jangan dikutip sebagai maksud desain aslinya.
- **Bagian B — ADR Baru (`ADR-N###`).** Keputusan yang diambil sejak project ini
  dilanjutkan, lengkap dengan tanggal dan pihak yang memutuskan.

---

# Bagian A — ADR Rekonstruksi

## ADR-R001 · Lokalisasi memakai Immersal VPS (visual), bukan WiFi/BLE

**Bukti:** `Packages/manifest.json` → `com.immersal.core`; 114 peta point cloud di scene;
tidak ada satu pun kode WiFi/BLE/magnetometer.

**Rekonstruksi alasan:** VPS visual memberi presisi sub-meter yang dibutuhkan untuk
overlay AR. Fusi radio/inersia (mis. pendekatan komersial seperti Situm) berada di kelas
akurasi 1–5 m — cukup untuk peta 2D, **tidak** cukup untuk menempelkan panah ke lantai.

**Konsekuensi:**
- Butuh pemindaian fisik seluruh gedung (114 peta terkumpul) — mahal di awal
- Gagal di area gelap, kosong tekstur, atau berubah tata letaknya
- Tidak ada posisi sama sekali sebelum localize pertama berhasil → butuh ADR-R004
- Mengikat project ke vendor komersial → lihat **KI-01**

**Status:** berlaku. **Belum tervalidasi angka** — akurasinya tidak pernah diukur
(**KI-05**).

---

## ADR-R002 · Pathfinding memakai Unity NavMesh, bukan graf waypoint A*

**Bukti:** `NavMesh-NavMesh.asset` ter-bake; 19 `NavFloor` + puluhan `Plane` manual;
20 `NavMeshLink`. Sementara `NavigationGraphManager` (A*, 238 baris) ada tapi
**0 komponen `Waypoint`** di scene mana pun, sehingga cabang A* di `DrawARPath()` selalu
gugur.

**Rekonstruksi alasan:** A* berbasis waypoint menuntut penempatan dan penyambungan node
manual (editor tool `WaypointAutoConnect` dibuat untuk itu, lalu ditinggalkan). NavMesh
memberi jalur bebas-hambatan dari permukaan yang digambar, tanpa merawat graf.

**Konsekuensi:**
- Rute mengikuti geometri, bukan daftar titik — otomatis menghindari rintangan
- Lintas lantai selesai lewat `NavMeshLink`, tanpa logika khusus (**ini kekuatan
  terbesar sistem ini**)
- Permukaan jalan digambar manual, tidak di-bake dari BIM → akurasinya tergantung
  ketelitian penempatan plane
- A* dan `WaypointAutoConnect` jadi kode mati yang menyesatkan pembaca (**KI-09**)

**Status:** berlaku. Konektivitas NavMesh antar lantai Tower **belum diverifikasi**.

---

## ADR-R003 · Nama GameObject adalah ID ruangan

**Bukti:** 63 instance `Navigation Target`, `targetName` kosong pada **semua**-nya,
sehingga `IsNavigationTarget.Start()` jatuh ke `targetName = gameObject.name`.
Pencocokan di seluruh sistem memakai string nama itu.

**Rekonstruksi alasan:** ID paling murah yang sudah tersedia — tidak perlu registry,
tidak perlu ScriptableObject, langsung terbaca di Hierarchy.

**Konsekuensi:**
- Rename di Hierarchy = mengganti ID, tanpa peringatan apa pun
- Tidak ada ID stabil yang independen dari nama tampilan
- Memaksa pencocokan bertingkat (exact → substring) untuk menutupi ketidakcocokan
  (`NavigationManager.cs:437-480`)
- **Duplikasi nama ke tabel QR dan daftar filter sudah melenceng** (**KI-02**)

**Status:** berlaku, dengan cacat terdokumentasi. Perbaikan ditunda — lihat **ADR-N002**.

---

## ADR-R004 · QR code sebagai posisi awal sementara, bukan sistem posisi

**Bukti:** `ScannerCameraController` → `PlayerPrefs["InitialStartRoom"]` →
`InitialPositionManager` memindahkan XR Origin ke posisi ruangan + 1,5 m. VPS kemudian
menimpa pose `XRSpace` saat localize berhasil.

**Rekonstruksi alasan:** menutup jeda kosong antara aplikasi dibuka dan VPS berhasil
melokalisasi. QR di titik fisik memberi tebakan yang jauh lebih baik daripada origin.

**Konsekuensi:**
- Pengguna melihat sesuatu sejak detik pertama, bukan layar menunggu
- Tabel QR jadi duplikasi ketiga dari nama ruangan (**KI-02**)
- **Risiko yang belum ditangani:** kalau QR salah **dan** VPS tidak pernah localize,
  pengguna dipandu dari titik awal yang keliru **tanpa peringatan apa pun**. Sistem tidak
  membedakan "posisi tebakan" dan "posisi terverifikasi"

**Status:** berlaku. Risiko di atas **belum diuji lapangan** dan menurutku layak jadi
ADR baru begitu ada datanya.

---

## ADR-R005 · Scene adalah sumber kebenaran daftar ruangan; backend hanya pelengkap

**Bukti:** `FetchRoomsFromDatabase()` memanggil `PopulateListFromSceneTargets()` **lebih
dulu** (`NavigationManager.cs:384`), baru menembak `GET /api/get-room-list` dan hanya
menambah nama yang belum ada. `GetRoomCoordinates()` juga mencari di scene dulu, backend
belakangan.

**Rekonstruksi alasan:** koordinat 3D hanya bermakna dalam ruang peta Immersal —
database tidak bisa jadi pemiliknya. Backend berperan untuk metadata dan riwayat.

**Konsekuensi:**
- **Aplikasi tetap berguna saat backend mati** — dan itu memang terjadi sekarang
  (**KI-03**)
- Menambah ruangan berarti mengedit scene, bukan mengisi database
- Ada proteksi eksplisit menolak koordinat piksel denah web Xeokit
  (`NavigationManager.cs:596-600`) — bukti bahwa database memang menyimpan koordinat dari
  sistem lain yang tidak sepadan

**Status:** berlaku, dan menurutku ini keputusan terbaik di repo ini.

---

## ADR-R006 · Jalur dipotong di mulut lift, tidak digambar menembus lantai

**Bukti:** `NavigationManager.cs:743-754` — bila `|Δy|` antar corner > 3,5 m, posisi lift
disimpan dan **sisa jalur dibuang**. Popup lift muncul saat pengguna < 2,5 m dari titik
itu.

**Rekonstruksi alasan:** menggambar pita AR menembus plafon ke lantai berikutnya
menyesatkan secara visual — pengguna tidak bisa berjalan lurus ke atas.

**Konsekuensi:**
- Sistem berhenti menggambar tepat ketika ia berhenti bisa memandu → jujur
- Pengguna diberi instruksi tekstual (popup lift), bukan panah palsu
- Ambang 3,5 m dipilih tanpa alasan tercatat; tangga landai bisa lolos deteksi

**Status:** berlaku. Ini pola yang layak dipertahankan.

---

## ADR-R007 · Point cloud Immersal disembunyikan dari pengguna saat runtime

**Bukti:** `PerformanceOptimizer.HidePointClouds()` berjalan tiap 1 detik, mematikan
renderer yang namanya mengandung `.vis`/`.bytes`/`pointcloud` atau punya komponen
`Immersal.XR.XRMap`. Semua shadow juga dimatikan, target 60 FPS, vSync off.

**Rekonstruksi alasan:** point cloud adalah alat diagnostik, bukan konten produk.
Dijalankan berulang (bukan sekali) karena peta diunduh dan di-instantiate secara dinamis
sepanjang sesi.

**Konsekuensi:**
- Tampilan bersih; beban render turun drastis di scene dengan 114 peta
- **Tidak ada cara menyalakannya kembali** untuk diagnosis di lapangan — padahal overlay
  point cloud adalah satu-satunya cara memeriksa kesejajaran peta secara visual
- Polling tiap detik selamanya, padahal cukup dipicu saat peta baru dimuat

**Status:** berlaku. Saranku: sediakan toggle diagnostik sebelum uji lapangan — tanpa itu
kesejajaran peta tidak bisa diperiksa mata.

---

# Bagian B — ADR Baru

## ADR-N001 · Dokumentasi ditulis secara rekonstruktif, dengan penandaan ketidakpastian

**Tanggal:** 2026-08-08 · **Diputuskan oleh:** pemilik project

**Konteks:** repo diserahkan tanpa README, tanpa ADR, tanpa catatan apa pun. Project ini
akan (a) diserahkan ke klien dan (b) jadi struktur awal penelitian dengan pemilik project
sebagai salah satu penulis.

**Keputusan:** dokumentasi disusun dengan membaca kode dan file scene, dengan aturan:

1. Setiap klaim faktual menyebut file dan baris yang bisa dicek ulang.
2. Alasan desain penulis asli **tidak boleh ditulis seolah diketahui** — ditandai sebagai
   rekonstruksi (Bagian A di atas).
3. Hal yang butuh pengujian perangkat ditandai **[belum terverifikasi]** dan tidak boleh
   dikutip sebagai fakta.

**Alasan:** dokumen ini akan jadi bahan bagian *system description* di publikasi. Mengarang
niat penulis asli, atau menyatakan performa yang tidak pernah diukur, adalah masalah
integritas — bukan sekadar dokumentasi yang kurang rapi.

**Konsekuensi:** dokumen terlihat lebih ragu-ragu daripada dokumentasi biasa. Itu
disengaja dan harus dipertahankan sampai angkanya benar-benar ada.

---

## ADR-N002 · Perbaikan duplikasi nama ruangan ditunda

**Tanggal:** 2026-08-08 · **Diputuskan oleh:** pemilik project

**Konteks:** nama ruangan diduplikasi ke tiga tempat dan tiga di antaranya sudah
melenceng, memutus dua fitur (**KI-02**).

**Keputusan:** **tunda.** Alasan pemilik project: perbaikannya menyentuh scene 60 MB dan
berisiko memutus hal lain di saat sistem belum punya pengujian apa pun.

**Yang dilakukan sebagai gantinya:**
1. Cacatnya didokumentasikan lengkap dengan bukti di **KI-02**
2. Prosedur manual dibuat supaya penambahan ruangan berikutnya tidak menambah
   penyimpangan → `.agents/skills/tambah-ruangan-navigasi/`

**Catatan jujur:** ini meredakan gejala, bukan menyembuhkan penyebab. Selama nama disalin
manual, salinannya akan melenceng lagi. Perbaikan yang benar adalah menurunkan kategori
gedung dari satu pemilik sah (field di `IsNavigationTarget`), bukan merawat tiga daftar.
Ditunda secara sadar, bukan dilupakan.

**Ditinjau ulang saat:** ada instrumentasi/pengujian yang bisa membuktikan perubahan
scene tidak merusak apa pun.

---

## ADR-N003 · Basis produk tetap Unity + Immersal, keputusan final menunggu status lisensi

**Tanggal:** 2026-08-08 · **Status: TENTATIF — menunggu jawaban**

**Konteks:** ada dua sistem paralel — repo ini (Immersal, 114 peta, cakupan luas,
tak terdokumentasi) dan repo DARSI (MultiSet + WebXR, cakupan sempit, terdokumentasi
rapi). Klien hanya bisa menerima satu.

**Arah tentatif:** jadikan repo ini basis produk, dan bawa **metode kerja** dari DARSI
(ADR, field test, disiplin verifikasi) ke sini. Alasannya: 114 peta dan navigasi lift
yang berfungsi butuh berbulan-bulan untuk dibangun ulang, sedangkan metode bisa
dipindahkan dalam hitungan hari.

**Yang membatalkan arah ini:** kalau lisensi Immersal ternyata tidak bisa dikuasai
(**KI-01**). Maka "melanjutkan" berubah jadi "memetakan ulang", dan basis MultiSet jadi
lebih masuk akal.

**Belum diputuskan.** Jangan bangun apa pun yang mengunci salah satu arah sampai
**KI-01** terjawab.
