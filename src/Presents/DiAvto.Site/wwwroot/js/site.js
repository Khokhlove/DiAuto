(function () {
    "use strict";

    let bound = false;

    function update() {
        const header = document.querySelector(".site-header");
        if (!header) {
            return;
        }

        header.classList.toggle("scrolled", window.scrollY > 24);
    }

    window.diavtoHeaderScroll = function () {
        if (!bound) {
            bound = true;
            window.addEventListener("scroll", update, { passive: true });
        }

        update();
    };
})();
