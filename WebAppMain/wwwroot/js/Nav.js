document.addEventListener('DOMContentLoaded', function () {
    const sidebar = document.querySelector('.sidebar');
    const toggle = document.querySelector('.toggle');

    // Open/close sidebar on mouse hover
    sidebar.addEventListener('mouseenter', function () {
        sidebar.classList.remove('close');
    });

    sidebar.addEventListener('mouseleave', function () {
        sidebar.classList.add('close');
    });

    // Toggle sidebar on click of the toggle button
    toggle.addEventListener('click', function () {
        sidebar.classList.toggle('close');
    });
});
