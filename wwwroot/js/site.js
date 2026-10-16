(() => {
  const toggle = document.getElementById("nav-toggle");
  if (toggle) {
    toggle.addEventListener("click", () => {
      const open = document.body.classList.toggle("sidebar-open");
      toggle.setAttribute("aria-expanded", String(open));
    });
  }
})();
