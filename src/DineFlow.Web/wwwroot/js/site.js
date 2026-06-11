// DineFlow Web — site.js

// ===== Toast Notifications =====
function showToast(message, type = 'success') {
    const container = document.getElementById('toast-container');
    if (!container) return;

    const icon = type === 'success' ? '✓' : '✕';
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.innerHTML = `
        <span class="toast-icon">${icon}</span>
        <span>${message}</span>
    `;
    container.appendChild(toast);

    // Auto remove after 4 seconds
    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transform = 'translateY(8px)';
        toast.style.transition = '0.3s ease';
        setTimeout(() => toast.remove(), 300);
    }, 4000);
}

// ===== Confirm Delete Modal =====
let _pendingDeleteForm = null;

function confirmDelete(formId, itemName) {
    _pendingDeleteForm = document.getElementById(formId);
    const modal = document.getElementById('delete-modal');
    if (modal) {
        const nameEl = document.getElementById('delete-item-name');
        if (nameEl) nameEl.textContent = itemName;
        modal.classList.add('show');
    }
}

function closeDeleteModal() {
    const modal = document.getElementById('delete-modal');
    if (modal) modal.classList.remove('show');
    _pendingDeleteForm = null;
}

function executeDelete() {
    if (_pendingDeleteForm) _pendingDeleteForm.submit();
    closeDeleteModal();
}

// ===== Image URL Preview =====
function previewImage(inputId, previewId) {
    const input   = document.getElementById(inputId);
    const preview = document.getElementById(previewId);
    if (!input || !preview) return;

    input.addEventListener('input', () => {
        const url = input.value.trim();
        const imgEl = preview.querySelector('img');
        if (url && imgEl) {
            imgEl.src = url;
            preview.style.display = 'block';
            imgEl.onerror = () => { preview.style.display = 'none'; };
        } else {
            preview.style.display = 'none';
        }
    });

    // Trigger on load if already has value
    if (input.value.trim()) input.dispatchEvent(new Event('input'));
}

// ===== Price Formatter =====
function formatPrice(value) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND',
        minimumFractionDigits: 0
    }).format(value);
}

// ===== DOM Ready =====
document.addEventListener('DOMContentLoaded', () => {
    // Close modal on overlay click
    document.querySelectorAll('.modal-overlay').forEach(overlay => {
        overlay.addEventListener('click', e => {
            if (e.target === overlay) closeDeleteModal();
        });
    });

    // Esc key closes modal
    document.addEventListener('keydown', e => {
        if (e.key === 'Escape') closeDeleteModal();
    });

    // Init image preview if elements exist
    previewImage('ImageUrl', 'img-preview');

    // Active nav item highlight (based on current URL)
    const currentPath = window.location.pathname.toLowerCase();
    document.querySelectorAll('.nav-item').forEach(link => {
        const href = link.getAttribute('href')?.toLowerCase() ?? '';
        if (href && currentPath.startsWith(href) && href !== '/') {
            link.classList.add('active');
        } else if (href === '/' && currentPath === '/') {
            link.classList.add('active');
        }
    });
});
