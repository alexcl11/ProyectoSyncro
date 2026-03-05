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
        heightAuto: false,
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
                heightAuto: false,
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
        heightAuto: false,
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
                heightAuto: false,
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

// --- FUNCIONES DEL MODAL DE NUEVO USUARIO ---

// 1. Abrir Modal
function openAddUserModal() {
    // Reseteamos el formulario al abrir
    document.getElementById('formNewUser').reset();
    document.getElementById('check-es-admin').checked = false; // Por defecto No es admin

    document.getElementById('newUserModal').classList.add('active');
}

// 2. Cerrar Modal
function closeAddUserModal() {
    document.getElementById('newUserModal').classList.remove('active');
}

// 3. Efecto visual (Opcional, si quieres cambiar algún texto al activar el Switch)
function actualizarTextoAdmin(checkbox) {
    const labelTexto = document.getElementById('esadmin-text');

    if (checkbox.checked) {
        labelTexto.textContent = 'Activa';
        labelTexto.style.color = '#10b981'; // Verde
    } else {
        labelTexto.textContent = 'Inactiva';
        labelTexto.style.color = '#64748b'; // Gris
    }
}

// 4. Enviar Datos por AJAX a C#
function guardarNuevoUsuario(event) {
    event.preventDefault();

    var form = document.getElementById('formNewUser');
    var formData = new FormData(form);

    // Como el checkbox solo manda valor si está "on", le forzamos el Booleano para C#
    var esAdmin = document.getElementById('check-es-admin').checked;
    formData.set('esAdmin', esAdmin); // Sobrescribimos con true/false

    // Mostramos estado de carga
    Swal.fire({
        title: 'Creando usuario...',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    // Enviamos al Controlador (Asegúrate de que la ruta apunta a tu controlador correcto)
    fetch('/Settings/CreateUser', {
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
            closeAddUserModal(); // Cerramos el modal primero

            Swal.fire({
                title: '¡Usuario creado!',
                text: 'El nuevo miembro del equipo ha sido añadido.',
                icon: 'success',
                timer: 1500,
                showConfirmButton: false
            }).then(() => {
                window.location.reload(); // Recargamos para ver al usuario en la tabla
            });
        })
        .catch(error => {
            Swal.fire({
                title: 'No se pudo crear',
                text: error.message,
                icon: 'error',
                confirmButtonText: 'Entendido',
                confirmButtonColor: '#dc2626'
            });
        });
}