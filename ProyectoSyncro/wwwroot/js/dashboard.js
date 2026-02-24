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