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
        
        if (result.isConfirmed) {
            Swal.fire({
                title: '¡Eliminado!',
                text: 'Los registros han sido borrados.',
                icon: 'success',
                timer: 1500, 
                showConfirmButton: false
            }).then(() => {
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
            formData.append('idFila', idFila);


            return fetch('/Dashboard/DeleteRegistro', {
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
        if (result.isConfirmed) {
            Swal.fire({
                title: '¡Eliminado!',
                text: 'Los registros han sido borrados.',
                icon: 'success',
                timer: 1500, 
                showConfirmButton: false
            }).then(() => {
                window.location.reload();
            });
        }
    });
}

// CONVIERTE UNA CELDA EN UN INPUT PARA MODIFICAR EL VALOR
function hacerCeldaEditable(td, idFila, columna, tipoDato, valorActual, nombreTabla) {
    if (td.classList.contains('editando')) return;

    td.classList.add('editando');
    var contenidoVisualOriginal = td.innerHTML;
    var input;


    if (tipoDato === 'Select' || tipoDato === 'Relacion') {
        input = document.createElement('select');
        input.className = 'form-control';

        
        var optionVacia = document.createElement('option');
        optionVacia.value = '';
        optionVacia.text = 'Vaciar celda...';
        input.appendChild(optionVacia);

       
        var opcionesJson = td.getAttribute('data-opciones');
        if (opcionesJson) {
            var opciones = JSON.parse(opcionesJson);

           
            opciones.forEach(function (opc) {
                var option = document.createElement('option');
                if (tipoDato === 'Select') {
                    option.value = opc.Valor;
                    option.text = opc.Valor;
                    option.dataset.color = opc.Color; // GUARDA EL COLOR DE FONDO PARA MOSTRARLO EN LA SIGUIENTE FUNCION
                } else {
                    option.value = opc.Id;
                    option.text = opc.Valor;
                }

                // SI ES LA OPCION ACTUAL SE MARCA COMO SELECCIONADA
                if (option.value === valorActual) option.selected = true;

                input.appendChild(option);
            });
        }

        input.addEventListener('change', function () {
            input.blur();
        });

    } else {
        
        input = document.createElement('input');
        input.value = valorActual;
        input.className = 'form-control';

        if (tipoDato === 'Numero' || tipoDato === 'Decimal') input.type = 'number';
        else if (tipoDato === 'Fecha') input.type = 'date';
        else input.type = 'text';

        input.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') input.blur();
            else if (e.key === 'Escape') {
                td.innerHTML = contenidoVisualOriginal;
                td.classList.remove('editando');
            }
        });
    }

    td.innerHTML = '';
    td.appendChild(input);
    input.focus();


    input.addEventListener('blur', function () {
        guardarEdicionCelda(td, idFila, columna, input.value, contenidoVisualOriginal, nombreTabla, tipoDato, input);
    });
}

// FUNCION PARA ENVIAR EL NUEVO VALOR DE LA CELDA Y ALMACENARLO EN BD
function guardarEdicionCelda(td, idFila, columna, nuevoValor, contenidoOriginal, nombreTabla, tipoDato, inputElement) {
    td.classList.remove('editando');
    var textoVisual = nuevoValor;

    // COMPROBACIÓN POR SI LA HAN DEJADO VACIA QUE NO PETE LA BD
    if (nuevoValor === '') {
        textoVisual = '&nbsp;';
    } else if (tipoDato === 'Select') {
        var selectedOption = inputElement.options[inputElement.selectedIndex];
        var color = selectedOption.dataset.color || '#64748b'; 
        textoVisual = `<span class="chip" style="background-color: ${color}; color: #ffffff; border: none;">${nuevoValor}</span>`;
    } else if (tipoDato === 'Relacion') {
        var selectedOption = inputElement.options[inputElement.selectedIndex];
        textoVisual = `<span class="cell-relation">${selectedOption.text}</span>`;
    } else if (tipoDato === 'Fecha') {
        var partes = nuevoValor.split('-');
        if (partes.length === 3) textoVisual = partes[2] + '-' + partes[1] + '-' + partes[0];
    }

    td.innerHTML = textoVisual;


    var formData = new FormData();
    formData.append('nombreTabla', nombreTabla);
    formData.append('idFila', idFila);
    formData.append('columna', columna);
    formData.append('valor', nuevoValor);

    fetch('/Dashboard/UpdateCelda', {
        method: 'POST',
        body: formData
    })
        .then(response => {
            if (!response.ok) {
                td.innerHTML = contenidoOriginal;
                alSwal.fire({
                    title: 'No se ha podido actualizar',
                    text: 'El registro no se ha podido actualizar correctamente en la base de datos',
                    icon: 'warning',
                    timer: 1500,
                    showConfirmButton: false
                }).then(() => {
                    window.location.reload();
                });
            }
        }).catch(error => {
            console.error('Error:', error);
            td.innerHTML = contenidoOriginal;
        });
}

// MODAL DE CONFIRMACIÓN PARA CERRAR SESIÓN
function confirmarCerrarSesion(event) {
    event.preventDefault();

    Swal.fire({
        title: '¿Cerrar sesión?',
        text: 'Tendrás que volver a introducir tus credenciales para acceder.',
        icon: 'question',
        heightAuto: false,
        showCancelButton: true,
        confirmButtonColor: '#dc2626', 
        cancelButtonColor: '#64748b',  
        confirmButtonText: 'Sí, salir',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {            
            window.location.href = '/Auth/LogOut';
        }
    });
}

// FUNCION PARA MOSTRAR LA PAPELERA EN LAS TABLAS SELECCIONADAS
function mostrarPapeleraTablas() {
    var btnTablas = document.getElementById('btn-delete-tablas');
    var tablasSeleccionadas = document.querySelectorAll('.checkbox-tabla:checked');

    if (tablasSeleccionadas.length > 0) {
        btnTablas.style.display = 'block';
    } else {
        btnTablas.style.display = 'none';
    }
}

// FUNCION QUE COGE LAS TABLAS SELECCIONADAS Y LANZA EL SWEETALERT
function getTablasABorrar() {
    var checkboxesTablas = document.querySelectorAll('.checkbox-tabla:checked');
    var tablasPendientes = [];

    checkboxesTablas.forEach(function (chk) {
        tablasPendientes.push(chk.value);
    });

    if (tablasPendientes.length === 0) return;

    Swal.fire({
        title: '¿Eliminar tablas?',
        text: `Vas a destruir ${tablasPendientes.length} tabla(s) y TODOS sus registros. Esta acción no se puede deshacer.`,
        icon: 'warning',
        heightAuto: false,
        showCancelButton: true,
        confirmButtonColor: '#dc2626',
        cancelButtonColor: '#64748b',
        confirmButtonText: 'Sí, destruir',
        cancelButtonText: 'Cancelar',
        showLoaderOnConfirm: true,
        preConfirm: () => {
            var formData = new FormData();
            tablasPendientes.forEach(t => formData.append('nombresTablas', t));

            return fetch('/Dashboard/DeleteTablasEmpresa', {
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
        if (result.isConfirmed) {
            Swal.fire({
                title: '¡Destruidas!',
                heightAuto: false,
                text: 'Las tablas han sido eliminadas por completo.',
                icon: 'success',
                timer: 1500,
                showConfirmButton: false
            }).then(() => {
                window.location.href = '/Dashboard/Index';
            });
        }
    });
}

// FUNCION PARA ELIMINAR UNA COLUMNA COMPLETA
function eliminarColumna(nombreTabla, nombreColumna) {
    Swal.fire({
        title: `¿Eliminar la columna '${nombreColumna}'?`,
        text: "Se perderán todos los datos de esta columna en TODOS los registros. Esta acción no se puede deshacer.",
        icon: 'warning',
        heightAuto: false,
        showCancelButton: true,
        confirmButtonColor: '#dc2626',
        cancelButtonColor: '#64748b',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar',
        showLoaderOnConfirm: true,
        preConfirm: () => {
            var formData = new FormData();
            formData.append('nombreTabla', nombreTabla);
            formData.append('nombreColumna', nombreColumna);

            return fetch('/Dashboard/DeleteColumna', {
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
        if (result.isConfirmed) {
            Swal.fire({
                title: '¡Eliminada!',
                heightAuto: false,
                text: 'La columna y sus datos han sido borrados.',
                icon: 'success',
                timer: 1500,
                showConfirmButton: false
            }).then(() => {
                window.location.reload();
            });
        }
    });
}
// MOSTRAR U OCULTAR RELACIONES Y SELECTS EN EDICIÓN
function toggleEditCamposDinamicos() {
    var tipoDato = document.getElementById('edit-tipoDato').value;
    var divRelacion = document.getElementById('edit-divRelacion');
    var divOpciones = document.getElementById('edit-divOpcionesSelect');

    if (tipoDato === 'Relacion') {
        divRelacion.style.display = 'block';
        divOpciones.style.display = 'none';
    } else if (tipoDato === 'Select') {
        divRelacion.style.display = 'none';
        divOpciones.style.display = 'block';
        document.getElementById('edit-nombreTablaRelacionada').value = '';
    } else {
        divRelacion.style.display = 'none';
        divOpciones.style.display = 'none';
        document.getElementById('edit-nombreTablaRelacionada').value = '';
    }
}
// RENOMBRAR TABLA (Menú Lateral)
function renombrarTabla(nombreViejo) {
    Swal.fire({
        title: 'Renombrar tabla',
        input: 'text',
        heightAuto: false,
        inputValue: nombreViejo,
        inputPlaceholder: 'Nuevo nombre...',
        showCancelButton: true,
        confirmButtonText: 'Guardar',
        cancelButtonText: 'Cancelar',
        inputValidator: (value) => {
            if (!value) return '¡Necesitas escribir un nombre!';
            if (value === nombreViejo) return 'El nombre es el mismo.';
        },
        showLoaderOnConfirm: true,
        preConfirm: (nombreNuevo) => {
            var formData = new FormData();
            formData.append('nombreOld', nombreViejo);
            formData.append('nombreNew', nombreNuevo);

            return fetch('/Dashboard/RenameTabla', { method: 'POST', body: formData })
                .then(response => {
                    if (!response.ok) throw new Error('Error al renombrar o nombre duplicado');
                    return response.json();
                })
                .catch(error => Swal.showValidationMessage(error.message));
        }
    }).then((result) => {
        if (result.isConfirmed) {
            window.location.href = result.value.url;
        }
    });
}

// ABRIR MODAL EDITAR COLUMNA
function editarColumna(nombreViejo, tipoDatoActual) {
    document.getElementById('edit-nombreOld').value = nombreViejo;
    document.getElementById('edit-nombreNew').value = nombreViejo;
    document.getElementById('edit-tipoDato').value = tipoDatoActual;

    // Limpiamos las opciones previas si las hubiera
    document.getElementById('edit-opcionesContainer').innerHTML = '';

    toggleEditCamposDinamicos();
    document.getElementById('editColumnModal').classList.add('active');
}

// CERRAR MODAL EDITAR COLUMNA
function closeEditColumnModal() {
    document.getElementById('editColumnModal').classList.remove('active');
}

// MANEJAR SELECTS Y RELACIONES EN EDICIÓN
function toggleEditCamposDinamicos() {
    var tipoDato = document.getElementById('edit-tipoDato').value;
    var divRelacion = document.getElementById('edit-divRelacion');
    var divOpciones = document.getElementById('edit-divOpcionesSelect');

    if (tipoDato === 'Relacion') {
        divRelacion.style.display = 'block';
        divOpciones.style.display = 'none';
    } else if (tipoDato === 'Select') {
        divRelacion.style.display = 'none';
        divOpciones.style.display = 'block';
        document.getElementById('edit-nombreTablaRelacionada').value = '';
    } else {
        divRelacion.style.display = 'none';
        divOpciones.style.display = 'none';
        document.getElementById('edit-nombreTablaRelacionada').value = '';
    }
}

// AÑADIR NUEVA ETIQUETA DE COLOR EN EDICIÓN
function agregarOpcionEdit() {
    var container = document.getElementById('edit-opcionesContainer');
    var div = document.createElement('div');
    div.className = 'opcion-row';
    div.style.display = 'flex';
    div.style.gap = '8px';
    div.style.marginBottom = '8px';

    div.innerHTML = `
        <input type="text" name="opcionesValor" class="form-control" placeholder="Ej: Nueva Etiqueta" style="flex: 1; margin: 0;">
        <input type="color" name="opcionesColor" class="form-control" value="#3b82f6" style="width: 50px; padding: 2px; height: 38px; cursor: pointer;">
        <button type="button" class="btn btn-outline" onclick="eliminarOpcion(this)" style="padding: 8px 12px; margin: 0; color: #dc2626;">✕</button>
    `;
    container.appendChild(div);
}

// INTERCEPTAR EL GUARDADO DE LA COLUMNA PARA GESTIONAR ERRORES EN PANTALLA
function guardarEdicionColumna(event) {
    event.preventDefault();

    var form = document.getElementById('formEditColumn');
    var formData = new FormData(form);

    Swal.fire({
        title: 'Guardando cambios...',
        allowOutsideClick: false,
        heightAuto: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });


    fetch('/Dashboard/RenameColumna', {
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
                text: 'La columna ha sido guardada con éxito.',
                icon: 'success',
                timer: 1500,
                showConfirmButton: false
            }).then(() => {
                window.location.reload(); 
            });
        })
        .catch(error => {
            Swal.fire({
                title: 'Error de conversión',
                text: error.message,
                heightAuto: false,
                icon: 'error',
                confirmButtonText: 'Entendido',
                confirmButtonColor: '#dc2626'
            });
        });
}

// ==========================================
// MENÚ DE 3 PUNTOS EN COLUMNAS
// ==========================================
function toggleColumnMenu(event, menuId) {
    // 1. Evitamos que el clic se propague y cierre el menú inmediatamente
    event.stopPropagation();

    // 2. Cerramos cualquier otro menú que estuviera abierto
    document.querySelectorAll('.column-dropdown').forEach(menu => {
        if (menu.id !== menuId) {
            menu.style.display = 'none';
        }
    });

    // 3. Abrimos/Cerramos el menú al que hemos hecho clic
    var menu = document.getElementById(menuId);
    if (menu.style.display === 'flex') {
        menu.style.display = 'none';
    } else {
        menu.style.display = 'flex';
    }
}

// Escuchamos los clics en todo el documento para cerrar el menú si se hace clic fuera
document.addEventListener('click', function (event) {
    document.querySelectorAll('.column-dropdown').forEach(menu => {
        menu.style.display = 'none';
    });
});


// ==========================================
// SISTEMA DE FILTROS DINÁMICOS
// ==========================================
function openFilterModal() {
    document.getElementById('filterModal').classList.add('active');
}

function closeFilterModal() {
    document.getElementById('filterModal').classList.remove('active');
}

// Esta función se ejecuta cada vez que el usuario cambia el desplegable de "Columna"
function construirFiltroUI() {
    var selectCol = document.getElementById('filterCol');
    var selectOp = document.getElementById('filterOp');
    var valContainer = document.getElementById('filterValContainer');

    var selectedOption = selectCol.options[selectCol.selectedIndex];

    // Si elige la opción por defecto "Elegir columna..."
    if (!selectedOption.value) {
        selectOp.innerHTML = '<option value="">---</option>';
        selectOp.disabled = true;
        valContainer.innerHTML = '<label class="settings-label">Valor</label><input type="text" class="form-control" disabled placeholder="---" />';
        return;
    }

    var tipoDato = selectedOption.getAttribute('data-tipo');
    var opcionesJson = selectedOption.getAttribute('data-opciones');

    // 1. CONSTRUIR LAS CONDICIONES (OPERADORES) SEGÚN EL TIPO DE DATO
    selectOp.disabled = false;
    selectOp.innerHTML = '';

    if (tipoDato === 'Texto') {
        selectOp.innerHTML += '<option value="CONTAINS">Contiene</option>';
        selectOp.innerHTML += '<option value="EQUALS">Es igual a</option>';
        selectOp.innerHTML += '<option value="NOT_EQUALS">No es igual a</option>';
    }
    else if (tipoDato === 'Numero' || tipoDato === 'Decimal') {
        selectOp.innerHTML += '<option value="EQUALS">Es igual a</option>';
        selectOp.innerHTML += '<option value="GREATER">Es mayor que</option>';
        selectOp.innerHTML += '<option value="LESS">Es menor que</option>';
    }
    else if (tipoDato === 'Fecha') {
        selectOp.innerHTML += '<option value="EQUALS">Fecha exacta</option>';
        selectOp.innerHTML += '<option value="GREATER">Después del</option>';
        selectOp.innerHTML += '<option value="LESS">Antes del</option>';
    }
    else if (tipoDato === 'SiNo') {
        selectOp.innerHTML += '<option value="EQUALS">Es</option>';
    }
    else if (tipoDato === 'Select' || tipoDato === 'Relacion') {
        selectOp.innerHTML += '<option value="EQUALS">Es exactamente</option>';
        selectOp.innerHTML += '<option value="NOT_EQUALS">No es</option>';
    }

    // 2. CONSTRUIR EL INPUT DE VALOR SEGÚN EL TIPO DE DATO
    var htmlInput = '<label class="settings-label">Valor</label>';

    if (tipoDato === 'Select') {
        var opciones = JSON.parse(opcionesJson);
        htmlInput += '<select name="filterVal" class="form-control" required><option value="">Elegir opción...</option>';
        opciones.forEach(opt => { htmlInput += `<option value="${opt}">${opt}</option>`; });
        htmlInput += '</select>';
    }
    else if (tipoDato === 'Relacion') {
        var opcionesRel = JSON.parse(opcionesJson);
        htmlInput += '<select name="filterVal" class="form-control" required><option value="">Elegir vínculo...</option>';
        opcionesRel.forEach(opt => { htmlInput += `<option value="${opt.Id}">${opt.Valor}</option>`; });
        htmlInput += '</select>';
    }
    else if (tipoDato === 'Fecha') {
        htmlInput += '<input type="date" name="filterVal" class="form-control" required />';
    }
    else if (tipoDato === 'SiNo') {
        htmlInput += `
            <select name="filterVal" class="form-control" required>
                <option value="1">Sí (Marcado)</option>
                <option value="0">No (Desmarcado)</option>
            </select>`;
    }
    else if (tipoDato === 'Numero' || tipoDato === 'Decimal') {
        htmlInput += '<input type="number" step="any" name="filterVal" class="form-control" placeholder="Ej: 100" required />';
    }
    else {
        // Por defecto: Texto
        htmlInput += '<input type="text" name="filterVal" class="form-control" placeholder="Buscar..." required />';
    }

    valContainer.innerHTML = htmlInput;
}