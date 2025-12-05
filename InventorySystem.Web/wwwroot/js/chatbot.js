// wwwroot/js/chatbot.js

(function () {

    const apiUrl = "/api/chatbot/message";

    const toggleBtn   = document.getElementById("chatbot-toggle");
    const chatWindow  = document.getElementById("chatbot-window");
    const closeBtn    = document.getElementById("chatbot-close");
    const form        = document.getElementById("chatbot-form");
    const input       = document.getElementById("chatbot-input");
    const messagesBox = document.getElementById("chatbot-messages");

    // Si falta algo del HTML, no hacemos nada
    if (!toggleBtn || !chatWindow || !form || !input || !messagesBox) {
        return;
    }

    // Sesión del chat (para que el backend pueda mantener contexto si quiere)
    let sessionId = window.localStorage.getItem("chatbotSessionId");
    if (!sessionId) {
        sessionId = (crypto.randomUUID ? crypto.randomUUID() : Date.now().toString());
        window.localStorage.setItem("chatbotSessionId", sessionId);
    }

    // Abrir/cerrar ventana
    toggleBtn.addEventListener("click", () => {
        chatWindow.classList.toggle("hidden");
    });

    if (closeBtn) {
        closeBtn.addEventListener("click", () => {
            chatWindow.classList.add("hidden");
        });
    }

    // Enviar mensaje
    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        const text = input.value.trim();
        if (!text) return;

        addMessage("user", text);
        input.value = "";
        input.focus();

        try {
            const payload = {
                sessionId: sessionId,
                text: text
            };

            const res = await fetch(apiUrl, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload)
            });

            if (!res.ok) {
                addMessage("bot", "Lo siento, hubo un error al conectar con el asistente.");
                return;
            }

            const data = await res.json();

            const reply =
                data.answer ||
                data.output ||
                data.message ||
                "Lo siento, no pude obtener una respuesta del asistente.";

            addMessage("bot", reply);

        } catch (err) {
            console.error(err);
            addMessage("bot", "Ocurrió un error de red al contactar al asistente.");
        }
    });

    // Agrega el mensaje a la ventana del chat
    function formatBotText(text) {
        // Escapar HTML básico para evitar problemas
        let safe = text
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;");

        // Separar párrafos por líneas en blanco
        const paragraphs = safe.split(/\n\s*\n+/);

        // Dentro de cada párrafo, los saltos de línea simples -> <br>
        const htmlParagraphs = paragraphs.map(p =>
            p.replace(/\n/g, "<br>")
        );

        // Un <br><br> entre párrafos
        return htmlParagraphs.join("<br><br>");
    }

    function addMessage(from, text) {
        const div = document.createElement("div");
        div.classList.add("chatbot-message");
        div.classList.add(from === "user" ? "user" : "bot");

        if (from === "bot") {
            div.innerHTML = formatBotText(text);
        } else {
            div.textContent = text;
        }

        messagesBox.appendChild(div);
        messagesBox.scrollTop = messagesBox.scrollHeight;
    }


})();
