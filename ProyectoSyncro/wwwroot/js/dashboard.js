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

    var filaEnMemoria = tableDataRaw.find(f => String(f['Id']) === String(idFila));
    if (filaEnMemoria) {
        filaEnMemoria[columna] = estaMarcado;
    }

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
                heightAuto: false,
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

    // Actualizamos el atributo onclick para que recuerde el valor nuevo
    var valorEscapado = nuevoValor.replace(/'/g, "\\'"); // Protegemos por si alguien escribe un nombre con apóstrofe (Ej: O'Connor)
    td.setAttribute('onclick', `hacerCeldaEditable(this, '${idFila}', '${columna}', '${tipoDato}', '${valorEscapado}', '${nombreTabla}')`);

    var filaEnMemoria = tableDataRaw.find(f => String(f['Id']) === String(idFila));
    if (filaEnMemoria) {
        // Guardamos el valor nuevo para que Kanban y Calendario lo vean
        filaEnMemoria[columna] = nuevoValor;
    }

    // También actualizamos el texto flotante por si pasan el ratón por encima
    if (tipoDato === 'Relacion' || tipoDato === 'Select') {
        td.setAttribute('title', textoVisual.replace(/<[^>]*>?/gm, '')); // Le quitamos el HTML del span al title
    } else {
        td.setAttribute('title', nuevoValor);
    }


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
                Swal.fire({
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
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {

            // 1. Mostramos un loader manual mientras el servidor trabaja
            Swal.fire({
                title: 'Destruyendo...',
                allowOutsideClick: false,
                heightAuto: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            // 2. Preparamos los datos
            var formData = new FormData();
            tablasPendientes.forEach(t => formData.append('nombresTablas', t));

            // 3. Hacemos la petición
            fetch('/Dashboard/DeleteTablasEmpresa', {
                method: 'POST',
                body: formData
            })
                .then(async response => {
                    if (!response.ok) {
                        // Si el servidor devuelve BadRequest, leemos el mensaje
                        let mensajeError = await response.text();
                        throw new Error(mensajeError);
                    }

                    // Si todo va bien, mostramos el éxito
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
                })
                .catch(error => {
                    // 4. Si hay error (SQL o Permisos), mostramos el SweetAlert ROJO
                    Swal.fire({
                        icon: 'error',
                        title: 'No se pudo eliminar',
                        text: error.message,
                        heightAuto: false,
                        confirmButtonColor: '#64748b'
                    });
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
    // 1. Evitamos que el clic se propague
    event.stopPropagation();

    // 2. Cerramos cualquier otro menú abierto
    document.querySelectorAll('.column-dropdown').forEach(menu => {
        if (menu.id !== menuId) {
            menu.style.display = 'none';
        }
    });

    var menu = document.getElementById(menuId);

    // 3. Abrimos o cerramos
    if (menu.style.display === 'flex' || menu.style.display === 'block') {
        menu.style.display = 'none';
    } else {
        // Capturamos el botón exacto que has clicado y sacamos sus coordenadas
        var boton = event.currentTarget;
        var coordenadas = boton.getBoundingClientRect();

        // Lo sacamos de la jerarquía HTML y lo fijamos a la pantalla
        menu.style.position = 'fixed';

        // Lo colocamos exactamente 4 píxeles por debajo del botón
        menu.style.top = (coordenadas.bottom + 4) + 'px';

        // Calculamos la distancia desde la derecha para que no se salga
        menu.style.left = 'auto';
        menu.style.right = (document.documentElement.clientWidth - coordenadas.right) + 'px';

        // Lo mostramos
        menu.style.display = 'flex';
    }
}

// Cerramos el menú al hacer clic en cualquier parte de la pantalla
document.addEventListener('click', function () {
    document.querySelectorAll('.column-dropdown').forEach(menu => {
        menu.style.display = 'none';
    });
});

// Si el usuario hace scroll en la tabla mientras el menú está abierto, lo cerramos
var tableContainer = document.querySelector('.data-table-container');
if (tableContainer) {
    tableContainer.addEventListener('scroll', function() {
        document.querySelectorAll('.column-dropdown').forEach(menu => {
            menu.style.display = 'none';
        });
    });
}

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


let vistaActual = 'tabla';

// FUNCIÓN PRINCIPAL PARA CAMBIAR DE VISTA
function changeView(viewName) {
    // 1. Validaciones previas
    if (viewName === 'calendario') {
        if (colsFecha.length === 0) {
            Swal.fire({
                title: 'No hay columnas de fecha',
                heightAuto: false, 
                text: 'Para usar la vista de Calendario, primero debes crear al menos una columna de tipo "Fecha" en tu tabla.',
                icon: 'info',
                confirmButtonColor: '#64748b'
            });
            return; // Detenemos el cambio de vista
        }
    }

    if (viewName === 'kanban') {
        if (colsSelect.length === 0) {
            Swal.fire({
                title: 'No hay etiquetas desplegables',
                heightAuto: false, 
                text: 'El tablero Kanban necesita organizar los datos por columnas. Crea primero una columna de tipo "Select (Desplegable)" (ej: Estado, Prioridad).',
                icon: 'info',
                confirmButtonColor: '#64748b'
            });
            return; // Detenemos el cambio de vista
        }
    }

    // 2. Cambiamos la apariencia de las pestañas
    document.querySelectorAll('.view-tab').forEach(tab => tab.classList.remove('active'));
    document.getElementById('tab-' + viewName).classList.add('active');

    // 3. Ocultamos todas las vistas y mostramos la seleccionada
    document.getElementById('vista-tabla').style.display = 'none';
    document.getElementById('vista-calendario').style.display = 'none';
    document.getElementById('vista-kanban').style.display = 'none';
    document.getElementById('vista-' + viewName).style.display = 'block';

    // 4. Manejamos el Submenú
    vistaActual = viewName;
    var submenu = document.getElementById('view-submenu');
    var selectSubmenu = document.getElementById('view-submenu-select');

    if (viewName === 'tabla') {
        submenu.style.display = 'none'; // En la tabla no agrupamos por ahora
    } else {
        submenu.style.display = 'flex';
        selectSubmenu.innerHTML = ''; // Limpiamos opciones

        let columnasAUsar = viewName === 'calendario' ? colsFecha : colsSelect;

        // Llenamos el select con las columnas compatibles
        columnasAUsar.forEach(col => {
            var option = document.createElement('option');
            option.value = col;
            option.text = col;
            selectSubmenu.appendChild(option);
        });

        // Llamamos a la función que renderizará los datos reales
        renderCurrentView();
    }
}

let fullCalendarInstance = null; 

// --- 1. FUNCIÓN REUTILIZABLE PARA LA FICHA DEL SWEETALERT ---
function mostrarFichaRegistro(datosFila, tituloRegistro) {
    var detallesHTML = '<div style="text-align: left; background: #f8fafc; padding: 15px; border-radius: 8px; border: 1px solid var(--border); margin-top: 10px; max-height: 300px; overflow-y: auto;">';
    
    for (var columna in datosFila) {
        var valor = datosFila[columna];
        
        if (columna !== 'Id' && columna.toLowerCase() !== 'fechacreacion' && valor !== null && valor !== '') {

            if (typeof valor === 'string' && valor.includes('T00:00:00')) {
                valor = valor.split('T')[0];
            }

            var valorString = String(valor);
            if (dictRelaciones && dictRelaciones[columna] && dictRelaciones[columna][valorString]) {
                valor = dictRelaciones[columna][valorString];
            }

            detallesHTML += `
                <div style="display: flex; justify-content: space-between; margin-bottom: 8px; border-bottom: 1px solid #e2e8f0; padding-bottom: 6px;">
                    <span style="font-weight: 600; color: var(--text-main); font-size: 0.85rem;">${columna}:</span>
                    <span style="color: var(--text-muted); font-size: 0.85rem; text-align: right; max-width: 60%; word-break: break-word;">${valor}</span>
                </div>`;
        }
    }
    detallesHTML += '</div>';

    Swal.fire({
        title: tituloRegistro,
        html: detallesHTML,
        heightAuto: false, 
        confirmButtonText: 'Cerrar',
        confirmButtonColor: '#64748b'
    });
}

// --- 2. RENDERIZADO PRINCIPAL DE LAS VISTAS ---
function renderCurrentView() {
    var columnaSeleccionada = document.getElementById('view-submenu-select').value;
    
    // ============================================
    // VISTA CALENDARIO
    // ============================================
    if (vistaActual === 'calendario') {
        var calendarEl = document.getElementById('calendar-container');
        var calendarEvents = [];

        tableDataRaw.forEach(function(fila) {
            var fechaRegistro = fila[columnaSeleccionada];
            if (fechaRegistro) {
                var titulo = fila[colTituloPorDefecto] || ('Registro #' + fila['Id']);
                calendarEvents.push({
                    id: fila['Id'], title: String(titulo), start: fechaRegistro,
                    allDay: true, backgroundColor: '#3b82f6', borderColor: '#2563eb',
                    extendedProps: { filaCompleta: fila }
                });
            }
        });

        if (fullCalendarInstance) fullCalendarInstance.destroy();

        fullCalendarInstance = new FullCalendar.Calendar(calendarEl, {
            initialView: 'dayGridMonth', locale: 'es', height: 'auto', firstDay: 1, 
            headerToolbar: { left: 'prev,next today', center: 'title', right: 'dayGridMonth,timeGridWeek,listMonth' },
            buttonText: {
                today: 'Hoy',
                month: 'Mes',
                week: 'Semana',
                list: 'Lista'
            },
            allDayText: 'Todo el día',
            events: calendarEvents, 
            // Usamos la nueva función reutilizable
            eventClick: function(info) {
                mostrarFichaRegistro(info.event.extendedProps.filaCompleta, info.event.title);
            }
        });
        fullCalendarInstance.render();
    } 
    
    // ============================================
    // VISTA KANBAN
    // ============================================
    else if (vistaActual === 'kanban') {
        var kanbanContainer = document.getElementById('vista-kanban');
        kanbanContainer.innerHTML = ''; // Limpiamos contenedor

        var board = document.createElement('div');
        board.className = 'kanban-board';

        // 1. Obtenemos las etiquetas configuradas para esta columna
        // dictSelects viene del C# como diccionario de arrays
        var opcionesColumna = dictSelects[columnaSeleccionada] || [];
        
        // 2. Preparamos los carriles (Columnas del Kanban)
        var carriles = {};
        
        // Creamos un carril por defecto para los que no tienen etiqueta
        carriles[''] = { titulo: 'Sin asignar', color: '#94a3b8', cards: [] };

        // Creamos los carriles oficiales con su color
        opcionesColumna.forEach(opc => {
            carriles[opc.Valor] = { titulo: opc.Valor, color: opc.Color, cards: [] };
        });

        // 3. Repartimos los datos (tableDataRaw) en los carriles
        tableDataRaw.forEach(fila => {
            var valorEtiqueta = fila[columnaSeleccionada] || '';
            
            // Si el valor no existe en la config (ej. etiqueta borrada), lo mandamos al gris
            if (!carriles[valorEtiqueta]) {
                carriles[valorEtiqueta] = { titulo: valorEtiqueta, color: '#94a3b8', cards: [] };
            }
            carriles[valorEtiqueta].cards.push(fila);
        });

        // 4. Dibujamos el HTML
        Object.keys(carriles).forEach(key => {
            var datosCarril = carriles[key];
            
            // Ocultamos la columna "Sin asignar" si está vacía para que quede más limpio
            if (key === '' && datosCarril.cards.length === 0) return; 

            var colDiv = document.createElement('div');
            colDiv.className = 'kanban-col';

            // Cabecera del carril
            var header = document.createElement('div');
            header.className = 'kanban-header';
            header.innerHTML = `<span class="kanban-badge" style="background-color: ${datosCarril.color}">${datosCarril.cards.length}</span> ${datosCarril.titulo}`;
            colDiv.appendChild(header);

            // Tarjetas (Cards)
            datosCarril.cards.forEach(fila => {
                var card = document.createElement('div');
                card.className = 'kanban-card';
                
                var tituloCard = fila[colTituloPorDefecto] || ('Registro #' + fila['Id']);
                card.innerHTML = `<div class="kanban-card-title">${tituloCard}</div>`;
                
                // Usamos la misma función de la ficha que en el Calendario
                card.onclick = function() {
                    mostrarFichaRegistro(fila, tituloCard);
                };

                colDiv.appendChild(card);
            });

            board.appendChild(colDiv);
        });

        kanbanContainer.appendChild(board);
    }
}

// ==========================================
// CONTROL DE LÍMITES (PLAN FREE)
// ==========================================
document.addEventListener("DOMContentLoaded", function () {
    // Buscamos la "miga de pan" que dejó el servidor
    var flagLimite = document.getElementById('flag-limite-free');

    // Si existe, lanzamos el SweetAlert
    if (flagLimite) {
        Swal.fire({
            icon: 'warning',
            title: 'Límite del Plan Free',
            heightAuto: false,
            html: 'Has alcanzado el límite de <b>3 tablas</b> de tu plan actual.<br><br>Para crear tablas ilimitadas y gestionar tu negocio sin frenos, mejora tu cuenta a Premium.',
            confirmButtonText: 'Ver planes Premium',
            confirmButtonColor: '#3b82f6',
            showCancelButton: true,
            cancelButtonText: 'Ahora no',
            cancelButtonColor: '#64748b'
        }).then((result) => {
            if (result.isConfirmed) {
                // Aquí en el futuro puedes redirigir a la vista de Stripe/Precios
                alert("¡Módulo de pagos en construcción! 🚀");
            }
        });
    }
});

// ==========================================
// MENÚ MÓVIL
// ==========================================
function toggleSidebar() {
    document.querySelector('.sidebar').classList.toggle('open');
    document.getElementById('mobileOverlay').classList.toggle('active');
}

// ABRIR MODAL DE NUEVA TABLA (CON LÍMITE DE 3 EN PLAN FREE)
function intentarCrearTabla(cantidadTablas, esPremium) {

    // Si NO es premium y ya tiene 3 o más tablas, ¡Paywall!
    if (!esPremium && cantidadTablas >= 3) {
        Swal.fire({
            icon: 'warning',
            title: 'Límite del Plan Free',
            heightAuto: false,
            html: 'Has alcanzado el límite de <b>3 tablas</b> de tu plan actual.<br><br>Para seguir creciendo y gestionar tu negocio sin frenos, mejora tu cuenta a Premium.',
            confirmButtonText: 'Desbloquear Premium',
            confirmButtonColor: '#3b82f6',
            showCancelButton: true,
            cancelButtonText: 'Ahora no',
            cancelButtonColor: '#64748b'
        }).then((result) => {
            if (result.isConfirmed) {
                // Redirigimos a la pasarela de pago de Stripe
                window.location.href = '/Payment/Checkout';
            }
        });
    }
    else {
        // Si tiene menos de 3 tablas (ej: 0, 1, 2) O es Premium, abre el modal sin problema
        document.getElementById('newTableModal').classList.add('active');
    }
}
// ==========================================
// EXPORTAR TABLA A PDF (VERSIÓN LIMPIA)
// ==========================================
function exportarTablaAPDF(idTabla, nombreTabla) {
    const { jsPDF } = window.jspdf;
    const doc = new jsPDF('l', 'pt', 'a4'); // 'l' = landscape (horizontal)

    // 1. Título y cabecera del documento
    doc.setFontSize(18);
    doc.setTextColor(30, 41, 59);
    doc.text(nombreTabla+' - Syncro', 40, 40);

    doc.setFontSize(10);
    doc.setTextColor(100, 116, 139);
    const fecha = new Date().toLocaleDateString('es-ES');
    doc.text(`Generado el: ${fecha}`, 40, 60);

    // =======================================================
    // 2. EXTRAER DATOS LIMPIOS DE LA TABLA HTML
    // =======================================================
    const table = document.getElementById(idTabla);
    const headers = [];
    const body = [];

    // A) Extraer Cabeceras (Empezamos en i=1 para saltar la columna del checkbox de borrar)
    const ths = table.querySelectorAll('thead th');
    for (let i = 1; i < ths.length; i++) {
        // Clonamos la cabecera para poder borrarle el menú sin afectar a la web real
        let thClone = ths[i].cloneNode(true);

        let menu = thClone.querySelector('.column-menu-container');
        if (menu) menu.remove(); // Quitamos el texto de Editar/Borrar

        // Quitamos las flechitas de ordenación (↑ ↓) si las hay
        let textoLimpio = thClone.innerText.replace(/↑|↓/g, '').trim();
        headers.push(textoLimpio);
    }

    // B) Extraer Filas (Empezamos en i=1 para saltar la primera columna vacía)
    const trs = table.querySelectorAll('tbody tr');
    trs.forEach(tr => {
        const rowData = [];
        const tds = tr.querySelectorAll('td');

        for (let i = 1; i < tds.length; i++) {
            let td = tds[i];
            let text = td.innerText.trim();

            // 🔥 ¡LA MAGIA DE LOS CHECKBOX!
            // Buscamos si dentro de esta celda hay un checkbox
            const checkbox = td.querySelector('input[type="checkbox"]');
            if (checkbox) {
                // Si lo hay, miramos si está marcado y escribimos Sí o No
                text = checkbox.checked ? 'Sí' : 'No';
            }

            rowData.push(text);
        }
        body.push(rowData);
    });

    // =======================================================
    // 3. GENERAR EL PDF
    // =======================================================
    doc.autoTable({
        head: [headers], // Le pasamos nuestro array limpio
        body: body,      // Le pasamos nuestras filas limpias
        startY: 80,
        theme: 'grid',
        headStyles: {
            fillColor: [36, 84, 147], // Azul Syncro
            textColor: 255,
            fontSize: 10,
            halign: 'center'
        },
        bodyStyles: {
            fontSize: 9,
            textColor: 50
        },
        alternateRowStyles: {
            fillColor: [248, 250, 252] // Gris muy clarito
        }
    });

    // 4. Descargar
    doc.save(`${nombreTabla}_Syncro_${fecha.replace(/\//g, '-')}.pdf`);
}

function refrescarMenuLateral() {
    // Usamos una URL relativa y sin especificar método (por defecto es GET)
    fetch('/Dashboard/GetSoloTablas')
        .then(response => {
            if (!response.ok) throw new Error("Error " + response.status);
            return response.json();
        })
        .then(tablas => {
            const contenedor = document.getElementById('contenedor-tablas-sidebar');
            if (!contenedor) return;

            if (tablas.length === 0) {
                contenedor.innerHTML = '<div style="padding: 10px 15px; font-size: 0.8rem; color: var(--text-muted);">Aún no tienes tablas.</div>';
                return;
            }

            // Construimos la lista idéntica a tu Layout
            let html = '<ul style="list-style: none; padding: 0; margin: 0;">';

            tablas.forEach(tabla => {
                html += `
                    <li style="display: flex; align-items: center; padding: 0 15px; margin-bottom: 4px;">
                        <input type="checkbox" class="checkbox-tabla" value="${tabla}" onchange="mostrarPapeleraTablas()" style="cursor: pointer; margin-right: 8px;">

                        <a href="/Dashboard/Index?tabla=${tabla}" class="nav-item" style="flex: 1; margin: 0; padding: 8px 10px;">
                            ${tabla}
                        </a>

                        <button type="button" style="background: transparent; border: none; padding: 4px; cursor: pointer; color: var(--brand);"
                                onclick="renombrarTabla('${tabla}')" title="Renombrar tabla">
                            <svg viewBox="0 0 24 24" style="width: 14px; height: 14px; stroke: currentColor; stroke-width: 2; fill: none; stroke-linecap: round; stroke-linejoin: round;">
                                <path d="M12 20h9"></path>
                                <path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z"></path>
                            </svg>
                        </button>
                    </li>`;
            });

            html += '</ul>';
            contenedor.innerHTML = html;
        })
        .catch(err => console.error("Fallo al refrescar:", err));
}