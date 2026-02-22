// ABRIR MODAL DE NUEVA TABLA
function openModal() {
    document.getElementById('newTableModal').classList.add('active');
}

// CERRAR MODAL DE NUEVA TABLA
function closeModal() {
    document.getElementById('newTableModal').classList.remove('active');
}

// CERRAR MODAL DE AÑADIR REGISTRO
function openRecordModal() {
    document.getElementById('newRecordModal').classList.add('active');
}

// CERRAR MODAL DE AÑADIR REGISTRO
function closeRecordModal() {
    document.getElementById('newRecordModal').classList.remove('active');
}

// MOSTRAR MODAL DE CREAR COLUMNA
function openColumnModal() {
    document.getElementById('newColumnModal').classList.add('active');
}

// OCULTAR MODAL DE CREAR COLUMNA
function closeColumnModal() {
    document.getElementById('newColumnModal').classList.remove('active');
}

// FUNCION PARA MOSTRAR LOS INPUTS DE LA COLUMNA DE TIPO 'Relacion' o 'Select'
function toggleCamposDinamicos() {
    var tipo = document.getElementById('selectTipoDato').value;

    document.getElementById('divRelacion').style.display = (tipo === 'Relacion') ? 'block' : 'none';
    document.getElementById('divOpcionesSelect').style.display = (tipo === 'Select') ? 'block' : 'none';
}

// FUNCION PARA AGREGAR OPCIONES AL CREAR COLUMNAS TIPO SELECT
function agregarOpcion() {
    var container = document.getElementById('opcionesContainer');
    var row = document.createElement('div');
    row.className = 'opcion-row';
    row.style = 'display: flex; gap: 8px; margin-bottom: 8px;';

    // COLORES PREDETERMINADOS PARA QUE SALGAN DE EJEMPLO, LUEGO EL USER LOS CAMBIA POR EL QUE QUIERA
    var colores = ['#ef4444', '#f97316', '#eab308', '#22c55e', '#3b82f6', '#a855f7', '#ec4899'];
    var colorRandom = colores[Math.floor(Math.random() * colores.length)];

    // SE AÑADE AL FORM DE CREAR COLUMNA
    row.innerHTML = `
            <input type="text" name="opcionesValor" class="form-control" placeholder="Nueva opción..." style="flex: 1; margin: 0;">
            <input type="color" name="opcionesColor" class="form-control" value="${colorRandom}" style="width: 50px; padding: 2px; height: 38px; cursor: pointer;">
            <button type="button" class="btn btn-outline" onclick="eliminarOpcion(this)" style="padding: 8px 12px; margin: 0; color: #dc2626;">✕</button>
        `;
    container.appendChild(row);
}

function eliminarOpcion(btn) {
    btn.parentElement.remove();
}

// ESTA FUNCION SIRVE PARA MODIFICAR LOS REGISTROS DE LAS COLUMNAS BOOLEAN
// SE VEN CON UNA CASILLA Y AL MARCARLAS, PARA MODIFICARLAS AL MOMENTO, HACE UN 
// FETCH AL controller: Dashboard y action: UpdateCelda Y DE ESTA MANERA SE ACTUALIZA

function actualizarSiNo(tabla, idFila, columna, estaMarcado) {
    var valorSql = estaMarcado ? "1" : "0";

    // EN ESTA PARTE DEL CODIGO CREO UN FORMULARIO TEMPORAL PARA ENVIAR LOS
    // DATOS MEDIANTE POST
    var formData = new FormData();
    formData.append('nombreTabla', tabla);
    formData.append('idFila', idFila);
    formData.append('columna', columna);
    formData.append('valor', valorSql);

    fetch('/Dashboard/UpdateCelda', {
        method: 'POST',
        body: formData
    })
        .then(response => {
            if (!response.ok) {
                alert("Error al guardar el cambio.");
            }
        })
        .catch(error => console.error('Error:', error));
}

// FUNCION PARA EVALUAR SI HAY QUE MOSTRAR LA PAPELERA GLOBAL
function mostrarPapeleraGlobal() {
    var btnGlobal = document.getElementById('btn-delete-all');
    var seleccionados = document.querySelectorAll('.checkbox-fila:checked');

    if (seleccionados.length > 1) {
        btnGlobal.style.display = 'flex';
    } else {
        btnGlobal.style.display = 'none';
    }
}

// FUNCION MARCAR TODOS LOS CHECKBOXES
function selectAllCheckBoxes() {
    var isChecked = document.getElementById('select-all-checkbox').checked;
    var checkboxesFilas = document.querySelectorAll('.checkbox-fila');
    checkboxesFilas.forEach(function (chk) {
        chk.checked = isChecked;

        var idFila = chk.value;
        mostrarBotonDelete(chk, idFila);
    });
    mostrarPapeleraGlobal();
}

// FUNCION MOSTRAR BOTON DELETE
function mostrarBotonDelete(checkbox, id){
    var btn = document.getElementById('btn-delete-' + id);
    if (checkbox.checked) {
        btn.style.display = 'flex'; 
    } else {
        btn.style.display = 'none';
    }
    mostrarPapeleraGlobal();
}

// FUNCION PARA ELIMINAR LOS REGISTROS SELECCIONADOS
function getFilasABorrar(nombreTabla) {
    var checkboxesSeleccionados = document.querySelectorAll('.checkbox-fila:checked');
    var idsPendientesDeBorrar = [];

    checkboxesSeleccionados.forEach(function (chk) {
        if (chk.value && chk.value !== "0") {
            idsPendientesDeBorrar.push(chk.value);
        }
    });

    if (idsPendientesDeBorrar.length === 0) return;

    Swal.fire({
        title: '¿Eliminar registros?',
        text: `Vas a eliminar ${idsPendientesDeBorrar.length} registro(s). Esta acción no se puede deshacer.`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc2626', 
        cancelButtonColor: '#64748b', 
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar',
        showLoaderOnConfirm: true, 
        preConfirm: () => {
            var formData = new FormData();
            formData.append('nombreTabla', nombreTabla);
            idsPendientesDeBorrar.forEach(id => formData.append('idsFilas', id));

            return fetch('/Dashboard/DeleteRegistros', {
                method: 'POST',
                body: formData
            })
                .then(response => {
                    if (!response.ok) {
                        return response.text().then(mensajeError => {
                            throw new Error(mensajeError);
                        });
                    }
                    return response;
                })
                .catch(error => {
                    Swal.showValidationMessage(`${error.message}`);
                });
        },
        allowOutsideClick: () => !Swal.isLoading()
    }).then((result) => {
        // Si C# respondió con un OK y terminó de cargar
        if (result.isConfirmed) {
            Swal.fire({
                title: '¡Eliminado!',
                text: 'Los registros han sido borrados.',
                icon: 'success',
                timer: 1500, // Se cierra solo en 1.5 segundos
                showConfirmButton: false
            }).then(() => {
                // Recargamos la página para que la tabla se actualice
                window.location.reload();
            });
        }
    });
}

// FUNCION PARA ELIMINAR UN REGISTRO
function deleteRegistro(nombreTabla, idFila) {
    
    Swal.fire({
        title: '¿Eliminar registros?',
        text: `Vas a eliminar un registro. Esta acción no se puede deshacer.`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc2626',
        cancelButtonColor: '#64748b',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar',
        showLoaderOnConfirm: true,
        preConfirm: () => {

            var formData = new FormData();
            formData.append('nombreTabla', nombreTabla);
            formData.append('idsFilas', idFila);


            return fetch('/Dashboard/DeleteRegistros', {
                method: 'POST',
                body: formData
            })
            then(response => {
                if (!response.ok) {
                    return response.text().then(mensajeError => {
                        throw new Error(mensajeError);
                    });
                }
                return response;
            })
                .catch(error => {
                    Swal.showValidationMessage(`${error.message}`);
                });
        },
        allowOutsideClick: () => !Swal.isLoading()
    }).then((result) => {
        // Si C# respondió con un OK y terminó de cargar
        if (result.isConfirmed) {
            Swal.fire({
                title: '¡Eliminado!',
                text: 'Los registros han sido borrados.',
                icon: 'success',
                timer: 1500, // Se cierra solo en 1.5 segundos
                showConfirmButton: false
            }).then(() => {
                // Recargamos la página para que la tabla se actualice
                window.location.reload();
            });
        }
    });
}