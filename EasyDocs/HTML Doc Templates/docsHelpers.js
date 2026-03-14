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

// Toggle a member entry when its header/button is clicked.
// The button is passed via "this" in markup.
function toggleMember(btn) {
    const member = btn.closest('.member');
    if (!member) return;
    const isOpen = member.classList.toggle('open');

    // simple visual indicator: replace the leading diamond with filled/empty (or rotate)
    if (isOpen) {
        btn.textContent = btn.textContent.replace('◈', '◆');
    } else {
        btn.textContent = btn.textContent.replace('◆', '◈');
    }
}

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
            link.href = `./${name}.html`;
            link.textContent = name;
            resultsBox.appendChild(link);
        });
    } else {
        resultsBox.style.display = 'none';
    }
});

// Optional: make Enter/Space trigger the same as click on keyboard-focusable toggles
document.addEventListener('keydown', (e) => {
    const target = e.target;
    if ((e.key === 'Enter' || e.key === ' ') && target.classList.contains('member-toggle')) {
        e.preventDefault();
        toggleMember(target);
    }
});