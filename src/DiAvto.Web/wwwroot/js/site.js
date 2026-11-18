(function () {
    "use strict";

    window.diavtoCopyText = function (text) {
        if (navigator.clipboard && window.isSecureContext) {
            return navigator.clipboard.writeText(text);
        }

        return new Promise(function (resolve, reject) {
            try {
                const textarea = document.createElement("textarea");
                textarea.value = text;
                textarea.setAttribute("readonly", "");
                textarea.style.position = "fixed";
                textarea.style.opacity = "0";
                document.body.appendChild(textarea);
                textarea.select();
                const copied = document.execCommand("copy");
                document.body.removeChild(textarea);

                if (copied) {
                    resolve();
                } else {
                    reject(new Error("copy failed"));
                }
            } catch (error) {
                reject(error);
            }
        });
    };

    const navToggle = document.querySelector("[data-nav-toggle]");
    const nav = document.getElementById("siteNav");

    if (navToggle && nav) {
        navToggle.addEventListener("click", function () {
            nav.classList.toggle("open");
        });

        nav.querySelectorAll("a").forEach(function (link) {
            link.addEventListener("click", function () {
                nav.classList.remove("open");
            });
        });
    }

    const lightbox = document.getElementById("carLightbox");
    const lightboxImage = document.getElementById("lightboxImage");
    const lightboxCounter = document.getElementById("lightboxCounter");
    const mainImage = document.getElementById("mainCarImage");

    if (lightbox && lightboxImage) {
        let images = [];
        let current = 0;

        function collectImages() {
            const sources = [];
            document.querySelectorAll(".photo-grid img, .gallery-thumb img").forEach(function (img) {
                if (img.src && sources.indexOf(img.src) === -1) {
                    sources.push(img.src);
                }
            });

            if (sources.length === 0 && mainImage && mainImage.src) {
                sources.push(mainImage.src);
            }

            return sources;
        }

        function render() {
            if (images.length === 0) {
                return;
            }

            if (current < 0) current = images.length - 1;
            if (current >= images.length) current = 0;

            lightboxImage.src = images[current];
            if (lightboxCounter) {
                lightboxCounter.textContent = (current + 1) + " / " + images.length;
            }
        }

        function open(src) {
            images = collectImages();
            const target = src || (mainImage ? mainImage.src : "");
            current = Math.max(0, images.indexOf(target));
            render();
            lightbox.hidden = false;
            document.body.style.overflow = "hidden";
        }

        function close() {
            lightbox.hidden = true;
            document.body.style.overflow = "";
        }

        document.querySelectorAll("[data-gallery-open]").forEach(function (button) {
            button.addEventListener("click", function () {
                open(button.getAttribute("data-src") || "");
            });
        });

        document.querySelectorAll("[data-gallery-thumb]").forEach(function (thumb) {
            thumb.addEventListener("click", function () {
                if (!mainImage) return;

                const src = thumb.getAttribute("data-src");
                if (!src) return;

                mainImage.src = src;
                document.querySelectorAll("[data-gallery-thumb]").forEach(function (item) {
                    item.classList.remove("active");
                });
                thumb.classList.add("active");
            });
        });

        lightbox.querySelectorAll("[data-lightbox-close]").forEach(function (button) {
            button.addEventListener("click", close);
        });

        lightbox.querySelectorAll("[data-lightbox-prev]").forEach(function (button) {
            button.addEventListener("click", function () {
                current--;
                render();
            });
        });

        lightbox.querySelectorAll("[data-lightbox-next]").forEach(function (button) {
            button.addEventListener("click", function () {
                current++;
                render();
            });
        });

        lightbox.addEventListener("click", function (event) {
            if (event.target === lightbox) {
                close();
            }
        });

        document.addEventListener("keydown", function (event) {
            if (lightbox.hidden) return;

            if (event.key === "Escape") close();
            if (event.key === "ArrowLeft") { current--; render(); }
            if (event.key === "ArrowRight") { current++; render(); }
        });
    }

    document.querySelectorAll(".alert-success").forEach(function (alert) {
        window.setTimeout(function () {
            alert.style.transition = "opacity .4s ease, transform .4s ease";
            alert.style.opacity = "0";
            alert.style.transform = "translateY(-4px)";
            window.setTimeout(function () { alert.remove(); }, 450);
        }, 6000);
    });
})();
