const BASE = import.meta.env.VITE_API || 'http://localhost:5000';

async function request(path, options = {}) {
    const res = await fetch(BASE + path, {
        headers: { 'Content-Type': 'application/json', ...(options.headers || {}) },
        ...options
    });
    if (!res.ok) {
        const text = await res.text();
        throw new Error(text || res.statusText);
    }
    if (res.status === 204) return null;
    return res.json();
}

export const ContactsApi = {
    list: () => request('/api/contacts'),
    get: id => request(`/api/contacts/${id}`),
    create: c => request('/api/contacts', { method: 'POST', body: JSON.stringify(c) }),
    update: (id, c) => request(`/api/contacts/${id}`, { method: 'PUT', body: JSON.stringify(c) }),
    remove: id => request(`/api/contacts/${id}`, { method: 'DELETE' }),
    saveAll: contacts => request('/api/contacts/save-all', { method: 'POST', body: JSON.stringify(contacts) })
};
