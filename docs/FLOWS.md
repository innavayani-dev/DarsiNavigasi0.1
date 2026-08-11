# Alur — DarsiNavigasi

Dokumen ini melengkapi [`ARCHITECTURE.md`](ARCHITECTURE.md): kalau dokumen itu
menjelaskan *apa* komponennya, dokumen ini menjelaskan *kapan* dan *dalam urutan apa*
mereka berjalan.

---

## 1. Alur layar

```
0_splash screen
      │  SplashScreenController (fade, delay)
      ▼
   1_Login ◀──────────┐
      │               │ registrasi sukses
      │               │
      │          2_regis
      │
      │ AuthManager: POST /login → status:true
      ▼
3_halaman utama ─────────────▶ 5_Tentang Aplikasi
      │
      │ SceneFlowManager.GoToScanner()
      ▼
4_Scan Screen  (ZXing, kamera WebCamTexture)
      │
      │ QR cocok → PlayerPrefs["InitialStartRoom"]
      │ kamera dimatikan, tunggu 0,5 dtk (lepas hardware)
      ▼
6_AR Navigasi  ← seluruh AR ada di sini
```

**Scene yang ada di build:** 0, 1, 2, 3, 5, 4, 6 (urutan sesuai Build Settings).
**`7_AR Navigasi` TIDAK ikut build** — duplikat eksperimen, 114 peta & 63 target sama,
bedanya memakai `RoomSearchFilter.cs` yang tidak dipakai di scene 6.

### Navigasi balik

`SceneFlowManager` menyimpan `PlayerPrefs["PrevScene"]` sebelum pindah ke scanner, lalu
`GoBack()` membacanya. Default kalau kosong: `1_Login`.

---

## 2. Alur runtime di scene AR

Ini bagian paling rumit dan paling rawan salah paham. Urutannya:

```
┌─ Awake ────────────────────────────────────────────────────────┐
│ PerformanceOptimizer  : 60 FPS, vSync off, semua shadow off    │
│ IsNavigationTarget×63 : SetVisible(false)  ← semua disembunyikan│
│ ArrivalNotificationUI : singleton, panel disembunyikan          │
└────────────────────────────────────────────────────────────────┘
                              ▼
┌─ Start ────────────────────────────────────────────────────────┐
│ InitialPositionManager:                                         │
│   baca PlayerPrefs["InitialStartRoom"]                          │
│   cari IsNavigationTarget dengan nama itu (perbandingan ==)     │
│   PINDAHKAN XR Origin ke posisi ruangan, Y + 1,5 m              │
│   salin juga rotasinya                                          │
│                                                                 │
│ NavigationManager.Start():                                      │
│   InitializeNavigationManager() → cari XRSpace, Camera.main,    │
│                                   instantiate prefab jalur      │
│   FetchRoomsFromDatabase()                                      │
└────────────────────────────────────────────────────────────────┘
                              ▼
┌─ berjalan terus ───────────────────────────────────────────────┐
│ Immersal VPS: cocokkan frame kamera → 114 peta                  │
│   BERHASIL → geser transform XRSpace                            │
│              ↑ ini MENIMPA posisi hasil teleport QR di atas     │
│                                                                 │
│ PerformanceOptimizer.HidePointClouds() tiap 1 detik             │
└────────────────────────────────────────────────────────────────┘
```

**Poin yang mudah salah dipahami:** teleport QR di `Start()` dan lokalisasi VPS adalah
dua mekanisme yang **saling menimpa, bukan saling melengkapi**. QR memberi posisi
sementara supaya UI tidak kosong; begitu VPS localize, posisi QR tidak berarti lagi.
Kalau QR salah dan VPS tidak pernah localize, pengguna dipandu dari titik awal yang
keliru tanpa peringatan apa pun. **[belum terverifikasi]** — perlu uji lapangan.

---

## 3. Alur memilih tujuan

```
user mengetik / menekan ruangan di daftar
              │
              ▼
      OnRoomSelected(roomName)
              │
              ▼
     GetRoomCoordinates(roomName)
              │
    ┌─────────┴──────────┐
    │  Pass 1: exact match di scene (OrdinalIgnoreCase)
    │  Pass 2: substring match di scene (nama ≥ 4 huruf)
    └─────────┬──────────┘
              │
      ketemu? ─── ya ──▶ pakai objek scene, mulai navigasi, selesai
              │
              no
              ▼
      GET /api/map/{roomId}
              │
      ┌───────┴────────┐
      │ Pass 1 exact, Pass 2 substring (nama ≥ 5 huruf,
      │ "Poli" & "Farmasi" polos dikecualikan)
      └───────┬────────┘
              │
      masih tidak ketemu?
              ▼
      pakai koordinat mentah dari database
      → Phantom_Database_Target dipindah ke situ
      → DITOLAK kalau |y| > 50 atau |x| > 500  (proteksi koordinat Xeokit BIM)
```

Setelah target ditentukan:

1. Semua 63 target lain `SetVisible(false)`, target terpilih `SetVisible(true)`
2. `m_navigationState = Navigating`
3. `DoorbellARPopup.CheckAndShow()` — cek apakah ruangan wajib tekan bel
4. `ActiveNavigationUI.ShowNavigation(dari, ke)`
5. `SimpanRiwayat(...)` → `POST /api/save-history`
6. Daftar ruangan ditutup

---

## 4. Alur menggambar jalur (tiap frame)

```
Update() → jika Navigating → DrawARPath()
    │
    ├─ jarak user→target < 1,2 m?
    │     ya → ArrivalNotificationUI.ShowArrival(nama, jarak awal, durasi)
    │          StopNavigation()  → selesai
    │
    ├─ Tingkat 1: NavigationGraphManager.FindPath()
    │     → SELALU kosong (0 Waypoint di scene)
    │
    ├─ Tingkat 2: NavMesh.CalculatePath()
    │     start & target dikonversi world → XRSpace local dulu
    │     hasil corner dikonversi balik XRSpace local → world
    │     ← INI yang benar-benar dipakai
    │
    ├─ Tingkat 3: garis lurus start→target (kalau dua-duanya gagal)
    │
    ├─ Deteksi lift: cari lompatan |Δy| > 3,5 m antar corner
    │     ketemu → simpan posisi lift, POTONG sisa jalur, tandai
    │
    ├─ NavigationPath.GeneratePath(corners, up)  → mesh pita 0,2 m
    │
    └─ Popup lift: tampil jika ada lift di jalur DAN jarak user < 2,5 m
```

---

## 5. Alur data & penyimpanan

### PlayerPrefs (persisten di HP)

| Key | Ditulis oleh | Dibaca oleh |
|---|---|---|
| `InitialStartRoom` | `ScannerCameraController` | `InitialPositionManager`, `NavigationManager` |
| `LoggedInUser` | `AuthManager` (saat login sukses) | **tidak ada yang membaca** — lihat KI-07 |
| `SavedEmail`, `SavedPassword` | `AuthManager` | `AuthManager` (auto-fill) |
| `PrevScene` | `SceneFlowManager` | `SceneFlowManager.GoBack()` |
| `KoordinatTujuan` | `NavigationSelectionManager` | **tidak ada** (script itu sendiri kode mati) |

> ⚠️ `SavedPassword` disimpan **plaintext** di PlayerPrefs. Lihat **KI-08**.

### Jaringan

Semua request memakai `UnityWebRequest`, JSON dirakit dengan **konkatenasi string**
(bukan serializer), dan respons dicek dengan `respon.Contains("\"status\":true")` —
bukan parsing. Rapuh terhadap perubahan format backend sekecil apa pun.

---

## 6. Daftar ruangan

63 objek `Navigation Target` di scene AR. **Nama GameObject = ID ruangan.**

### Gedung Graha (Ground–LT5)

`BPJS Center` · `Bank Mega Syariah` · `Bilik Dahak` · `Farmasi` · `Hemodialisis LT1` ·
`Irna Anak LT3` · `Irna Bedah LT2` · `Irna Dewasa LT4` · `Kamar Jenazah` · `Kasir` ·
`Klinik Vaksin LT1` · `Layanan Transportasi` · `Lift` · `Loket Pendaftaran` ·
`Mandiri BPJS` · `Masjid` · `Minimarket Kantin` · `Poli 1` · `Poli 2` · `Poli Bedah` ·
`Poli Gigi` · `Poli Mata` · `Poli Ortopedi` · `Poli Penyakit Dalam` ·
`Poli Spesialis Jantung` · `Poli Spesialis Paru` · `Poli Syaraf` · `Poli THT` ·
`Poli Urologi LT3` · `R. Direksi LT5` · `R. Komdik LT5` · `R. Madinah LT3` ·
`R. Makkah LT3` · `R. Managemen LT5` · `R. Multazam` · `R. Oprasi Gigi` ·
`R. Poli Anak` · `R. Poli Anak Laktasi` · `R. Poli Kandungan` · `R. TB DOTS` ·
`R. Thaif LT2` · `Rehabilitasi Medik` · `Resepsionis` · `Ruang Gizi` · `Toilet Umum`

### Gedung Tower (LT1–LT13)

`Gedung Tower LT1` · `IGD LT2` · `Farmasi LT2` · `Laboratorium LT3` · `Radiologi LT3` ·
`R. Rawat Intensif LT4` · `R. Operasi LT5` · `CSSD LT5` · `Poli Eksekutif LT6` ·
`Coffee Bean LT6` · `Farmasi LT6` · `An-Nisa Irna Bersalin LT7` ·
`Al-Kautsar Irna Anak LT8` · `Ar-Rayyan Irna Dewasa LT9` ·
`Ar-Radhiin Irna Dewasa LT10` · `Ar-Raudhah Irna Eksekutif LT11` · `Kordik LT12` ·
`Yarsis LT13`

### Ruangan wajib tekan bel

`R. Rawat Intensif LT4` · `R. Operasi LT5` · `CSSD LT5` · `An-Nisa Irna Bersalin LT7` ·
`Al-Kautsar Irna Anak LT8` · `Ar-Rayyan Irna Dewasa LT9` ·
`Ar-Radhiin Irna Dewasa LT10` · `Ar-Raudhah Irna Eksekutif LT11`

> Nama-nama di atas juga disalin ke `daftarGraha`/`daftarTower`
> (`NavigationManager.cs:30-51`) dan sebagian ke tabel QR
> (`ScannerCameraController.cs:249-274`). **Salinannya sudah melenceng** —
> lihat **KI-02** sebelum menambah atau mengubah ruangan.
