# GEMINI.md — DarsiNavigasi (Unity Repo)

File ini adalah cermin dari [`AGENTS.md`](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/AGENTS.md) dan [`CLAUDE.md`](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/CLAUDE.md) untuk memastikan kepatuhan aturan oleh Gemini / Antigravity Agent.

Silakan merujuk ke [`AGENTS.md`](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/AGENTS.md) atau dokumen utama berikut sebelum melakukan perubahan apa pun:
- [`docs/ARCHITECTURE.md`](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/docs/ARCHITECTURE.md)
- [`docs/FLOWS.md`](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/docs/FLOWS.md)
- [`docs/DECISIONS.md`](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/docs/DECISIONS.md)
- [`docs/KNOWN-ISSUES.md`](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/docs/KNOWN-ISSUES.md)
- [`docs/ROADMAP.md`](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/docs/ROADMAP.md)

---

## Ringkasan Aturan Kunci

1. **Jangan Pernah Re-import Immersal SDK Core Samples**: Otak utama navigasi ([`NavigationManager.cs`](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/Assets/Samples/Immersal%20SDK/2.2.1/Core%20Samples/Scripts/Navigation/NavigationManager.cs)) berada di folder Samples SDK dan telah dimodifikasi secara berat.
2. **Jangan Rename GameObject Ruangan Tanpa Perencanaan**: Nama GameObject di scene adalah ID ruangan (`targetName` kosong).
3. **Minta Persetujuan Sebelum Mengubah Scene Utama**: Scene [`Assets/Scenes/6_AR Navigasi.unity`](file:///D:/Dev/Projects/UnityProjects/DarsiNavigasi0.1/Assets/Scenes/6_AR%20Navigasi.unity) (60 MB) sangat sensitif dan rawan diff raksasa.
4. **NavMesh adalah Sistem Pathfinding Nyata**: Algoritma A* pada `NavigationGraphManager.cs` adalah kode mati.
5. **Standar Kejujuran Academic**: Sebutkan file:baris pada setiap klaim faktual dan tandai `[belum terverifikasi]` jika belum diuji di perangkat fisik.
6. **Dilarang Push & Dilarang Masuk Collaborator**: AI Agent **DILARANG KERAS** melakukan `git push` ke remote repo dan **DILARANG** mencantumkan nama/identitas AI sebagai author, committer, atau collaborator pada git history/PR.
7. **Commit Membutuhkan Consent**: Action `git commit` HANYA boleh dilakukan dengan persetujuan (*consent*) eksplisit dari pemilik projek (Bagus).

