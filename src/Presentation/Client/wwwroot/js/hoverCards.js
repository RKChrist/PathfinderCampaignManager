window.getWindowSize = () => {
    return {
        width: window.innerWidth,
        height: window.innerHeight
    };
};

window.getMousePosition = (event) => {
    return {
        x: event.clientX,
        y: event.clientY
    };
};

window.getElementPosition = (element) => {
    const rect = element.getBoundingClientRect();
    return {
        x: rect.left,
        y: rect.top,
        width: rect.width,
        height: rect.height
    };
};

window.adjustHoverCardPosition = (cardElement, mouseX, mouseY) => {
    const windowWidth = window.innerWidth;
    const windowHeight = window.innerHeight;
    const cardRect = cardElement.getBoundingClientRect();
    
    let x = mouseX;
    let y = mouseY;
    
    // Adjust X position if card would go off right edge
    if (x + cardRect.width > windowWidth) {
        x = windowWidth - cardRect.width - 20;
    }
    
    // Adjust Y position if card would go off bottom edge
    if (y + cardRect.height > windowHeight) {
        y = Math.max(20, y - cardRect.height - 20);
    }
    
    // Ensure minimum margins
    x = Math.max(20, x);
    y = Math.max(20, y);
    
    return { x, y };
};

// Utility function to check if an element or its parents have a specific class
window.hasParentWithClass = (element, className) => {
    let current = element;
    while (current && current !== document.body) {
        if (current.classList && current.classList.contains(className)) {
            return true;
        }
        current = current.parentElement;
    }
    return false;
};

// Debounce function for hover events
window.debounce = (func, wait) => {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func.apply(this, args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
};