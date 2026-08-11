# Arsitektur — DarsiNavigasi

> **Cara dokumen ini dibuat:** direkonstruksi pada 2026-08-08 dengan membaca kode dan
> file scene, **bukan** dari catatan penulis aslinya (repo diserahkan tanpa dokumentasi
> apa pun). Setiap klaim di sini bisa ditelusuri ke file dan baris. Hal yang tidak bisa
> diverifikasi ditandai **[belum terverifikasi]** — jangan dikutip sebagai fakta di paper
> tanpa dicek ulang di perangkat.

---

## 1. Gambaran besar

```
┌──────────────┐   QR code    ┌──────────────────────────────────────────┐
│ 4_Scan Screen│─────────────▶│           6_AR Navigasi                  │
│   (ZXing)    │  PlayerPrefs │                                          │
└──────────────┘              │  ┌────────────┐      ┌────────────────┐  │
                              │  │ Immersal   │      │  Unity NavMesh │  │
                              │  │ VPS        │─────▶│  + NavMeshLink │  │
                              │  │ 114 peta   │ pose │  (lift/tangga) │  │
                              │  └────────────┘      └───────┬────────┘  │
                              │        ▲                     │ corners   │
                              │        │ frame kamera        ▼           │
                              │  ┌─────┴──────┐      ┌────────────────┐  │
                              │  │ARFoundation│      │ NavigationPath │  │
                              │  │  ARCore    │      │  (mesh jalur)  │  │
                              │  └────────────┘      └────────────────┘  │
                              └──────────────────────────────────────────┘
```

Tiga sistem koordinat bertemu di scene AR, dan **inilah sumber sebagian besar
kerumitannya**:

| Ruang | Milik siapa | Dipakai untuk |
|---|---|---|
| **World space** | Unity | posisi kamera AR, posisi objek ruangan |
| **XRSpace local** | Immersal (`XRSpace`) | anchor peta; transform-nya **berubah tiap kali VPS localize** |
| **NavMesh space** | Unity AI | hasil bake, statis |

`NavigationManager` mengonversi bolak-balik lewat `XRSpaceToUnity()` /
`UnityToXRSpace()` (`NavigationManager.cs:838-839`). Komponen lain yang lupa melakukan
konversi ini akan menghasilkan angka yang salah — lihat **KI-04**.

---

## 2. Lapisan lokalisasi (Immersal VPS)

### Cara kerjanya

Immersal melokalisasi dengan mencocokkan frame kamera terhadap **point cloud** hasil
pemindaian gedung. Saat cocok, SDK menggeser `XRSpace` sehingga seluruh konten AR
(titik ruangan, jalur, BIM) sejajar dengan dunia nyata.

### Cakupan peta

| | Jumlah |
|---|---|
| Peta direferensikan scene `6_AR Navigasi` | **114** |
| Punya file lokal di `Assets/Map Data/` | **62** |
| **Hanya ada di cloud Immersal** | **52** (ID `145851`–`147585`) |

Peta dikelompokkan ke 20 container di hierarki scene:

- `XRMap`, `XRMap1`–`XRMap5` → gedung **Graha**
- `XRMapT1`–`XRMapT13` → gedung **Tower**, satu per lantai
- `XRMapOutdoor` → area luar

Penamaan peta mengikuti lokasi fisik pemindaian, bukan skema formal — misalnya
`138160-DepanFarmasi`, `140524-liftFM`, `147432-igdmasuk`, `139852-yok`.

> ⚠️ 52 peta tanpa file lokal berarti aplikasi **wajib online saat pertama kali masuk
> area itu**, dan repo ini tidak cukup untuk membangun ulang sistemnya. Lihat **KI-01**.

### Konfigurasi

`ImmersalSDK` adalah prefab instance di scene AR dengan `developerToken` di-override
langsung di file scene (`6_AR Navigasi.unity:54077`). Tidak ada file konfigurasi
terpisah, tidak ada environment variable.

---

## 3. Lapisan penentuan posisi awal (QR)

Sebelum VPS berhasil localize, aplikasi tidak tahu pengguna di mana. Solusinya: QR code
di titik-titik fisik rumah sakit.

```
ScannerCameraController          InitialPositionManager
  scan QR (ZXing)                  baca PlayerPrefs
  cocokkan ke tabel  ──────────▶   cari IsNavigationTarget dgn nama itu
  simpan nama ruangan              PINDAHKAN XR Origin ke posisi ruangan + 1,5 m
  PlayerPrefs["InitialStartRoom"]
```

Tabelnya **hardcoded** di `ScannerCameraController.cs:249-274` — 23 entri, memetakan
teks QR seperti `"tower lantai 2"` ke nama ruangan `"IGD LT2"`. Pencocokan mengabaikan
huruf besar/kecil, spasi, dan underscore, serta menyediakan alias untuk typo yang
diketahui (`"Poli Tengahe"`, `"Farmmasi Graha"`).

**Sifat penting:** posisi dari QR itu **tebakan kasar dan sementara**. Begitu VPS
berhasil localize, Immersal menimpa pose `XRSpace` dan posisi QR tidak berlaku lagi.
QR hanya menutup jeda sebelum localize pertama.

---

## 4. Lapisan tujuan (63 titik ruangan)

63 instance prefab `Navigation Target` (dari Immersal Core Samples) tersebar di scene
AR. Komponennya `IsNavigationTarget`.

**Nama GameObject adalah identitasnya.** Field `targetName` dikosongkan pada semua 63
instance, sehingga `IsNavigationTarget.Start()` jatuh ke fallback
`targetName = gameObject.name`. Konsekuensinya rename objek di Hierarchy = mengganti ID
ruangan. Lihat **KI-02**.

Daftar lengkap ruangan ada di [`FLOWS.md`](FLOWS.md#daftar-ruangan).

Selain 63 target, ada satu objek khusus **`Phantom_Database_Target`** — target cadangan
yang dipindah-pindah ke koordinat dari backend kalau ruangan yang diminta tidak ada
objeknya di scene.

---

## 5. Lapisan rute (NavMesh)

`NavigationManager.DrawARPath()` (`NavigationManager.cs:681-777`) dipanggil setiap frame
saat navigasi aktif. Tiga tingkat, berurutan:

| # | Metode | Status nyata |
|---|---|---|
| 1 | **A\*** via `NavigationGraphManager.FindPath()` | **tidak pernah aktif** — 0 komponen `Waypoint` di scene, jadi selalu balik list kosong |
| 2 | **NavMesh** `NavMesh.CalculatePath()` | **inilah yang benar-benar dipakai** |
| 3 | Garis lurus start→target | jaring pengaman kalau NavMesh gagal |

### Data NavMesh

- Hasil bake: `Assets/Scenes/6_AR Navigasi/NavMesh-NavMesh.asset`
- Permukaan jalan: 19 objek `NavFloor` + puluhan `Plane` yang ditempatkan manual
  mengikuti denah — **bukan** hasil bake dari mesh BIM
- **20 `NavMeshLink`** menghubungkan antar lantai:
  `Link_Lift_Ground_to_L1` … `Link_Lift_lt12_to_lt13`, plus `NavmeshLinkGraha` dan
  `NavmeshLinkTower`

**Inilah cara navigasi lintas-lantai diselesaikan** — bukan lewat logika khusus, tapi
dengan membiarkan NavMesh memperlakukan lift sebagai edge biasa.

### Penanganan lift

Setelah rute didapat, `DrawARPath` memindai lompatan vertikal:

```csharp
if (Mathf.Abs(corners[i].y - corners[i+1].y) > m_elevatorHeightThreshold)  // 3,5 m
{
    m_elevatorPosition = corners[i];
    corners.RemoveRange(i + 1, corners.Count - (i + 1));   // potong sisa jalur
    break;
}
```

Jalur **dipotong di mulut lift** — tidak digambar menembus lantai. Saat pengguna
mendekat (< 2,5 m), `Panel_ElevatorPopup` muncul. Ini keputusan desain yang jujur:
sistem berhenti menggambar ketika ia berhenti tahu.

### Visualisasi

`NavigationPath.GeneratePath(corners, upDir)` membangun mesh pita mengikuti corner,
lebar 0,2 m, diangkat 0,5 m dari lantai. Navigasi dianggap selesai saat jarak
pengguna→target < 1,2 m.

---

## 6. Lapisan UI

| Komponen | File | Fungsi |
|---|---|---|
| Daftar & pencarian ruangan | `NavigationManager.cs:178-330` | filter teks + filter gedung Graha/Tower |
| Filter gedung | `FilterUI.cs` + dua `HashSet` di `NavigationManager.cs:30-51` | **duplikasi nama — lihat KI-02** |
| Minimap | `MiniMapFollow.cs` + `MinimapFloorManager.cs` | kamera ortografis dari atas, RenderTexture bundar |
| Notifikasi tiba | `ArrivalNotificationUI.cs` | nama ruangan + jarak + durasi, kirim riwayat |
| Panel navigasi aktif | `ActiveNavigationUI.cs` | "dari X ke Y" |
| Popup "tekan bel" | `DoorbellARPopup.cs` | 8 ruangan steril (ICU, OK, CSSD, ruang bersalin) |

`PerformanceOptimizer.cs` mengunci 60 FPS, mematikan vSync, mematikan seluruh shadow,
dan tiap detik menyembunyikan renderer point cloud Immersal supaya titik-titik peta
tidak terlihat pengguna.

---

## 7. Backend

Dua base URL berbeda hidup berdampingan (lihat README). Endpoint yang dipakai:

| Endpoint | Pemanggil | Fungsi |
|---|---|---|
| `POST /login`, `/register` | `AuthManager` | autentikasi |
| `POST /forgot-password`, `/reset-password` | `AuthManager` | OTP via email |
| `GET /api/get-room-list` | `NavigationManager` | daftar ruangan (pelengkap, bukan sumber utama) |
| `GET /api/map/{roomId}` | `NavigationManager` | koordinat ruangan |
| `POST /api/save-history` | `NavigationManager`, `ArrivalNotificationUI` | riwayat perjalanan |

**Sumber kebenaran daftar ruangan adalah scene, bukan database.**
`FetchRoomsFromDatabase()` memanggil `PopulateListFromSceneTargets()` lebih dulu, lalu
backend hanya menambah nama yang belum ada. Ini yang membuat aplikasi tetap berguna
meski backend mati.

Ada satu proteksi menarik di `NavigationManager.cs:596-600` — koordinat dari backend
ditolak kalau `|y| > 50` atau `|x| > 500`, karena database juga menyimpan koordinat
piksel denah web (Xeokit BIM) yang kalau dipakai mentah akan melempar target ke langit.

---

## 8. Kode mati

Dari 30 script runtime, **11 tidak dipakai** di build yang dikirim (0 referensi di scene
mana pun yang masuk Build Settings, 0 referensi dari kode runtime):

**Benar-benar mati** (tidak dirujuk dari mana pun):
`ARPathDistance.cs` · `CircularMiniMap.cs` · `DebounceSearch.cs` ·
`NavigationSearchBar.cs` · `NavigationSelectionManager.cs` · `SafeArea.cs` ·
`SceneController.cs`

**Yatim generator** — hanya dirujuk oleh tool editor (`UIBuilderEditor.cs`,
`SetupNavUI.cs`) yang dipakai sekali untuk membangun UI, tapi hasilnya di scene tidak
memakai script tersebut:
`NavListManager.cs` · `Spinner.cs` · `BottomNavigation.cs` · `SplashManager.cs`

**Hidup meski tidak ada di scene:** `DoorbellARPopup.cs` membuat GameObject-nya sendiri
lewat singleton getter dan dipanggil dari 6 tempat. Jangan dihapus.

**Hanya di scene 7 (tidak ikut build):** `RoomSearchFilter.cs`

**Editor tool yang percuma:** `WaypointAutoConnect.cs` — menghubungkan `Waypoint`,
padahal tidak ada satu pun `Waypoint` di scene (lihat §5).

---

## 9. Aset

| Folder | Isi |
|---|---|
| `Assets/Map Data/` | 62 peta Immersal: `.bytes` (peta biner), `-sparse.ply` (point cloud), `-metadata.json` (ID, GPS, timestamp) |
| `Assets/GlbFiles/` | model BIM per lantai: `LTG`, `LT1`–`LT4`, `Tower 1`–`Tower 13`, `Outdoor` |
| `Assets/DenahRsi/` | denah 2D `graha` dan `tower` |

Metadata peta menyimpan koordinat GPS titik pindai — contoh `138147-Lobby1` di
`-7.30620, 112.73509`. Berguna untuk memverifikasi cakupan pemetaan di paper.

---

## 10. Yang belum terverifikasi

Hal-hal berikut **tidak bisa** kupastikan dari kode saja dan harus diuji di perangkat
sebelum masuk paper atau janji ke klien:

- **[belum terverifikasi]** Akurasi lokalisasi Immersal di koridor RSI (tidak ada
  instrumentasi sama sekali di kode — lihat **KI-05**)
- **[belum terverifikasi]** Tingkat keberhasilan localize dan waktu tunggunya
- **[belum terverifikasi]** Apakah 52 peta cloud masih bisa diunduh dengan token saat ini
- **[belum terverifikasi]** Apakah NavMesh yang di-bake benar-benar tersambung di semua
  lantai Tower (bake ada, konektivitasnya belum diuji)
- **[belum terverifikasi]** Perilaku nyata `InitialPositionManager` ketika VPS localize
  di tengah jalan setelah teleport QR
