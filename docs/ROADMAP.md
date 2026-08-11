# Roadmap — dari prototipe ke serah-terima

**Dua tujuan yang harus dipenuhi sekaligus:**

1. **Produk** — diserahkan ke klien (RS Islam A. Yani) sebagai aplikasi yang bisa
   dipakai dan dirawat
2. **Penelitian** — jadi struktur awal penelitian, dengan pemilik project sebagai salah
   satu penulis

Keduanya menuntut hal yang berbeda. Produk menuntut **bisa dirawat orang lain**.
Penelitian menuntut **bisa diukur dan direproduksi**. Roadmap ini memenuhi keduanya
dalam satu urutan.

---

## Fase 0 — Gerbang keputusan (SEKARANG, belum boleh ngoding)

Dua pertanyaan yang jawabannya menentukan apakah pekerjaan berikutnya terpakai atau
terbuang.

| # | Pertanyaan | Kenapa memblokir |
|---|---|---|
| **0.1** | Siapa pemilik lisensi Immersal, dan apakah mencakup deployment ke klien? | 52 dari 114 peta hanya ada di cloud Immersal. Kalau akun tidak bisa dikuasai, repo ini tidak cukup untuk membangun ulang sistemnya — "melanjutkan" berubah jadi "memetakan ulang" (**KI-01**, **ADR-N003**) |
| **0.2** | Apa kalimat klaim penelitiannya? | Menentukan apa yang harus diukur, dan instrumentasi harus dipasang **sebelum** uji lapangan. "Sistem navigasi indoor untuk RS" tidak bisa diuji; "VPS visual mencapai akurasi X m di koridor RS" bisa |

**Status:** ⏸ menunggu jawaban.

**Yang boleh dikerjakan sambil menunggu** (tidak terbuang di skenario mana pun):
dokumentasi (✅ selesai), rotasi token, pembersihan repo.

---

## Fase 1 — Kelayakan dasar

Tidak tergantung jawaban Fase 0.

- [x] **1.1** Dokumentasi arsitektur, alur, ADR, known issues — *selesai 2026-08-08*
- [x] **1.2** README + panduan AI assistant — *selesai 2026-08-08*
- [ ] **1.3** **Rotasi developer token Immersal** dan pindahkan keluar dari file scene
      (**KI-01**)
- [ ] **1.4** Ganti app ID `com.DefaultCompany.DarsiNav` → identitas sebenarnya, set
      `bundleVersion` yang berarti (**KI-11**)
- [ ] **1.5** Hapus APK 425 MB dari tracking; pastikan `.gitignore` menutup build
      (**KI-11**)
- [ ] **1.6** Putuskan nasib `7_AR Navigasi.unity` — cadangan resmi atau hapus
      (**KI-12**)

**Kriteria selesai:** repo bisa di-clone dan di-build oleh orang lain tanpa membocorkan
kredensial siapa pun.

---

## Fase 2 — Instrumentasi (prasyarat penelitian)

**Harus selesai sebelum uji lapangan.** Mengukur setelah pengujian berarti mengulang
pengujiannya.

- [ ] **2.1** Catat waktu: sesi AR mulai → localize pertama berhasil
- [ ] **2.2** Catat percobaan localize vs berhasil, **per ID peta** — ini yang
      mengungkap peta mana yang lemah
- [ ] **2.3** Catat selisih posisi VPS terhadap titik referensi yang diketahui
      (butuh titik uji fisik yang ditandai di lapangan)
- [ ] **2.4** Catat panjang rute NavMesh vs jarak yang benar-benar ditempuh
- [ ] **2.5** Ekspor log terstruktur (CSV/JSON) yang bisa diambil dari perangkat
- [ ] **2.6** Toggle diagnostik untuk menampilkan kembali point cloud — satu-satunya
      cara memeriksa kesejajaran peta dengan mata (**ADR-R007**)

**Kriteria selesai:** satu sesi berjalan menghasilkan file data yang bisa langsung
dianalisis, tanpa mencatat manual.

---

## Fase 3 — Uji lapangan & validasi

Baru bermakna setelah Fase 2. Semua item bertanda **[belum terverifikasi]** di
[`ARCHITECTURE.md`](ARCHITECTURE.md#10-yang-belum-terverifikasi) dijawab di sini.

- [ ] **3.1** Verifikasi 52 peta cloud masih bisa diunduh dengan token baru
- [ ] **3.2** Uji konektivitas NavMesh di seluruh lantai Tower — apakah rute LT1→LT13
      benar-benar terbentuk
- [ ] **3.3** Ukur akurasi lokalisasi per gedung dan per lantai
- [ ] **3.4** Uji perilaku QR + VPS: apa yang terjadi kalau QR salah dan VPS tidak
      pernah localize (**ADR-R004**, risiko yang belum ditangani)
- [ ] **3.5** Uji navigasi lintas-lantai end-to-end, termasuk popup lift
- [ ] **3.6** Catat semua hasil ke `docs/FIELD-TESTS.md` (belum ada, dibuat di fase ini)

**Kriteria selesai:** ada angka nyata untuk setiap klaim yang akan masuk paper.

---

## Fase 4 — Kelayakan produk

- [ ] **4.1** Perbaiki atau tentukan lingkup backend (**KI-03**) — satu base URL, bukan
      dua
- [ ] **4.2** Hentikan penyimpanan password plaintext (**KI-08**)
- [ ] **4.3** Pakai identitas pengguna yang sebenarnya untuk riwayat, bukan nama
      hardcoded (**KI-07**)
- [ ] **4.4** Tangani kasus VPS gagal localize dengan jujur — beri tahu pengguna, jangan
      pandu dari posisi tebakan
- [ ] **4.5** Pindahkan `NavigationManager.cs` keluar dari folder `Samples/` (**KI-06**)
- [ ] **4.6** Hapus 11 script mati (**KI-10**) — hati-hati, `DoorbellARPopup` hanya
      *terlihat* mati

---

## Fase 5 — Utang terstruktur

Dikerjakan setelah ada pengujian yang bisa membuktikan perubahan tidak merusak apa pun.

- [ ] **5.1** **Duplikasi nama ruangan** (**KI-02**, **ADR-N002**) — turunkan kategori
      gedung dari satu pemilik sah, bukan tiga daftar manual
- [ ] **5.2** ID ruangan yang stabil, terpisah dari nama tampilan (**ADR-R003**)
- [ ] **5.3** Hapus A* dan `WaypointAutoConnect` yang tidak terpakai, atau pakai
      sungguhan (**KI-09**)

---

## Fase 6 — Serah-terima

- [ ] **6.1** Panduan operasional untuk klien: cara menambah ruangan, cara memindai peta
      baru, siapa dihubungi saat rusak
- [ ] **6.2** Dokumen ketergantungan & biaya: lisensi Immersal, hosting backend, masa
      berlaku
- [ ] **6.3** Rencana keberlanjutan: apa yang terjadi kalau gedung direnovasi dan peta
      tidak cocok lagi
- [ ] **6.4** Build rilis bertanda tangan + catatan rilis

---

## Catatan urutan

Tiga hal yang urutannya sering dibalik dan selalu mahal:

1. **Instrumentasi sebelum uji lapangan.** Mengukur belakangan = mengulang pengujian.
2. **Gerbang lisensi sebelum development.** Membangun di atas akun yang tidak dikuasai
   berisiko membuang seluruh pekerjaan.
3. **Pengujian sebelum refactor.** Itu sebabnya Fase 5 ada di belakang, bukan di depan —
   dan itu alasan **ADR-N002** menunda perbaikan duplikasi nama.
