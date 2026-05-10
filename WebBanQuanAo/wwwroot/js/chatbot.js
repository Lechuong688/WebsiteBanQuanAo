
var floatingMenu = document.querySelector(".floating-menu");
var mainButton = document.getElementById("main-button");

if (mainButton && floatingMenu) {
    mainButton.onclick = function (e) {
        e.stopPropagation();
        floatingMenu.classList.toggle("active");
    };
}

function scrollToTop() {
    window.scrollTo({ top: 0, behavior: "smooth" });
}