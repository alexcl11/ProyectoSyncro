// ==========================================
// LÓGICA DEL ASISTENTE IA (CHATBOT)
// ==========================================

// Función para guardar en sessionStorage
function guardarHistorialChat() {
    const container = document.getElementById('chatbot-messages');
    if (container) {
        sessionStorage.setItem('chatbot_history', container.innerHTML);
    }
}

// Al cargar la página, restaurar todo
document.addEventListener("DOMContentLoaded", function () {
    const history = sessionStorage.getItem('chatbot_history');
    if (history) {
        document.getElementById('chatbot-messages').innerHTML = history;
        scrollToBottomChat();
    }

    // Opcional: Reabrir el chat si estaba abierto
    if (sessionStorage.getItem('chatbot_open') === 'true') {
        document.getElementById('chatbot-window').classList.add('active');
    }
});

// 2. Función para cargar el historial al iniciar la página
function cargarHistorialChat() {
    const history = sessionStorage.getItem('chatbot_history');
    if (history) {
        const messagesContainer = document.getElementById('chatbot-messages');
        messagesContainer.innerHTML = history;
        scrollToBottomChat();
    }
}

// 3. Modificar tu función toggleChatbot para recordar si estaba abierto
function toggleChatbot() {
    var window = document.getElementById('chatbot-window');
    window.classList.toggle('active');

    // Guardamos el estado (abierto/cerrado) para que no se esconda al cambiar de tabla
    sessionStorage.setItem('chatbot_open', window.classList.contains('active'));

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

    // --- GUARDAMOS AQUÍ (Mensaje del usuario registrado) ---
    guardarHistorialChat();

    input.value = '';
    sendBtn.disabled = true;
    scrollToBottomChat();

    // 2. Pintamos el indicador de "Escribiendo..."
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

    var formData = new FormData();
    formData.append('prompt', messageText);

    const urlParams = new URLSearchParams(window.location.search);
    const tablaActual = urlParams.get('tabla');

    if (tablaActual) {
        formData.append('tablaActual', tablaActual); 
    }

    fetch('/api/AiIntegration/AskAi', {
        method: 'POST',
        body: formData
    })
        .then(async response => {
            if (!response.ok) {
                var errorMensaje = await response.text();
                throw new Error(errorMensaje);
            }
            return response.json();
        })
        .then(data => {
            document.getElementById(typingId).remove();

            var aiMsgHTML = `<div class="chat-msg ai">${data.respuesta}</div>`;
            messagesContainer.insertAdjacentHTML('beforeend', aiMsgHTML);

            // --- GUARDAMOS AQUÍ (Respuesta de la IA registrada) ---
            guardarHistorialChat();

            scrollToBottomChat();
            sendBtn.disabled = false;

            const mensajeIA = data.respuesta.toLowerCase();

            // 1. Detectamos si la IA nos está informando de un fallo
            const palabrasError = ["error", "fallo", "problema", "no he podido", "lo siento", "no pude"];
            const hayError = palabrasError.some(p => mensajeIA.includes(p));

            // Solo ejecutamos las recargas si NO hay errores
            if (!hayError) {
                // 2. Palabras para cuando crea Tablas/Columnas
                const palabrasEstructura = ["creado", "tabla", "columna", "lista nueva"];
                if (palabrasEstructura.some(p => mensajeIA.includes(p))) {
                    refrescarMenuLateral();
                }

                // 3. Palabras para cuando inserta Datos
                const palabrasDatos = ["insertado", "añadido", "registro", "guardado", "fila"];
                if (palabrasDatos.some(p => mensajeIA.includes(p))) {
                    setTimeout(() => {
                        window.location.reload();
                    }, 1500);
                }
            }

            input.focus();
        })
        .catch(error => {
            // Si el elemento de typing existe aún, lo quitamos
            const tNode = document.getElementById(typingId);
            if (tNode) tNode.remove();

            var errorHTML = `<div class="chat-msg ai" style="color: #dc2626; border-color: #fca5a5; background: #fef2f2;">
                ⚠️ <b>Error:</b> ${error.message}
            </div>`;
            messagesContainer.insertAdjacentHTML('beforeend', errorHTML);

            // --- GUARDAMOS AQUÍ (Incluso el error se guarda para que el usuario sepa qué pasó) ---
            guardarHistorialChat();

            scrollToBottomChat();
            sendBtn.disabled = false;
        });
}
// Función auxiliar para que el chat baje automáticamente al último mensaje
function scrollToBottomChat() {
    var messagesContainer = document.getElementById('chatbot-messages');
    messagesContainer.scrollTop = messagesContainer.scrollHeight;
}

// Función para los botones de sugerencias
function usarSugerencia(texto) {
    var input = document.getElementById('chatbot-input');

    // Rellenamos el input con el texto oculto del botón
    input.value = texto;

    // Simulamos que el usuario le ha dado a Enviar automáticamente
    sendChatbotMessage();
}