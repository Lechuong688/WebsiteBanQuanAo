// Sử dụng var để có thể khai báo lại mà không gây lỗi crash JS
var floatingMenu = document.querySelector(".floating-menu");
var mainButton = document.getElementById("main-button");

if (mainButton && floatingMenu) {
    // Gán trực tiếp hàm để đảm bảo không bị chồng chéo sự kiện
    mainButton.onclick = function (e) {
        e.stopPropagation();
        floatingMenu.classList.toggle("active");
    };
}

// Hàm global cho các nút con
function scrollToTop() {
    window.scrollTo({ top: 0, behavior: "smooth" });
}