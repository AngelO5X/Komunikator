# SpeakNow
-------------------------------

# 🛡️ Distributed Messenger System (DMS)

System rozproszonej komunikacji opartej na architekturze **CWC-PIK**, zoptymalizowany dla platform x86 jak i ARM64 i zapewniający pełną prywatność dzięki Szyfrowaniu End-to-End (E2EE).

## 🏗️ Architektura Systemu

System składa się z dwóch kluczowych komponentów:
1.  **Centralny Węzeł Certyfikujący (CWC):** Odpowiada za zarządzanie tożsamością, rejestrację użytkowników i autoryzację instancji.
2.  **Prywatna Instancja Komunikacyjna (PIK):** Lekki węzeł (relay) służący do przesyłania wiadomości i przechowywania zaszyfrowanej historii.



---

## 🚀 Kluczowe Funkcjonalności (WF)

### WF-01: Tożsamość i Autoryzacja
* **Centralna Rejestracja:** Konta tworzone są wyłącznie w zaufanym węźle CWC.
* **Weryfikacja Instancji:** Każdy PIK musi posiadać certyfikat wydany przez CWC.
* **Logowanie Hybrydowe:** Użytkownik uwierzytelnia się w CWC, otrzymując token JWT akceptowany przez instancje prywatne.

### WF-02: Komunikacja i E2EE
* **SignalR WebSockets:** Komunikacja w czasie rzeczywistym.
* **Model Kryptograficzny (1+2):**
    * **Podpis:** Klucz prywatny nadawcy (Autentyczność).
    * **Szyfrowanie:** Klucz publiczny odbiorcy (Poufność).
* **Persystencja:** Przechowywanie danych w SQLite/PostgreSQL (Zero Knowledge).

### WF-03: Media
* **Strumieniowanie:** Asynchroniczny transfer plików przez .NET Streams.
* **WebRTC:** Bezpośrednie połączenia głosowe i wideo (P2P).

---

## ⚙️ Specyfikacja Techniczna (WNF)

### Środowisko i Wydajność
* **Native x86:** Wsparcie jak najszersze gammy serwerów i maszyn w celu ułatwienia hostowania serwerów.
* **Native ARM64:** Optymalizacja pod Apple Silicon, AWS Graviton oraz Raspberry Pi.
* **Docker Ready:** Wdrożenie za pomocą `docker-compose`.
* **Low Resource:** Instancja PIK wymaga docelowo jedynie **512MB RAM**.

### Bezpieczeństwo
* **Protokół:** TLS 1.3 dla wszystkich połączeń.
* **Web Crypto API:** Generowanie kluczy wyłącznie po stronie klienta.
* **Store and Forward:** Kolejkowanie zaszyfrowanych wiadomości dla użytkowników offline.

---

## 🔐 Model Kryptograficzny (1+2)

Każda wiadomość w systemie musi spełniać poniższy standard matematyczny w celu zapewnienia integralności:
<img width="636" height="157" alt="Zrzut ekranu 2026-03-09 160628" src="https://github.com/user-attachments/assets/3dade03a-59c7-4ca9-a084-6bb04bae494e" />



