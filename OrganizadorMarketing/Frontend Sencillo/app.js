const API_BASE = 'https://localhost:7207';

function getPrioridadString(val) {
    if (typeof val === 'string') return val;
    return val == 2 ? 'Alto' : val == 1 ? 'Medio' : 'Bajo';
}

function getPrioridadValor(val) {
    if (typeof val === 'number') return val;
    if (val === 'Alto' || val == 2) return 2;
    if (val === 'Medio' || val == 1) return 1;
    return 0;
}

let authToken = localStorage.getItem('token');
let currentUser = null;
let usuariosList = [];

document.addEventListener('DOMContentLoaded', () => {
    initApp();
});

function initApp() {
    if (authToken) {
        try {
            const payload = JSON.parse(atob(authToken.split('.')[1]));
            currentUser = {
                userId: payload.userId,
                organizacionId: payload.organizacionId,
                rol: payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload.role,
                email: payload['http://schemas.xmlsoap.org/ws/2005/05/claim/label'] || payload.email
            };
            showDashboard();
        } catch (e) {
            localStorage.removeItem('token');
            authToken = null;
            showLogin();
        }
    } else {
        showLogin();
    }
}

function showLogin() {
    document.getElementById('login-view').classList.remove('hidden');
    document.getElementById('dashboard-view').classList.add('hidden');
}

function showDashboard() {
    document.getElementById('login-view').classList.add('hidden');
    document.getElementById('dashboard-view').classList.remove('hidden');
    
    document.getElementById('user-info').textContent = currentUser.email;
    document.getElementById('role-badge').textContent = currentUser.rol;
    
    const adminTabs = document.querySelectorAll('.admin-tab-btn');
    if (currentUser.rol === 'Admin') {
        adminTabs.forEach(tab => tab.classList.remove('hidden'));
    } else {
        adminTabs.forEach(tab => tab.classList.add('hidden'));
    }
    
    loadMisTareas();
    loadTodasTareas();
    if (currentUser.rol === 'Admin') {
        loadUsuarios();
        loadUsuariosForSelect();
    }
}

document.getElementById('login-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    const correo = document.getElementById('login-email').value;
    const password = document.getElementById('login-password').value;
    const errorDiv = document.getElementById('login-error');
    
    errorDiv.classList.add('hidden');
    
    try {
        const response = await fetch(`${API_BASE}/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ correo, password })
        });
        
        if (!response.ok) {
            throw new Error('Credenciales inválidas');
        }
        
        const data = await response.json();
        authToken = data.token;
        localStorage.setItem('token', authToken);
        
        const payload = JSON.parse(atob(authToken.split('.')[1]));
        currentUser = {
            userId: payload.userId,
            organizacionId: payload.organizacionId,
            rol: payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload.role,
            email: payload['http://schemas.xmlsoap.org/ws/2005/05/claim/label'] || payload.email
        };
        
        showDashboard();
    } catch (error) {
        errorDiv.textContent = error.message || 'Error al iniciar sesión';
        errorDiv.classList.remove('hidden');
    }
});

document.getElementById('logout-btn').addEventListener('click', () => {
    localStorage.removeItem('token');
    authToken = null;
    currentUser = null;
    showLogin();
});

document.querySelectorAll('.tab-btn, .admin-tab-btn').forEach(btn => {
    btn.addEventListener('click', () => {
        const tabId = btn.dataset.tab;
        
        document.querySelectorAll('.tab-btn, .admin-tab-btn').forEach(b => b.classList.remove('active'));
        btn.classList.add('active');
        
        document.querySelectorAll('.tab-content').forEach(content => content.classList.add('hidden'));
        document.getElementById(`tab-${tabId}`).classList.remove('hidden');
    });
});

document.getElementById('btn-crear-mi-tarea').addEventListener('click', () => {
    openModal('modal-crear-mi-tarea');
});

document.getElementById('crear-mi-tarea-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    const errorDiv = document.getElementById('crear-mi-tarea-error');
    errorDiv.classList.add('hidden');
    
    const titulo = document.getElementById('mi-tarea-titulo').value;
    const descripcion = document.getElementById('mi-tarea-descripcion').value;
    const prioridad = document.getElementById('mi-tarea-prioridad').value;
    const limiteTarea = document.getElementById('mi-tarea-fecha').value || null;
    
    try {
        const response = await fetch(`${API_BASE}/tareas`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${authToken}`
            },
            body: JSON.stringify({
                titulo,
                descripcion,
                asignadoaId: currentUser.userId,
                prioridad: getPrioridadString(prioridad),
                limiteTarea
            })
        });
        
        const data = await response.json();
        
        if (!response.ok) {
            throw new Error(data.message || 'Error al crear tarea');
        }
        
        closeModal('modal-crear-mi-tarea');
        document.getElementById('crear-mi-tarea-form').reset();
        loadMisTareas();
        loadTodasTareas();
    } catch (error) {
        errorDiv.textContent = error.message;
        errorDiv.classList.remove('hidden');
    }
});

document.getElementById('btn-crear-usuario').addEventListener('click', () => {
    openModal('modal-crear-usuario');
});

document.getElementById('crear-usuario-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    const errorDiv = document.getElementById('crear-usuario-error');
    errorDiv.classList.add('hidden');
    
    const correo = document.getElementById('usuario-correo').value;
    const password = document.getElementById('usuario-password').value;
    const rol = document.getElementById('usuario-rol').value;
    
    try {
        const response = await fetch(`${API_BASE}/usuarios`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${authToken}`
            },
            body: JSON.stringify({ correo, password, rol })
        });
        
        const data = await response.json();
        
        if (!response.ok) {
            throw new Error(data.message || 'Error al crear usuario');
        }
        
        closeModal('modal-crear-usuario');
        document.getElementById('crear-usuario-form').reset();
        loadUsuarios();
    } catch (error) {
        errorDiv.textContent = error.message;
        errorDiv.classList.remove('hidden');
    }
});

document.getElementById('crear-tarea-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    const errorDiv = document.getElementById('crear-tarea-error');
    errorDiv.classList.add('hidden');
    
    const titulo = document.getElementById('tarea-titulo').value;
    const descripcion = document.getElementById('tarea-descripcion').value;
    const asignadoaId = parseInt(document.getElementById('tarea-asignado').value);
    const prioridad = document.getElementById('tarea-prioridad').value;
    const limiteTarea = document.getElementById('tarea-fecha').value || null;
    
    try {
        const response = await fetch(`${API_BASE}/tareas`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${authToken}`
            },
            body: JSON.stringify({
                titulo,
                descripcion,
                asignadoaId,
                prioridad: getPrioridadString(prioridad),
                limiteTarea
            })
        });
        
        const data = await response.json();
        
        if (!response.ok) {
            throw new Error(data.message || 'Error al crear tarea');
        }
        
        document.getElementById('crear-tarea-form').reset();
        document.getElementById('crear-tarea-error').classList.add('hidden');
        alert('Tarea creada exitosamente');
        loadTodasTareas();
    } catch (error) {
        errorDiv.textContent = error.message;
        errorDiv.classList.remove('hidden');
    }
});

document.getElementById('editar-usuario-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    const errorDiv = document.getElementById('editar-usuario-error');
    errorDiv.classList.add('hidden');
    
    const id = document.getElementById('editar-usuario-id').value;
    const correo = document.getElementById('editar-usuario-correo').value;
    const rol = document.getElementById('editar-usuario-rol').value;
    
    try {
        const response = await fetch(`${API_BASE}/usuarios/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${authToken}`
            },
            body: JSON.stringify({ correo, rol })
        });
        
        const data = await response.json();
        
        if (!response.ok) {
            throw new Error(data.message || 'Error al actualizar usuario');
        }
        
        closeModal('modal-editar-usuario');
        loadUsuarios();
    } catch (error) {
        errorDiv.textContent = error.message;
        errorDiv.classList.remove('hidden');
    }
});

document.getElementById('editar-tarea-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    const errorDiv = document.getElementById('editar-tarea-error');
    errorDiv.classList.add('hidden');
    
    const id = document.getElementById('editar-tarea-id').value;
    const titulo = document.getElementById('editar-tarea-titulo').value;
    const descripcion = document.getElementById('editar-tarea-descripcion').value;
    const asignadoaId = parseInt(document.getElementById('editar-tarea-asignado').value);
    const prioridad = document.getElementById('editar-tarea-prioridad').value;
    const estado = document.getElementById('editar-tarea-estado').value;
    const limiteTarea = document.getElementById('editar-tarea-fecha').value || null;
    
    try {
        const response = await fetch(`${API_BASE}/tareas/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${authToken}`
            },
            body: JSON.stringify({
                titulo,
                descripcion,
                asignadoaId,
                prioridad: getPrioridadString(prioridad),
                estado,
                limiteTarea
            })
        });
        
        const text = await response.text();
        
        if (!response.ok) {
            throw new Error(text || 'Error al actualizar tarea');
        }
        
        closeModal('modal-editar-tarea');
        loadMisTareas();
        loadTodasTareas();
    } catch (error) {
        errorDiv.textContent = error.message;
        errorDiv.classList.remove('hidden');
    }
});

document.getElementById('btn-filtrar').addEventListener('click', () => {
    loadTodasTareas();
});

async function loadMisTareas() {
    const container = document.getElementById('mis-tareas-list');
    container.innerHTML = '<div class="loading">CARGANDO...</div>';
    
    try {
        const response = await fetch(`${API_BASE}/tareas/mis-tareas`, {
            headers: { 'Authorization': `Bearer ${authToken}` }
        });
        
        if (!response.ok) {
            container.innerHTML = '<div class="empty-state">Error al cargar tareas</div>';
            return;
        }
        
        const tareas = await response.json();
        renderMisTareas(tareas, container);
    } catch (error) {
        container.innerHTML = '<div class="empty-state">Error de conexión</div>';
    }
}

function renderMisTareas(tareas, container) {
    if (!tareas || tareas.length === 0) {
        container.innerHTML = '<div class="empty-state">No tienes tareas asignadas</div>';
        return;
    }
    
    container.innerHTML = '';
    tareas.forEach(tarea => {
        const estado = tarea.estado || 'Pendiente';
        const estadoClass = estado.toLowerCase();
        
        const card = document.createElement('div');
        card.className = `tarea-card ${estadoClass}`;
        card.innerHTML = `
            <div class="flex justify-between items-start mb-2">
                <h3 class="text-lg font-bold text-turquoise-300">${tarea.titulo}</h3>
                <div class="flex gap-2">
                    <span class="estado-badge ${estadoClass}">${estado}</span>
                    <span class="prioridad-badge ${getPrioridadString(tarea.prioridad).toLowerCase()}">${getPrioridadString(tarea.prioridad)}</span>
                </div>
            </div>
            <p class="text-turquoise-400/70 mb-3">${tarea.descripcion}</p>
            ${tarea.limiteTarea ? `<p class="text-turquoise-500 text-sm mb-3">Límite: ${new Date(tarea.limiteTarea).toLocaleString()}</p>` : ''}
            <div class="flex gap-2">
                ${estado !== 'Completo' ? `
                <button class="action-btn" onclick="cambiarEstado(${tarea.id}, 'Processo')">Processo</button>
                <button class="action-btn" onclick="cambiarEstado(${tarea.id}, 'Completo')">Completar</button>
                ` : ''}
                ${estado === 'Processo' ? `
                <button class="action-btn" onclick="cambiarEstado(${tarea.id}, 'Pendiente')">Retroceder</button>
                ` : ''}
            </div>
        `;
        container.appendChild(card);
    });
}

async function loadTodasTareas() {
    const container = document.getElementById('todas-tareas-list');
    container.innerHTML = '<div class="loading">CARGANDO...</div>';
    
    const filterUsuario = document.getElementById('filter-usuario').value;
    const filterEstado = document.getElementById('filter-estado').value;
    const filterPrioridad = document.getElementById('filter-prioridad').value;
    
    let url = `${API_BASE}/tareas/all`;
    
    try {
        const response = await fetch(url, {
            headers: { 'Authorization': `Bearer ${authToken}` }
        });
        
        if (!response.ok) {
            container.innerHTML = '<div class="empty-state">Error al cargar tareas</div>';
            return;
        }
        
        let tareas = await response.json();
        
        if (filterUsuario) {
            tareas = tareas.filter(t => t.asignadoId == filterUsuario || t.asignado === document.querySelector(`#filter-usuario option[value="${filterUsuario}"]`)?.text);
        }
        
        if (filterEstado) {
            tareas = tareas.filter(t => t.estado === filterEstado);
        }
        
        if (filterPrioridad) {
            tareas = tareas.filter(t => getPrioridadValor(t.prioridad) == filterPrioridad);
        }
        
        renderTodasTareas(tareas, container);
    } catch (error) {
        container.innerHTML = '<div class="empty-state">Error de conexión</div>';
    }
}

function renderTodasTareas(tareas, container) {
    if (!tareas || tareas.length === 0) {
        container.innerHTML = '<div class="empty-state">No hay tareas</div>';
        return;
    }
    
    container.innerHTML = '';
    tareas.forEach(tarea => {
        const estado = tarea.estado || 'Pendiente';
        const estadoClass = estado.toLowerCase();
        
        const card = document.createElement('div');
        card.className = `tarea-card ${estadoClass}`;
        card.innerHTML = `
            <div class="flex justify-between items-start mb-2">
                <div>
                    <h3 class="text-lg font-bold text-turquoise-300">${tarea.titulo}</h3>
                    <p class="text-turquoise-500 text-sm">Asignado: ${tarea.asignado || 'N/A'}</p>
                </div>
                <div class="flex gap-2">
                    <span class="estado-badge ${estadoClass}">${estado}</span>
                    <span class="prioridad-badge ${getPrioridadString(tarea.prioridad).toLowerCase()}">${getPrioridadString(tarea.prioridad)}</span>
                </div>
            </div>
            <p class="text-turquoise-400/70 mb-3">${tarea.descripcion}</p>
            ${tarea.limiteTarea ? `<p class="text-turquoise-500 text-sm mb-3">Límite: ${new Date(tarea.limiteTarea).toLocaleString()}</p>` : ''}
            <div class="flex gap-2">
                ${currentUser.rol === 'Admin' ? `
                <button class="action-btn" onclick="openEditarTarea(${tarea.id}, '${tarea.titulo}', '${tarea.descripcion}', '${tarea.asignado}', ${getPrioridadValor(tarea.prioridad)}, '${estado}')">Editar</button>
                ${estado !== 'Completo' ? `
                <button class="action-btn" onclick="cambiarEstado(${tarea.id}, 'Processo')">Processo</button>
                <button class="action-btn" onclick="cambiarEstado(${tarea.id}, 'Completo')">Completar</button>
                ` : ''}
                <button class="action-btn danger" onclick="eliminarTarea(${tarea.id})">Eliminar</button>
                ` : ''}
                ${currentUser.rol !== 'Admin' && tarea.asignado === currentUser.email && estado !== 'Completo' ? `
                <button class="action-btn" onclick="cambiarEstado(${tarea.id}, 'Processo')">Processo</button>
                <button class="action-btn" onclick="cambiarEstado(${tarea.id}, 'Completo')">Completar</button>
                ` : ''}
            </div>
        `;
        container.appendChild(card);
    });
}

async function loadUsuarios() {
    const container = document.getElementById('usuarios-list');
    container.innerHTML = '<div class="loading">CARGANDO...</div>';
    
    try {
        const response = await fetch(`${API_BASE}/usuarios`, {
            headers: { 'Authorization': `Bearer ${authToken}` }
        });
        
        if (!response.ok) {
            container.innerHTML = '<div class="empty-state">Error al cargar usuarios</div>';
            return;
        }
        
        usuariosList = await response.json();
        renderUsuarios(usuariosList, container);
    } catch (error) {
        container.innerHTML = '<div class="empty-state">Error de conexión</div>';
    }
}

function renderUsuarios(usuarios, container) {
    if (!usuarios || usuarios.length === 0) {
        container.innerHTML = '<div class="empty-state">No hay usuarios</div>';
        return;
    }
    
    container.innerHTML = '';
    usuarios.forEach(usuario => {
        const card = document.createElement('div');
        card.className = 'usuario-card';
        card.innerHTML = `
            <div>
                <p class="text-turquoise-300 font-bold">${usuario.id}</p>
                <p class="text-turquoise-400">${usuario.correo}</p>
            </div>
            <div class="flex gap-2 items-center">
                <span class="px-3 py-1 rounded-full text-xs font-bold bg-turquoise-700 text-black">${usuario.rol}</span>
                <button class="action-btn" onclick="openEditarUsuario(${usuario.id}, '${usuario.correo}', '${usuario.rol}')">Editar</button>
                <button class="action-btn danger" onclick="eliminarUsuario(${usuario.id})">Eliminar</button>
            </div>
        `;
        container.appendChild(card);
    });
}

async function loadUsuariosForSelect() {
    try {
        const response = await fetch(`${API_BASE}/usuarios`, {
            headers: { 'Authorization': `Bearer ${authToken}` }
        });
        
        if (!response.ok) return;
        
        const usuarios = await response.json();
        
        const selectTarea = document.getElementById('tarea-asignado');
        const selectEditar = document.getElementById('editar-tarea-asignado');
        const filterSelect = document.getElementById('filter-usuario');
        
        selectTarea.innerHTML = '<option value="">Seleccionar usuario</option>';
        selectEditar.innerHTML = '';
        filterSelect.innerHTML = '<option value="">Todos los usuarios</option>';
        
        usuarios.forEach(u => {
            selectTarea.innerHTML += `<option value="${u.id}">${u.correo}</option>`;
            selectEditar.innerHTML += `<option value="${u.id}">${u.correo}</option>`;
            filterSelect.innerHTML += `<option value="${u.id}">${u.correo}</option>`;
        });
    } catch (error) {
        console.error('Error al cargar usuarios para select');
    }
}

async function cambiarEstado(tareaId, nuevoEstado) {
    console.log('Cambiando estado:', tareaId, nuevoEstado);
    try {
        const response = await fetch(`${API_BASE}/tareas/${tareaId}/estado`, {
            method: 'PATCH',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${authToken}`
            },
            body: JSON.stringify({ estado: nuevoEstado })
        });
        
        console.log('Response status:', response.status);
        
        if (!response.ok) {
            const text = await response.text();
            console.log('Response body:', text);
            if (text) {
                try {
                    const data = JSON.parse(text);
                    alert(data.message || 'Error al cambiar estado');
                } catch {
                    alert(text || 'Error al cambiar estado');
                }
            } else {
                alert('Error: Estado HTTP ' + response.status);
            }
            return;
        }
        
        loadMisTareas();
        loadTodasTareas();
    } catch (error) {
        console.error('Error:', error);
        alert('Error de conexión: ' + error.message);
    }
}

async function eliminarTarea(tareaId) {
    if (!confirm('¿Estás seguro de eliminar esta tarea?')) return;
    
    try {
        const response = await fetch(`${API_BASE}/tareas/${tareaId}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${authToken}` }
        });
        
        if (!response.ok) {
            const text = await response.text();
            alert(text || 'Error al eliminar tarea');
            return;
        }
        
        loadMisTareas();
        loadTodasTareas();
    } catch (error) {
        alert('Error de conexión');
    }
}

async function eliminarUsuario(usuarioId) {
    if (!confirm('¿Estás seguro de eliminar este usuario?')) return;
    
    try {
        const response = await fetch(`${API_BASE}/usuarios/${usuarioId}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${authToken}` }
        });
        
        if (!response.ok) {
            const data = await response.json();
            alert(data.message || 'Error al eliminar usuario');
            return;
        }
        
        loadUsuarios();
        loadUsuariosForSelect();
    } catch (error) {
        alert('Error de conexión');
    }
}

function openModal(modalId) {
    document.getElementById(modalId).classList.remove('hidden');
}

function closeModal(modalId) {
    document.getElementById(modalId).classList.add('hidden');
}

function openEditarUsuario(id, correo, rol) {
    document.getElementById('editar-usuario-id').value = id;
    document.getElementById('editar-usuario-correo').value = correo;
    document.getElementById('editar-usuario-rol').value = rol;
    openModal('modal-editar-usuario');
}

function openEditarTarea(id, titulo, descripcion, asignado, prioridad, estado) {
    document.getElementById('editar-tarea-id').value = id;
    document.getElementById('editar-tarea-titulo').value = titulo;
    document.getElementById('editar-tarea-descripcion').value = descripcion;
    
    loadUsuariosForSelect().then(() => {
        const select = document.getElementById('editar-tarea-asignado');
        for (let i = 0; i < select.options.length; i++) {
            if (select.options[i].text === asignado) {
                select.selectedIndex = i;
                break;
            }
        }
    });
    
    const prioridadValor = prioridad == 2 ? '2' : prioridad == 1 ? '1' : '0';
    document.getElementById('editar-tarea-prioridad').value = prioridadValor;
    document.getElementById('editar-tarea-estado').value = estado;
    openModal('modal-editar-tarea');
}