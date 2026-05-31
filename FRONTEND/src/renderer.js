const app = document.getElementById("app");

const channels = [
    "Ogólny",
    "Programowanie",
    "Memiki",
    "Testowy",
];

const messages = [];

app.innerHTML = `
<div class="app">

  <div class="sidebar">
    <div class="logo">
      <h1>SpeakNow</h1>
      <p>Twój komunikator</p>
    </div>

    <div class="channels">
      ${channels.map(channel => `
        <div class="channel">
          # ${channel}
        </div>
      `).join("")}
    </div>
  </div>

  <div class="chat">

    <div class="chat-header">
      <h2># Ogólny</h2>
    </div>

    <div class="messages"></div>

    <div class="input-area">
      <input id="my-message" placeholder="Napisz wiadomość..." />
      <button id="send">Wyślij</button>
    </div>

  </div>

</div>
`;

const button1 = document.getElementById("send");
const input = document.getElementById("my-message");
// const
const messagesContainer = document.querySelector(".messages");

function renderMyMessages() {

    messagesContainer.innerHTML = "";

    messages.forEach(msg => {

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

    });
}

function renderGuestMessages() {

    messagesContainer.innerHTML = "";

    messages.forEach(msg => {

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

    });
}



function send() {

    const text = input.value;

    if (text.trim() === "")
        return;

    const message = {
        user: "Ty",
        text: text,
        time: new Date().toLocaleTimeString([], {
            hour: "2-digit",
            minute: "2-digit"
        })
    };

    messages.push(message);

    renderMyMessages();

    input.value = "";
}

input.addEventListener("keydown", function (event) {
    if (event.key == "Enter") {
        send();
    }
});

button1.addEventListener("click", send);