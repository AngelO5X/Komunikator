const app = document.getElementById("app");

const API_URL = "http://localhost:5000";

let activeReceiverId = null;
let activeReceiverName = null;

let users = [];
let refreshInterval = null;

let authToken = localStorage.getItem("token");

const messages = [];

let currentUser = null;
let input = null;
let messagesContainer = null;

// START APLIKACJI
function startApp() {
    const token = localStorage.getItem("token");

    if (token) {
        currentUser = {
            id: localStorage.getItem("userId"),
            username: localStorage.getItem("username")
        };

        renderChat();
    } else {
        renderLogin();
    }
}

startApp();

function renderLogin() {
    app.innerHTML = `
        <div class="auth-screen">
            <div class="auth-box">
                <h1>SpeakNow</h1>
                <p>Zaloguj się do komunikatora</p>

                <input id="login-username" placeholder="Nazwa użytkownika" />
                <input id="login-password" type="password" placeholder="Hasło" />

                <button id="login-btn">Zaloguj</button>

                <p class="auth-switch">
                    Nie masz konta?
                    <span id="go-register">Zarejestruj się</span>
                </p>
            </div>
        </div>
    `;

    const loginBtn = document.getElementById("login-btn");
    const goRegister = document.getElementById("go-register");

    loginBtn.addEventListener("click", login);
    goRegister.addEventListener("click", renderRegister);
}

function renderRegister() {
    app.innerHTML = `
        <div class="auth-screen">
            <div class="auth-box">
                <h1>SpeakNow</h1>
                <p>Utwórz nowe konto</p>

                <input id="register-username" placeholder="Nazwa użytkownika" />
                <input id="register-email" placeholder="Email" />
                <input id="register-password" type="password" placeholder="Hasło" />

                <button id="register-btn">Zarejestruj</button>

                <p class="auth-switch">
                    Masz już konto?
                    <span id="go-login">Zaloguj się</span>
                </p>
            </div>
        </div>
    `;

    const registerBtn = document.getElementById("register-btn");
    const goLogin = document.getElementById("go-login");

    registerBtn.addEventListener("click", register);
    goLogin.addEventListener("click", renderLogin);
}

function renderChat() {
    app.innerHTML = `
    <div class="app">

      <div class="sidebar">
        <div class="logo">
          <h1>SpeakNow</h1>
          <p>Twój komunikator</p>
        </div>

        <div class="users-section">
          <p class="section-title">Użytkownicy</p>
          <div class="users-list">
            <p class="loading-users">Ładowanie użytkowników...</p>
          </div>
        </div>

        <div class="sidebar-bottom">
            <p>Zalogowano jako:</p>
            <strong>${currentUser?.username ?? "Użytkownik"}</strong>
            <button id="logout-btn">Wyloguj</button>
        </div>
      </div>

      <div class="chat">

        <div class="chat-header">
          <h2 id="chat-title">Wybierz użytkownika</h2>
        </div>

        <div class="messages"></div>

        <div class="input-area">
          <input id="my-message" placeholder="Najpierw wybierz użytkownika..." disabled />
          <button id="send" disabled>Wyślij</button>
        </div>

      </div>

    </div>
    `;

    const button1 = document.getElementById("send");
    const logoutBtn = document.getElementById("logout-btn");

    input = document.getElementById("my-message");
    messagesContainer = document.querySelector(".messages");

    button1.addEventListener("click", send);

    input.addEventListener("keydown", function (event) {
        if (event.key === "Enter") {
            send();
        }
    });

    logoutBtn.addEventListener("click", logout);

    loadUsers();
}

async function loadUsers() {
    try {
        const response = await fetch(`${API_URL}/api/users`, {
            method: "GET",
            headers: getAuthHeaders()
        });

        if (!response.ok) {
            const error = await response.text();
            console.error("Błąd pobierania użytkowników:", error);
            alert("Nie udało się pobrać użytkowników.");
            return;
        }

        users = await response.json();

        renderUsers();

    } catch (error) {
        console.error("Nie udało się połączyć z backendem:", error);
        alert("Nie udało się połączyć z backendem.");
    }
}

function renderUsers() {
    const usersList = document.querySelector(".users-list");

    if (!usersList) {
        return;
    }

    if (users.length === 0) {
        usersList.innerHTML = `
            <p class="loading-users">Brak innych użytkowników</p>
        `;
        return;
    }

    usersList.innerHTML = users.map(user => `
        <div class="user-item" data-id="${user.userId}" data-name="${user.username}">
            <div class="user-avatar">
                ${user.username.charAt(0).toUpperCase()}
            </div>

            <div>
                <div class="user-name">${user.username}</div>
                <div class="user-status">dostępny</div>
            </div>
        </div>
    `).join("");

    document.querySelectorAll(".user-item").forEach(item => {
        item.addEventListener("click", () => {
            const userId = item.dataset.id;
            const username = item.dataset.name;

            selectUser(userId, username);
        });
    });
}

function selectUser(userId, username) {
    activeReceiverId = userId;
    activeReceiverName = username;

    document.getElementById("chat-title").innerText = `Rozmowa z ${username}`;

    input.disabled = false;
    document.getElementById("send").disabled = false;

    input.placeholder = `Napisz wiadomość do ${username}...`;

    document.querySelectorAll(".user-item").forEach(item => {
        item.classList.remove("active-user");
    });

    const activeItem = document.querySelector(`[data-id="${userId}"]`);

    if (activeItem) {
        activeItem.classList.add("active-user");
    }

    messages.length = 0;
    renderMessages();

    loadConversation();
    startMessagePolling();
}

function startMessagePolling() {
    if (refreshInterval) {
        clearInterval(refreshInterval);
    }

    refreshInterval = setInterval(() => {
        if (activeReceiverId) {
            loadConversation();
        }
    }, 1500);
}

function renderMessages() {
    messagesContainer.innerHTML = "";

    messages.forEach(msg => {
        if (msg.isMine) {
            renderMyMessage(msg);
        } else {
            renderGuestMessage(msg);
        }
    });

}

function renderMyMessage(msg) {
    messagesContainer.innerHTML += `
        <div class="user-message">

            <div class="message-top">
                <div class="message-user">
                    ${msg.user}
                </div>

                <div class="message-time">
                    ${msg.time}
                </div>
            </div>

            <div>${msg.text}</div>

        </div>
    `;
}

function renderGuestMessage(msg) {
    messagesContainer.innerHTML += `
        <div class="guest-message">

            <div class="message-top">
                <div class="message-user">
                    ${msg.user}
                </div>

                <div class="message-time">
                    ${msg.time}
                </div>
            </div>

            <div>${msg.text}</div>

        </div>
    `;
}

async function send() {
    const text = input.value;

    if (!activeReceiverId) {
        alert("Najpierw wybierz użytkownika.");
        return;
    }

    if (text.trim() === "") {
        return;
    }

    const myUserId = localStorage.getItem("userId");

    try {
        const response = await fetch(`${API_URL}/api/messages`, {
            method: "POST",
            headers: getAuthHeaders(),
            body: JSON.stringify({
                senderUUID: myUserId,
                receiverUUID: activeReceiverId,
                content: text
            })
        });

        if (!response.ok) {
            const error = await response.text();
            console.error("Błąd wysyłania wiadomości:", error);
            alert("Nie udało się wysłać wiadomości.");
            return;
        }

        input.value = "";

        await loadConversation();

    } catch (error) {
        console.error("Nie udało się połączyć z backendem:", error);
        alert("Nie udało się połączyć z backendem.");
    }
}

async function login() {
    const usernameOrEmail = document.getElementById("login-username").value;
    const password = document.getElementById("login-password").value;

    if (usernameOrEmail.trim() === "" || password.trim() === "") {
        alert("Podaj login/email i hasło");
        return;
    }

    try {
        const response = await fetch(`${API_URL}/api/auth/login`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                usernameOrEmail: usernameOrEmail,
                password: password
            })
        });

        if (!response.ok) {
            const error = await response.text();
            console.error("Błąd logowania:", error);
            alert("Nieprawidłowy login/email lub hasło.");
            return;
        }

        const data = await response.json();

        console.log("Zalogowano:", data);

        localStorage.setItem("token", data.token);
        localStorage.setItem("userId", data.userId);
        localStorage.setItem("username", data.username);

        authToken = data.token;

        currentUser = {
            id: data.userId,
            username: data.username
        };

        renderChat();

    } catch (error) {
        console.error("Brak połączenia z backendem:", error);
        alert("Nie udało się połączyć z backendem.");
    }
}

async function register() {
    const username = document.getElementById("register-username").value;
    const email = document.getElementById("register-email").value;
    const password = document.getElementById("register-password").value;

    if (
        username.trim() === "" ||
        email.trim() === "" ||
        password.trim() === ""
    ) {
        alert("Uzupełnij wszystkie pola");
        return;
    }

    try {
        const response = await fetch(`${API_URL}/api/auth/register`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                username: username,
                email: email,
                password: password,
                language: "pl"
            })
        });

        if (!response.ok) {
            const error = await response.text();
            console.error("Błąd rejestracji:", error);
            alert("Nie udało się utworzyć konta. Sprawdź dane.");
            return;
        }

        const data = await response.json();

        console.log("Utworzono konto:", data);

        alert("Konto utworzone. Możesz się zalogować.");
        renderLogin();

    } catch (error) {
        console.error("Brak połączenia z backendem:", error);
        alert("Nie udało się połączyć z backendem.");
    }
}

function logout() {
    if (refreshInterval) {
        clearInterval(refreshInterval);
        refreshInterval = null;
    }

    localStorage.removeItem("token");
    localStorage.removeItem("userId");
    localStorage.removeItem("username");

    authToken = null;
    currentUser = null;
    activeReceiverId = null;
    activeReceiverName = null;

    messages.length = 0;

    renderLogin();
}

function getAuthHeaders() {
    return {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${authToken}`
    };
}

async function loadConversation() {
    const myUserId = localStorage.getItem("userId");

    if (!myUserId || !activeReceiverId) {
        return;
    }

    try {
        const response = await fetch(
            `${API_URL}/api/messages/${myUserId}/${activeReceiverId}`,
            {
                method: "GET",
                headers: getAuthHeaders()
            }
        );

        if (!response.ok) {
            const error = await response.text();
            console.error("Błąd pobierania wiadomości:", error);
            return;
        }

        const data = await response.json();

        messages.length = 0;

        data.forEach(msg => {
            const senderId = msg.senderUUID ?? msg.senderUuid;
            const receiverId = msg.receiverUUID ?? msg.receiverUuid;

            const normalizedSenderId = senderId.toLowerCase();
            const normalizedMyUserId = myUserId.toLowerCase();

            messages.push({
                id: msg.messageId,
                senderId: senderId,
                receiverId: receiverId,
                user: normalizedSenderId === normalizedMyUserId ? "Ty" : activeReceiverName,
                text: msg.content,
                time: new Date(msg.createdAt).toLocaleTimeString([], {
                    hour: "2-digit",
                    minute: "2-digit"
                }),
                isMine: normalizedSenderId === normalizedMyUserId
            });
        });

        renderMessages();

    } catch (error) {
        console.error("Nie udało się pobrać rozmowy:", error);
    }
}