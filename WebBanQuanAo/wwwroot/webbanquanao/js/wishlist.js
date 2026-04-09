function showToast(message, type = "success") {
    let toast = document.getElementById("toast");

    if (!toast) {
        toast = document.createElement("div");
        toast.id = "toast";
        document.body.appendChild(toast);

        toast.style.position = "fixed";
        toast.style.top = "20px";
        toast.style.right = "20px";
        toast.style.padding = "12px 20px";
        toast.style.borderRadius = "8px";
        toast.style.color = "#fff";
        toast.style.zIndex = "9999";
        toast.style.opacity = "0";
        toast.style.transition = "all 0.3s ease";
    }

    toast.innerText = message;
    toast.style.background = type === "success" ? "#9933FF" : "#FF9900";

    toast.style.opacity = "1";

    setTimeout(() => {
        toast.style.opacity = "0";
    }, 2000);
}


document.addEventListener("click", async function (e) {
    const btn = e.target.closest('.wishlist-btn');
    if (!btn) return;

    e.preventDefault();

    const productId = btn.dataset.productId;
    const icon = btn.querySelector('i');

    try {
        const res = await fetch('/Product/ToggleWishlist', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(productId)
        });

        if (res.status === 401) {
            window.location.href = '/Account/Login';
            return;
        }

        const data = await res.json();

        icon.classList.toggle('text-danger', data.isAdded);
        icon.classList.toggle('text-dark', !data.isAdded);

        if (data.isAdded) {
            showToast("❤️ Đã thêm vào danh sách yêu thích", "success");
        } else {
            showToast("❌ Đã xóa khỏi danh sách yêu thích", "error");

            const item = btn.closest('.col-md-6, .col-lg-4, .col-xl-3');
            if (item) item.remove();
        }

    } catch (err) {
        console.error(err);
    }
});