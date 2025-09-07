// Notes Panel JavaScript utilities
window.NotesPanel = {
    // Reference to the current NotesPanel .NET object
    dotNetRef: null,

    // Initialize the notes panel with hotkeys and event listeners
    initialize: function(dotNetRef) {
        this.dotNetRef = dotNetRef;
        this.setupHotkeys();
        this.setupToastContainer();
    },

    // Setup keyboard shortcuts for notes
    setupHotkeys: function() {
        document.addEventListener('keydown', (e) => {
            // Check if we're in an input field or textarea
            if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA' || e.target.isContentEditable) {
                return;
            }

            // 'N' key - Create new note (only if notes panel is visible)
            if (e.key.toLowerCase() === 'n' && !e.ctrlKey && !e.altKey && !e.shiftKey) {
                const notesPanel = document.querySelector('.notes-panel');
                if (notesPanel && this.isElementVisible(notesPanel) && this.dotNetRef) {
                    e.preventDefault();
                    this.dotNetRef.invokeMethodAsync('TriggerAddNote');
                }
            }

            // 'Escape' key - Cancel current note creation/editing
            if (e.key === 'Escape') {
                const createForm = document.querySelector('.create-note-form');
                const editingCard = document.querySelector('.note-card.editing');
                
                if (createForm || editingCard) {
                    e.preventDefault();
                    // Let the component handle the escape key
                    if (this.dotNetRef) {
                        this.dotNetRef.invokeMethodAsync('HandleEscapeKey');
                    }
                }
            }

            // Ctrl+Enter - Save current note (when editing or creating)
            if (e.key === 'Enter' && e.ctrlKey) {
                const activeForm = document.querySelector('.create-note-form') || 
                                 document.querySelector('.note-card.editing .edit-form');
                
                if (activeForm) {
                    e.preventDefault();
                    const saveButton = activeForm.querySelector('.btn-primary[type="submit"], .btn-primary:contains("Save")');
                    if (saveButton && !saveButton.disabled) {
                        saveButton.click();
                    }
                }
            }
        });
    },

    // Check if an element is visible in the viewport
    isElementVisible: function(element) {
        const rect = element.getBoundingClientRect();
        return rect.width > 0 && rect.height > 0 && 
               rect.top >= 0 && rect.left >= 0 && 
               rect.bottom <= window.innerHeight && 
               rect.right <= window.innerWidth;
    },

    // Setup toast notification container
    setupToastContainer: function() {
        if (!document.getElementById('toast-container')) {
            const container = document.createElement('div');
            container.id = 'toast-container';
            container.className = 'toast-container';
            container.style.cssText = `
                position: fixed;
                top: 20px;
                right: 20px;
                z-index: 9999;
                pointer-events: none;
                max-width: 350px;
            `;
            document.body.appendChild(container);
        }
    },

    // Show a toast notification
    showToast: function(message, type = 'info', duration = 3000) {
        this.setupToastContainer();
        
        const toast = document.createElement('div');
        const toastId = 'toast-' + Date.now() + '-' + Math.random().toString(36).substr(2, 9);
        
        toast.id = toastId;
        toast.className = `toast toast-${type} show`;
        toast.style.cssText = `
            background: white;
            border: 1px solid ${this.getToastBorderColor(type)};
            border-left: 4px solid ${this.getToastColor(type)};
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            margin-bottom: 10px;
            padding: 12px 16px;
            pointer-events: auto;
            opacity: 0;
            transform: translateX(100%);
            transition: all 0.3s ease;
            max-width: 100%;
            word-wrap: break-word;
        `;

        const icon = this.getToastIcon(type);
        toast.innerHTML = `
            <div style="display: flex; align-items: flex-start; gap: 10px;">
                <div style="color: ${this.getToastColor(type)}; font-size: 16px; line-height: 1.5; margin-top: 1px;">
                    ${icon}
                </div>
                <div style="flex: 1; font-size: 14px; line-height: 1.4; color: #1f2937;">
                    ${this.escapeHtml(message)}
                </div>
                <button onclick="window.NotesPanel.removeToast('${toastId}')" 
                        style="background: none; border: none; color: #6b7280; cursor: pointer; font-size: 18px; padding: 0; margin-left: 10px;">
                    ×
                </button>
            </div>
        `;

        const container = document.getElementById('toast-container');
        container.appendChild(toast);

        // Animate in
        requestAnimationFrame(() => {
            toast.style.opacity = '1';
            toast.style.transform = 'translateX(0)';
        });

        // Auto remove after duration
        setTimeout(() => {
            this.removeToast(toastId);
        }, duration);

        return toastId;
    },

    // Remove a toast notification
    removeToast: function(toastId) {
        const toast = document.getElementById(toastId);
        if (toast) {
            toast.style.opacity = '0';
            toast.style.transform = 'translateX(100%)';
            
            setTimeout(() => {
                if (toast.parentNode) {
                    toast.parentNode.removeChild(toast);
                }
            }, 300);
        }
    },

    // Get toast color based on type
    getToastColor: function(type) {
        switch (type) {
            case 'success': return '#10b981';
            case 'error': return '#ef4444';
            case 'warning': return '#f59e0b';
            default: return '#3b82f6';
        }
    },

    // Get toast border color based on type
    getToastBorderColor: function(type) {
        switch (type) {
            case 'success': return '#d1fae5';
            case 'error': return '#fee2e2';
            case 'warning': return '#fef3c7';
            default: return '#dbeafe';
        }
    },

    // Get toast icon based on type
    getToastIcon: function(type) {
        switch (type) {
            case 'success': return '✓';
            case 'error': return '✕';
            case 'warning': return '⚠';
            default: return 'ℹ';
        }
    },

    // Escape HTML for security
    escapeHtml: function(text) {
        const map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        return text.replace(/[&<>"']/g, m => map[m]);
    },

    // Focus management for notes
    focusElement: function(selector) {
        const element = document.querySelector(selector);
        if (element) {
            element.focus();
        }
    },

    // Auto-resize textarea based on content
    autoResizeTextarea: function(textarea) {
        textarea.style.height = 'auto';
        textarea.style.height = Math.max(80, textarea.scrollHeight) + 'px';
    },

    // Setup auto-resize for textarea
    setupAutoResize: function(selector) {
        const textarea = document.querySelector(selector);
        if (textarea) {
            textarea.addEventListener('input', () => {
                this.autoResizeTextarea(textarea);
            });
            // Initial resize
            this.autoResizeTextarea(textarea);
        }
    },

    // Cleanup function
    cleanup: function() {
        this.dotNetRef = null;
        // Remove event listeners if needed
    }
};

// Global helper functions for backward compatibility
window.setupNotesHotkeys = function(dotNetRef) {
    window.NotesPanel.initialize(dotNetRef);
};

window.showToast = function(message, type = 'info', duration = 3000) {
    return window.NotesPanel.showToast(message, type, duration);
};