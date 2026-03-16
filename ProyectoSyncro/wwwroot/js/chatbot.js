// ==========================================
// LÓGICA DEL ASISTENTE IA (CHATBOT)
// ==========================================

function toggleChatbot() {
    var window = document.getElementById('chatbot-window');
    window.classList.toggle('active');

    // Si lo abrimos, ponemos el foco en el input automáticamente
    if (window.classList.contains('active')) {
        document.getElementById('chatbot-input').focus();
    }
}

// Permitir enviar con la tecla "Enter" (sin Shift)
function handleChatbotKeyPress(event) {
    if (event.key === 'Enter' && !event.shiftKey) {
        event.preventDefault();
        sendChatbotMessage();
    }
}

// Función principal para enviar el mensaje
function sendChatbotMessage() {
    var input = document.getElementById('chatbot-input');
    var messageText = input.value.trim();

    if (messageText === '') return;

    var messagesContainer = document.getElementById('chatbot-messages');
    var sendBtn = document.getElementById('chatbot-send-btn');

    // 1. Pintamos el mensaje del usuario
    var userMsgHTML = `<div class="chat-msg user">${messageText}</div>`;
    messagesContainer.insertAdjacentHTML('beforeend', userMsgHTML);

    // Limpiamos el input y bloqueamos el botón
    input.value = '';
    sendBtn.disabled = true;
    scrollToBottomChat();

    // 2. Pintamos el indicador de "Escribiendo..." de la IA
    var typingId = 'typing-' + Date.now();
    var typingHTML = `
        <div class="chat-msg ai" id="${typingId}">
            <div class="typing-indicator">
                <div class="typing-dot"></div>
                <div class="typing-dot"></div>
                <div class="typing-dot"></div>
            </div>
        </div>`;
    messagesContainer.insertAdjacentHTML('beforeend', typingHTML);
    scrollToBottomChat();

    // 3. AQUÍ ES DONDE LLAMAMOS A TU BACKEND (QUE CONECTARÁ CON n8n)
    var formData = new FormData();
    formData.append('prompt', messageText);

    fetch('/api/AiIntegration/AskAi', {
        method: 'POST',
        body: formData
    })
        .then(async response => {
            if (!response.ok) {
                // 🔥 AQUÍ LEEMOS EL ERROR REAL DE C# 🔥
                var errorMensaje = await response.text();
                throw new Error(errorMensaje);
            }
            return response.json();
        })
        .then(data => {
            document.getElementById(typingId).remove();

            var aiMsgHTML = `<div class="chat-msg ai">${data.respuesta}</div>`;
            messagesContainer.insertAdjacentHTML('beforeend', aiMsgHTML);

            scrollToBottomChat();
            sendBtn.disabled = false;

            // 🔥 TRUCO: Si la respuesta de la IA sugiere que ha creado algo, recargamos
            // Puedes ajustar estas palabras clave según cómo responda tu agente
            const palabrasClave = ["creado", "tabla", "lista", "finalizado", "columna"];
            const mensajeLow = data.respuesta.toLowerCase();

            if (palabrasClave.some(p => mensajeLow.includes(p))) {
                setTimeout(() => {
                    window.location.reload();
                }, 2000); // Esperamos 2 segundos para que el usuario pueda leer el mensaje de éxito
            }

            input.focus();
        })
        .catch(error => {
            document.getElementById(typingId).remove();

            // 🔥 AHORA EL CHAT MOSTRARÁ EL ERROR EXACTO DEL SERVIDOR 🔥
            var errorHTML = `<div class="chat-msg ai" style="color: #dc2626; border-color: #fca5a5; background: #fef2f2;">
            ⚠️ <b>Error:</b> ${error.message}
        </div>`;
            messagesContainer.insertAdjacentHTML('beforeend', errorHTML);

            scrollToBottomChat();
            sendBtn.disabled = false;
        });
}

// Función auxiliar para que el chat baje automáticamente al último mensaje
function scrollToBottomChat() {
    var messagesContainer = document.getElementById('chatbot-messages');
    messagesContainer.scrollTop = messagesContainer.scrollHeight;
}