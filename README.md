🛡️ SecurAI Gateway v2.1
Tugas Keamanan Pemrograman – III Rekayasa Perangkat Lunak Kripto

SecurAI Gateway adalah sebuah proxy gateway yang dibuat untuk mengamankan komunikasi antara pengguna dan AI inference engine.
Project ini berfokus pada penerapan konsep keamanan pemrograman, khususnya pada manajemen identitas, pembatasan akses, dan perlindungan infrastruktur backend pada sistem berbasis AI.

🎯 Tujuan Project
Project ini dibuat sebagai implementasi konsep keamanan pemrograman pada sistem AI, dengan tujuan untuk:
1. Mengamankan akses pengguna menggunakan autentikasi berbasis token
2. Mencegah penyalahgunaan layanan AI
3. Memisahkan client dan AI engine agar sistem lebih aman

🚀 Fitur Utama
1. Stateless Identity Management
Autentikasi menggunakan JWT (JSON Web Token) 256-bit sehingga tidak memerlukan session di server dan tetap aman.
2. Infrastructure Protection
Diterapkan rate limiting dengan batas 3 request per 10 detik untuk mencegah spam dan serangan DoS.
3. Secure Proxy Layer
Client tidak berkomunikasi langsung dengan Ollama (LLM). Semua request AI diproses melalui backend API sebagai lapisan keamanan tambahan.
4. Modern User Interface
UI dibuat responsif menggunakan Tailwind CSS dan mendukung Markdown rendering agar output AI seperti tabel dan list lebih mudah dibaca.
5. Credential Visibility
Tersedia fitur toggle password pada halaman login untuk meningkatkan kenyamanan pengguna.

🛠️ Teknologi yang Digunakan
1. Backend: .NET 10 (C#)
2. Frontend: Razor Pages, Tailwind CSS
3. Keamanan: JWT Bearer Authentication, Fixed Window Rate Limiter
4. AI Engine: Ollama (Model: Llama3)

📦 Cara Menjalankan Project
1. Prasyarat
Pastikan sudah:
Menginstal Ollama dan menjalankan model dengan perintah:
ollama run llama3
Menginstal .NET 10 SDK

2. Menjalankan Backend API
cd SecurAI.API
dotnet run

Setelah backend berjalan, aplikasi siap digunakan melalui browser.

📁 Struktur Singkat Project
SecurAI.API   -> Backend, security layer, dan proxy ke AI
SecurAI.Web   -> Frontend dan user interface