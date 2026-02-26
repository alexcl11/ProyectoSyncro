/* Evita que SweetAlert y los modales colapsen la barra lateral de Flexbox */
body.swal2 - height - auto {
    height: 100vh!important;
}

body.swal2 - shown {
    /* Evita el pequeño salto horizontal cuando desaparece la barra de scroll */
    padding - right: 0!important;
    overflow - y: auto!important;
}