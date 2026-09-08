# 📱 Dokumentasi REST API - BaknusITCare (Untuk Pengembang Flutter)

Dokumentasi ini dibuat sebagai panduan teknis bagi pengembang aplikasi mobile (**Flutter Android & iOS**) untuk berintegrasi dengan sistem backend **BaknusITCare SMK Bakti Nusantara 666**.

---

## 🌐 Informasi Umum
* **Base URL**: `https://baknusitcare.smkbn666.sch.id/api`
* **Content-Type**: `application/json`
* **CORS**: Diizinkan untuk semua origin (`AllowAll`) sehingga dapat diakses langsung dari Flutter Web, Emulator, maupun Real Device.

---

## 🔑 1. Autentikasi (`/api/auth`)

### 1.1. Login Pengguna
* **Endpoint**: `POST /api/auth/login`
* **Deskripsi**: Autentikasi email dan password Mailcow sekolah.
* **Request Body**:
  ```json
  {
    "email": "frian_p@smk.baktinusantara666.sch.id",
    "password": "passwordMailcow"
  }
  ```
* **Response (200 OK)**:
  ```json
  {
    "success": true,
    "message": "Login berhasil.",
    "user": {
      "id": "7697dd5c-e5ff-4ab3-bf58-963dbab3cf32",
      "email": "frian_p@smk.baktinusantara666.sch.id",
      "fullName": "Frian P",
      "role": "Teknisi", // "Pelapor" | "Teknisi" | "Admin"
      "department": "Guru",
      "isAdmin": false,
      "isTechnician": true
    }
  }
  ```

---

## 📋 2. Metadata Formulir (`/api/meta`)

### 2.1. Ambil Opsi Formulir
* **Endpoint**: `GET /api/meta/options`
* **Deskripsi**: Mengambil 19 opsi ruangan, jenis layanan, kendala internet, dan daftar aplikasi BaknusID secara dinamis.
* **Response (200 OK)**:
  ```json
  {
    "success": true,
    "message": "Daftar opsi formulir berhasil diambil.",
    "data": {
      "locations": [
        "1. Ruang Workshop",
        "2. Ruang Kepsek",
        "3. Ruang Bahagian",
        "4. Wifi lantai 1",
        "5. Ruang BK, Ruang Guru",
        "6. Wifi lantai 2 Timur (deket lab AKT) . Wifi lantai 2 Barat",
        "7. Wifi lantai 2 Selatan",
        "8. Lab Akuntansi",
        "9. Ruang TU Keuangan",
        "10. Ruang Perpustakaan",
        "11. Lab Animasi",
        "12. Lab DKV",
        "13. Ruang TU",
        "14. Lab PK lantai 1",
        "15. Lab PK lantai 2",
        "16. Lab PK Lantai 3",
        "17. Ruang Photografi",
        "18. Lab Pemasaran",
        "19. Lainnya"
      ],
      "services": ["Internet", "BaknusID"],
      "internetSubIssues": [
        "Internet tidak terkoneksi",
        "Wi-Fi / LAN tidak nyala",
        "Sinyal Wi-Fi Lemah / RTO"
      ],
      "baknusIdApps": [
        "WebsiteBaknus",
        "Baknusmail",
        "BaknusAttend",
        "BaknusDrive",
        "BaknusClass"
      ],
      "priorities": ["Rendah", "Sedang", "Tinggi", "Darurat"],
      "statuses": ["Baru", "Diproses", "MenungguSparepart", "Selesai", "Ditutup"]
    }
  }
  ```

---

## 👨‍💻 3. Petugas IT & Teknisi Terpilih (`/api/technicians`)

### 3.1. Ambil Daftar Petugas IT yang Ditunjuk Admin
* **Endpoint**: `GET /api/technicians`
* **Query Parameters**:
  - `includeAdmins` (opsional, boolean, default: `true`): Menyertakan Administrator IT.
* **Deskripsi**: Mengambil daftar seluruh staf Guru/TU yang telah ditunjuk dan diberi wewenang oleh Admin sebagai Anggota Tim IT (Teknisi) untuk menangani keluhan IT, lengkap dengan statistik tiket yang sedang/telah ditangani.
* **Response (200 OK)**:
  ```json
  {
    "success": true,
    "message": "Ditemukan 3 Petugas IT yang telah ditunjuk.",
    "data": [
      {
        "id": "7697dd5c-e5ff-4ab3-bf58-963dbab3cf32",
        "fullName": "Frian P",
        "email": "frian_p@smk.baktinusantara666.sch.id",
        "phoneNumber": "08123456789",
        "role": "Teknisi",
        "roleLabel": "Petugas IT (Teknisi)",
        "department": "Guru",
        "activeTicketsHandled": 2,
        "totalResolvedTickets": 14
      },
      {
        "id": "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d",
        "fullName": "Admin IT Support",
        "email": "admin@smk.baktinusantara666.sch.id",
        "phoneNumber": "-",
        "role": "Admin",
        "roleLabel": "Administrator IT",
        "department": "Admin",
        "activeTicketsHandled": 1,
        "totalResolvedTickets": 28
      }
    ]
  }
  ```

---

## 🎫 4. Manajemen Tiket (`/api/tickets`)

### 3.1. Ambil Daftar Tiket
* **Endpoint**: `GET /api/tickets`
* **Query Parameters**:
  - `userId` (opsional): ID pengguna pelapor (jika login sebagai pelapor).
  - `role` (opsional): `Pelapor` / `Teknisi` / `Admin`. (Jika `Teknisi`/`Admin`, sistem mengembalikan seluruh tiket).
  - `status` (opsional): Filter status (`Baru`, `Diproses`, `Selesai`).
  - `search` (opsional): Kata kunci pencarian judul atau nama.
* **Response (200 OK)**:
  ```json
  {
    "success": true,
    "message": "Ditemukan 2 tiket.",
    "data": [
      {
        "id": 10,
        "ticketCode": "NET-101",
        "title": "[Internet] Wi-Fi / LAN tidak nyala di 1. Ruang Workshop",
        "category": "Jaringan & Internet",
        "location": "1. Ruang Workshop",
        "priority": "Sedang",
        "status": "Baru",
        "requesterName": "Budi Santoso",
        "requesterEmail": "budi@smk.baktinusantara666.sch.id",
        "assignedTechnicianName": "Belum Ditugaskan",
        "createdAt": "2026-09-08T10:00:00Z",
        "dueDate": "2026-09-08T12:00:00Z",
        "isOverdue": false,
        "commentsCount": 3,
        "rating": null
      }
    ]
  }
  ```

---

### 3.2. Buat Tiket Baru
* **Endpoint**: `POST /api/tickets`
* **Catatan**: Sistem otomatis menyiarkan notifikasi email ke Petugas IT & mengirim konfirmasi ke pelapor.
* **Request Body (Kendala Internet)**:
  ```json
  {
    "serviceType": "Internet",
    "subIssue": "Wi-Fi / LAN tidak nyala",
    "location": "1. Ruang Workshop",
    "description": "Lampu indikator router mati dan kabel LAN terlepas.",
    "priority": "Sedang",
    "requesterId": "7697dd5c-e5ff-4ab3-bf58-963dbab3cf32",
    "requesterName": "Frian P",
    "requesterEmail": "frian_p@smk.baktinusantara666.sch.id"
  }
  ```
* **Request Body (Kendala Layanan BaknusID)**:
  ```json
  {
    "serviceType": "BaknusID",
    "baknusIdApp": "Baknusmail",
    "baknusIdNote": "Tidak bisa login pesan 'Invalid Password'",
    "priority": "Sedang",
    "requesterId": "7697dd5c-e5ff-4ab3-bf58-963dbab3cf32",
    "requesterName": "Frian P",
    "requesterEmail": "frian_p@smk.baktinusantara666.sch.id"
  }
  ```
* **Response (201 Created)**:
  ```json
  {
    "success": true,
    "message": "Tiket #NET-102 berhasil dibuat dan disiarkan ke Petugas IT.",
    "data": {
      "id": 11,
      "ticketCode": "NET-102",
      "title": "[Internet] Wi-Fi / LAN tidak nyala di 1. Ruang Workshop",
      "status": "Baru",
      "dueDate": "2026-09-08T14:00:00Z",
      "createdAt": "2026-09-08T12:00:00Z"
    }
  }
  ```

---

### 3.3. Ambil Detail Tiket
* **Endpoint**: `GET /api/tickets/{id}`
* **Response (200 OK)**: Mengembalikan rincian lengkap termasuk data pelapor, teknisi penangan, SLA, dan daftar komentar percakapan.

---

### 3.4. Pelacakan Cepat Berdasarkan Nomor Tiket (Publik)
* **Endpoint**: `GET /api/tickets/track/{ticketCode}`
* **Contoh**: `GET /api/tickets/track/NET-101`
* **Deskripsi**: Pelacakan cepat tanpa perlu login.

---

## 💬 4. Tanya Jawab & Diskusi Tiket

### 4.1. Ambil Komentar Tiket
* **Endpoint**: `GET /api/tickets/{id}/comments`
* **Response (200 OK)**:
  ```json
  {
    "success": true,
    "data": [
      {
        "id": 1,
        "userId": "guid-user",
        "userName": "Frian P (Teknisi)",
        "userRole": "Teknisi",
        "message": "Teknisi sedang meluncur ke Ruang Workshop.",
        "isInternal": false,
        "createdAt": "2026-09-08T12:15:00Z"
      }
    ]
  }
  ```

### 4.2. Kirim Komentar / Pertanyaan Baru
* **Endpoint**: `POST /api/tickets/{id}/comments`
* **Request Body**:
  ```json
  {
    "userId": "guid-user-123",
    "userName": "Budi Santoso",
    "userRole": "Pelapor",
    "message": "Terima kasih Pak, kami tunggu di lokasi.",
    "isInternal": false
  }
  ```

---

## 🛠️ 5. Fitur Khusus Petugas IT (Teknisi)

### 5.1. Update Status Tiket & Catatan Penyelesaian
* **Endpoint**: `POST /api/tickets/{id}/status`
* **Deskripsi**: Memperbarui status penanganan tiket (`Diproses`, `Selesai`, `Ditutup`). Otomatis mengirim email update ke pelapor.
* **Request Body**:
  ```json
  {
    "newStatus": "Selesai",
    "updatedByUserId": "guid-teknisi-123",
    "updatedByName": "Frian P",
    "userRole": "Teknisi",
    "comment": "Kabel LAN dan access point ruangan telah diganti baru. Koneksi internet kini normal kembali."
  }
  ```

### 5.2. Tugaskan Teknisi
* **Endpoint**: `POST /api/tickets/{id}/assign`
* **Request Body**:
  ```json
  {
    "technicianId": "guid-teknisi-123",
    "technicianName": "Frian P",
    "assignedByUserId": "guid-admin-123"
  }
  ```

---

## 📊 6. Statistik Dashboard Mobile
* **Endpoint**: `GET /api/tickets/dashboard?role=Teknisi`
* **Response (200 OK)**:
  ```json
  {
    "success": true,
    "data": {
      "totalTickets": 25,
      "newTickets": 3,
      "inProgressTickets": 4,
      "pendingPartsTickets": 1,
      "resolvedTickets": 17,
      "overdueSlaTickets": 0,
      "averageRating": 4.8
    }
  }
  ```
