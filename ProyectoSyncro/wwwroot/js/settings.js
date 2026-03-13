// ==========================================
// NAVEGACIÓN DE PESTAÑAS (TABS)
// ==========================================
function switchTab(button, tabId) {
    document.querySelectorAll('.settings-nav-item').forEach(btn => btn.classList.remove('active'));
    document.querySelectorAll('.settings-card').forEach(card => card.style.display = 'none');

    button.classList.add('active');
    document.getElementById('tab-' + tabId).style.display = 'block';
}

// ==========================================
// PESTAÑA: EMPRESA
// ==========================================
function actualizarTextoEstado(checkbox) {
    const labelTexto = document.getElementById('estado-text');
    if (checkbox.checked) {
        labelTexto.textContent = 'Activa';
        labelTexto.style.color = '#10b981';
    } else {
        labelTexto.textContent = 'Inactiva';
        labelTexto.style.color = '#64748b';
    }
}

function guardarEmpresa(event) {
    event.preventDefault();
    var formData = new FormData(document.getElementById('formEmpresa'));

    Swal.fire({
        title: 'Guardando...',
        heightAuto: false,
        allowOutsideClick: false,
        didOpen: () => { Swal.showLoading(); }
    });

    fetch('/Settings/UpdateEmpresa', { method: 'POST', body: formData })
        .then(response => {
            if (!response.ok) return response.text().then(msg => { throw new Error(msg); });
            return response;
        })
        .then(() => {
            Swal.fire({
                title: '¡Actualizado!',
                text: 'Los datos de la empresa han sido guardados.',
                icon: 'success',
                heightAuto: false,
                timer: 1500,
                showConfirmButton: false
            }).then(() => window.location.reload());
        })
        .catch(error => {
            Swal.fire({
                title: 'Error',
                text: error.message,
                icon: 'error',
                heightAuto: false,
                confirmButtonColor: '#dc2626'
            });
        });
}

// ==========================================
// PESTAÑA: MI PERFIL
// ==========================================
function guardarPerfil(event) {
    event.preventDefault();
    var formData = new FormData(event.target);

    Swal.fire({
        title: 'Actualizando perfil...',
        allowOutsideClick: false,
        heightAuto: false,
        didOpen: () => { Swal.showLoading(); }
    });

    fetch('/Settings/UpdatePerfil', { method: 'POST', body: formData })
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
            }).then(() => window.location.reload());
        })
        .catch(error => {
            Swal.fire({ title: 'Error', text: error.message, icon: 'error', heightAuto: false });
        });
}

// ==========================================
// GESTIÓN DE EQUIPO: CREAR USUARIO
// ==========================================
function openAddUserModal() {
    document.getElementById('formNewUser').reset();
    document.getElementById('check-es-admin').checked = false;
    document.getElementById('newUserModal').classList.add('active');
}

function closeAddUserModal() {
    document.getElementById('newUserModal').classList.remove('active');
}

function actualizarTextoAdmin(checkbox) {
    const labelTexto = document.getElementById('esadmin-text');
    if (checkbox.checked) {
        labelTexto.textContent = 'Activa';
        labelTexto.style.color = '#10b981';
    } else {
        labelTexto.textContent = 'Inactiva';
        labelTexto.style.color = '#64748b';
    }
}

function guardarNuevoUsuario(event) {
    event.preventDefault();
    var formData = new FormData(document.getElementById('formNewUser'));
    var esAdmin = document.getElementById('check-es-admin').checked;
    formData.set('esAdmin', esAdmin);

    Swal.fire({
        title: 'Creando usuario...',
        allowOutsideClick: false,
        heightAuto: false,
        didOpen: () => { Swal.showLoading(); }
    });

    fetch('/Settings/CreateUser', { method: 'POST', body: formData })
        .then(response => {
            if (!response.ok) return response.text().then(msg => { throw new Error(msg); });
            return response;
        })
        .then(() => {
            closeAddUserModal();
            Swal.fire({
                title: '¡Usuario creado!',
                text: 'El nuevo miembro del equipo ha sido añadido.',
                icon: 'success',
                heightAuto: false,
                timer: 1500,
                showConfirmButton: false
            }).then(() => window.location.reload());
        })
        .catch(error => {
            Swal.fire({
                title: 'No se pudo crear',
                text: error.message,
                icon: 'error',
                heightAuto: false,
                confirmButtonColor: '#dc2626'
            });
        });
}

// ==========================================
// GESTIÓN DE EQUIPO: EDITAR USUARIO
// ==========================================
function abrirModalEditarUsuario(btn) {
    document.getElementById('edit-idUsuario').value = btn.getAttribute('data-id');
    document.getElementById('edit-nombre').value = btn.getAttribute('data-nombre');
    document.getElementById('edit-email').value = btn.getAttribute('data-email');
    document.getElementById('edit-admin').checked = btn.getAttribute('data-admin') === 'true';

    document.getElementById('editUserModal').classList.add('active');
}

function cerrarModalEditarUsuario() {
    document.getElementById('editUserModal').classList.remove('active');
}

function guardarEdicionUsuario(event) {
    event.preventDefault();
    var formData = new FormData(document.getElementById('formEditUser'));

    Swal.fire({ title: 'Guardando...', heightAuto: false, didOpen: () => { Swal.showLoading(); } });

    fetch('/Settings/UpdateUsuarioEquipo', { method: 'POST', body: formData })
        .then(response => {
            if (!response.ok) return response.text().then(msg => { throw new Error(msg); });
            return response;
        })
        .then(() => {
            Swal.fire({
                title: '¡Actualizado!',
                icon: 'success',
                heightAuto: false,
                timer: 1500,
                showConfirmButton: false
            }).then(() => window.location.reload());
        })
        .catch(error => Swal.fire({ title: 'Error', text: error.message, icon: 'error', heightAuto: false }));
}

// ==========================================
// GESTIÓN DE EQUIPO: ELIMINAR USUARIO
// ==========================================
function eliminarUsuario(btn) {
    var idUsuario = btn.getAttribute('data-id');
    var nombreUsuario = btn.getAttribute('data-nombre');

    Swal.fire({
        title: '¿Eliminar a ' + nombreUsuario + '?',
        text: "Perderá el acceso a la empresa inmediatamente.",
        icon: 'warning',
        heightAuto: false,
        showCancelButton: true,
        confirmButtonColor: '#dc2626',
        cancelButtonColor: '#64748b',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            Swal.fire({ title: 'Eliminando...', heightAuto: false, didOpen: () => { Swal.showLoading(); } });

            var formData = new FormData();
            formData.append('idUsuario', idUsuario);

            fetch('/Settings/DeleteUsuarioEquipo', { method: 'POST', body: formData })
                .then(response => {
                    if (!response.ok) return response.text().then(msg => { throw new Error(msg); });
                    return response;
                })
                .then(() => {
                    Swal.fire({
                        title: 'Eliminado',
                        text: 'El usuario ha sido eliminado.',
                        icon: 'success',
                        heightAuto: false,
                        timer: 1500,
                        showConfirmButton: false
                    }).then(() => window.location.reload());
                })
                .catch(error => Swal.fire({ title: 'Error', text: error.message, icon: 'error', heightAuto: false }));
        }
    });
}

function confirmarEliminarEmpresa() {
    Swal.fire({
        title: '¿Estás completamente seguro?',
        text: "Se borrarán todos los datos, tablas y usuarios de la empresa. ¡No podrás revertir esto!",
        icon: 'warning',
        heightAuto: false, 
        input: 'text',
        inputPlaceholder: 'Escribe ELIMINAR para confirmar',
        showCancelButton: true,
        confirmButtonColor: '#dc2626',
        cancelButtonColor: '#64748b',
        confirmButtonText: 'Sí, eliminar todo',
        cancelButtonText: 'Cancelar',
        // Validamos que haya escrito la palabra exacta
        preConfirm: (inputValue) => {
            if (inputValue !== 'ELIMINAR') {
                Swal.showValidationMessage('Debes escribir la palabra ELIMINAR en mayúsculas para confirmar.');
                return false;
            }
        }
    }).then((result) => {
        if (result.isConfirmed) {

            Swal.fire({
                title: 'Eliminando empresa...',
                text: 'Por favor, no cierres esta ventana.',
                heightAuto: false, 
                allowOutsideClick: false,
                didOpen: () => { Swal.showLoading(); }
            });

            // Llamamos al backend para destruir los datos
            fetch('/Settings/DeleteEmpresa', { // Ajusta el controlador si no es Settings
                method: 'POST'
            })
                .then(response => {
                    if (!response.ok) throw new Error('Error al eliminar la empresa');
                    return response.json();
                })
                .then(data => {
                    Swal.fire({
                        title: 'Empresa eliminada',
                        text: 'Serás redirigido al inicio de sesión.',
                        heightAuto: false, 
                        icon: 'success',
                        timer: 2000,
                        showConfirmButton: false
                    }).then(() => {
                        window.location.href = data.url; // Redirigimos al Login
                    });
                })
                .catch(error => {
                    Swal.fire('Error', error.message, 'error');
                });
        }
    });
}

// ==========================================
// CONTROL DE LÍMITES: CREAR USUARIO
// ==========================================
function intentarCrearUsuario(cantidadUsuarios, esPremium) {

    // Si NO es premium y ya hay 3 o más usuarios en el equipo, ¡Bloqueo!
    if (!esPremium && cantidadUsuarios >= 3) {
        Swal.fire({
            icon: 'warning',
            title: 'Límite de Equipo Alcanzado',
            heightAuto: false,
            html: 'Tu plan Free solo permite un máximo de <b>3 usuarios</b>.<br><br>Mejora tu cuenta a Premium para invitar a todos tus empleados sin restricciones.',
            confirmButtonText: 'Desbloquear Premium',
            confirmButtonColor: '#3b82f6',
            showCancelButton: true,
            cancelButtonText: 'Ahora no',
            cancelButtonColor: '#64748b'
        }).then((result) => {
            if (result.isConfirmed) {
                // Le mandamos a la pasarela de pago de Stripe
                window.location.href = '/Payment/Checkout';
            }
        });
    }
    else {
        // Si tiene menos de 3 usuarios O ya ha pagado, abrimos el modal normal
        openAddUserModal();
    }
}