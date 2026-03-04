// ==========================================
// NAVEGACIÓN DE PESTAÑAS (TABS)
// ==========================================
function switchTab(button, tabId) {
    // 1. Desactivar todos los botones y ocultar tarjetas
    document.querySelectorAll('.settings-nav-item').forEach(btn => btn.classList.remove('active'));
    document.querySelectorAll('.settings-card').forEach(card => card.style.display = 'none');

    // 2. Activar el botón pulsado y mostrar su tarjeta
    button.classList.add('active');
    document.getElementById('tab-' + tabId).style.display = 'block';
}

// ==========================================
// PESTAÑA: EMPRESA
// ==========================================

// Cambia visualmente el texto y color del interruptor
function actualizarTextoEstado(checkbox) {
    const labelTexto = document.getElementById('estado-text');

    if (checkbox.checked) {
        labelTexto.textContent = 'Activa';
        labelTexto.style.color = '#10b981'; // Verde
    } else {
        labelTexto.textContent = 'Inactiva';
        labelTexto.style.color = '#64748b'; // Gris
    }
}

// Guarda los datos de la empresa por AJAX
function guardarEmpresa(event) {
    event.preventDefault(); // Evita recargar la página

    var form = document.getElementById('formEmpresa');
    var formData = new FormData(form);

    Swal.fire({
        title: 'Guardando...',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    fetch('/Settings/UpdateEmpresa', {
        method: 'POST',
        body: formData
    })
        .then(response => {
            if (!response.ok) {
                return response.text().then(msg => { throw new Error(msg); });
            }
            return response;
        })
        .then(() => {
            Swal.fire({
                title: '¡Actualizado!',
                text: 'Los datos de la empresa han sido guardados.',
                icon: 'success',
                timer: 1500,
                showConfirmButton: false
            }).then(() => {
                // Recargamos la página. Al recargar, el _Layout leerá la nueva sesión
                // y tu nombre abajo a la izquierda habrá cambiado mágicamente.
                window.location.reload();
            });;
        })
        .catch(error => {
            Swal.fire({
                title: 'Error',
                text: error.message,
                icon: 'error',
                confirmButtonColor: '#dc2626'
            });
        });
}
// ==========================================
// PESTAÑA: MI PERFIL
// ==========================================
function guardarPerfil(event) {
    event.preventDefault();

    var form = event.target; // Capturamos el formulario que lanzó el evento
    var formData = new FormData(form);

    Swal.fire({
        title: 'Actualizando perfil...',
        allowOutsideClick: false,
        didOpen: () => { Swal.showLoading(); }
    });

    fetch('/Settings/UpdatePerfil', {
        method: 'POST',
        body: formData
    })
        .then(response => {
            if (!response.ok) throw new Error("Error al actualizar");
            return response;
        })
        .then(() => {
            Swal.fire({
                title: '¡Perfil actualizado!',
                icon: 'success',
                timer: 1500,
                showConfirmButton: false
            }).then(() => {
                // Recargamos la página. Al recargar, el _Layout leerá la nueva sesión
                // y tu nombre abajo a la izquierda habrá cambiado mágicamente.
                window.location.reload();
            });
        })
        .catch(error => {
            Swal.fire('Error', error.message, 'error');
        });
}