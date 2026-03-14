// helper.js
// Simple toggle for sidebar menus.

function closeMenu(menu) {
    menu.style.display = 'none';
    menu.classList.remove('open');

    const btn = menu.previousElementSibling;
    const arrow = btn && btn.querySelector('.arrow');
    if (arrow) arrow.textContent = '▼';
}

function openMenu(menu) {
    menu.style.display = 'flex';
    menu.classList.add('open');

    const btn = menu.previousElementSibling;
    const arrow = btn && btn.querySelector('.arrow');
    if (arrow) arrow.textContent = '▶';
}

function closeAllOtherMenus(exceptId) {
    const allMenus = document.querySelectorAll('.nav-links');
    allMenus.forEach(m => {
        if (m.id !== exceptId && (m.style.display === 'flex' || m.classList.contains('open'))) {
            closeMenu(m);
        }
    });
}

function toggleMenu(id) {
    const menu = document.getElementById(id);
    if (!menu) return;

    // Close any others
    closeAllOtherMenus(id);

    // Toggle the clicked one
    if (menu.classList.contains('open') || menu.style.display === 'flex') {
        closeMenu(menu);
    } else {
        openMenu(menu);
    }
}


// Optional: allow Enter / Space to toggle when the button is focused
document.addEventListener('keydown', function (e) {
    const el = document.activeElement;
    if (!el) return;
    if ((e.key === 'Enter' || e.key === ' ') && el.matches('button[onclick^="toggleMenu"]')) {
        e.preventDefault();
        // extract id string from onclick attribute, e.g. toggleMenu('classes')
        const onclick = el.getAttribute('onclick') || '';
        const m = onclick.match(/toggleMenu\(['"]([^'"]+)['"]\)/);
        if (m) toggleMenu(m[1]);
    }
});


// Get elements
const searchInputHome = document.getElementById('search');
const resultsBox = document.getElementById('search-results');
const pages = window.searchablePages;

searchInputHome.addEventListener('input', () => {
    const query = searchInputHome.value.trim().toLowerCase();
    resultsBox.innerHTML = '';

    if (!query) {
        resultsBox.style.display = 'none';
        return;
    }

    const matches = pages.filter(name =>
        name.toLowerCase().includes(query)
    );

    if (matches.length > 0) {
        resultsBox.style.display = 'block';
        matches.forEach(name => {
            const link = document.createElement('a');
            link.href = `./objs/${name}.html`;
            link.textContent = name;
            resultsBox.appendChild(link);
        });
    } else {
        resultsBox.style.display = 'none';
    }
});


// Hide results when clicking outside
document.addEventListener('click', (e) => {
    if (!resultsBox.contains(e.target) && e.target !== searchInputHome) {
        resultsBox.style.display = 'none';
    }
});
