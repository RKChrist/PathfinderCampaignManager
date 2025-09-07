// Pathfinder Campaign Manager JavaScript Functions

// Toast notifications
window.showToast = (message, type = 'info') => {
    // Create toast container if it doesn't exist
    let container = document.getElementById('toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container';
        container.className = 'position-fixed top-0 end-0 p-3';
        container.style.zIndex = '9999';
        document.body.appendChild(container);
    }

    // Create toast element
    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white bg-${getBootstrapColorClass(type)} border-0`;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');
    
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;

    container.appendChild(toast);
    
    // Initialize and show toast
    const bsToast = new bootstrap.Toast(toast, { delay: 4000 });
    bsToast.show();
    
    // Remove toast after it's hidden
    toast.addEventListener('hidden.bs.toast', () => {
        toast.remove();
    });
};

function getBootstrapColorClass(type) {
    switch (type.toLowerCase()) {
        case 'success': return 'success';
        case 'error': return 'danger';
        case 'warning': return 'warning';
        case 'info':
        default: return 'primary';
    }
}

// Copy to clipboard
window.copyToClipboard = async (text) => {
    try {
        await navigator.clipboard.writeText(text);
        showToast('Copied to clipboard!', 'success');
    } catch (err) {
        // Fallback for older browsers
        const textArea = document.createElement('textarea');
        textArea.value = text;
        document.body.appendChild(textArea);
        textArea.select();
        document.execCommand('copy');
        document.body.removeChild(textArea);
        showToast('Copied to clipboard!', 'success');
    }
};

// Hotkey setup functions
window.setupWizardHotkeys = () => {
    document.addEventListener('keydown', (e) => {
        if (e.ctrlKey) {
            switch (e.key.toLowerCase()) {
                case 'arrowleft':
                    e.preventDefault();
                    // Find and click previous button
                    const prevBtn = document.querySelector('.btn-secondary:contains("Previous"), .btn-outline-secondary:contains("Back")');
                    if (prevBtn && !prevBtn.disabled) prevBtn.click();
                    break;
                case 'arrowright':
                case 'enter':
                    e.preventDefault();
                    // Find and click next/finish button
                    const nextBtn = document.querySelector('.btn-primary:contains("Next"), .btn-success:contains("Finish"), .btn-primary:contains("Continue")');
                    if (nextBtn && !nextBtn.disabled) nextBtn.click();
                    break;
            }
        }
    });
};

window.setupRuleNavigatorHotkeys = (dotNetObjectRef) => {
    document.addEventListener('keydown', (e) => {
        if (e.ctrlKey) {
            switch (e.key.toLowerCase()) {
                case 'f':
                    e.preventDefault();
                    const searchInput = document.querySelector('input[type="search"], input[placeholder*="search" i]');
                    if (searchInput) searchInput.focus();
                    break;
                case 'c':
                    e.preventDefault();
                    // This would trigger copying selected rule
                    dotNetObjectRef.invokeMethodAsync('CopySelectedRule');
                    break;
            }
        }
    });
};

window.setupNotesHotkeys = (dotNetObjectRef) => {
    document.addEventListener('keydown', (e) => {
        if (e.ctrlKey) {
            switch (e.key.toLowerCase()) {
                case 'n':
                    e.preventDefault();
                    dotNetObjectRef.invokeMethodAsync('CreateNewNote');
                    break;
                case 's':
                    e.preventDefault();
                    dotNetObjectRef.invokeMethodAsync('SaveCurrentNote');
                    break;
            }
        }
    });
};

// Modal functions
window.showRuleModal = (ruleId) => {
    const modal = document.getElementById('ruleModal');
    if (modal) {
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
    }
};

// Enhanced confirm dialog
window.confirm = (message) => {
    return window.confirm(message);
};

// Focus management
window.focusElement = (selector) => {
    const element = document.querySelector(selector);
    if (element) {
        element.focus();
    }
};

// Smooth scroll to element
window.scrollToElement = (selector) => {
    const element = document.querySelector(selector);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth' });
    }
};

// Initialize app
document.addEventListener('DOMContentLoaded', () => {
    // Add any initialization code here
    console.log('Pathfinder Campaign Manager initialized');
});