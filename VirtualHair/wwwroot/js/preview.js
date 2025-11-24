// --- Elements ---
const uploadInput = document.getElementById('imageUpload');
const canvas = document.getElementById('previewCanvas');
const ctx = canvas.getContext('2d');
const styleThumbs = document.querySelectorAll('.style-thumb');

const saveBtn = document.getElementById("saveLookBtn");
const titleInput = document.getElementById("lookTitle");
const saveMsg = document.getElementById("saveLookMsg");

// track selected catalog ids (can be null)
let selectedHairstyleId = null;
let selectedFacialHairId = null;

let baseImage = null;
let overlayImage = null;
let overlayX = 0, overlayY = 0, overlayScale = 1, overlayRotation = 0;
let isImageLoaded = false;
let cropper = null;
let selectedThumb = null;

// --- Cropper Modal ---
const cropModal = document.createElement('div');
cropModal.classList.add('cropper-modal');
cropModal.innerHTML = `
<div class="cropper-box">
  <h5 class="mb-2">Adjust your photo</h5>
  <img id="cropperImage" style="max-width:100%; display:block; border-radius:8px;">
  <div class="d-flex justify-content-center gap-3 mt-3">
    <button id="confirmCrop" class="btn btn-primary px-3">Confirm</button>
    <button id="cancelCrop" class="btn btn-outline-secondary px-3">Cancel</button>
  </div>
</div>`;
document.body.appendChild(cropModal);

// --- Upload Photo ---
uploadInput.addEventListener('change', (e) => {
    const file = e.target.files[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (event) => {
        const cropperImg = document.getElementById('cropperImage');
        cropperImg.src = event.target.result;
        cropModal.style.display = 'flex';

        cropperImg.onload = () => {
            if (cropper) cropper.destroy();
            cropper = new Cropper(cropperImg, {
                aspectRatio: 1,
                viewMode: 2,
                movable: true,
                zoomable: true,
                background: false,
                guides: false
            });
        };
    };
    reader.readAsDataURL(file);
});

document.getElementById('confirmCrop').addEventListener('click', () => {
    const croppedCanvas = cropper.getCroppedCanvas({ width: 480, height: 480 });

    baseImage = new Image();
    baseImage.onload = () => {
        isImageLoaded = true;
        cropModal.style.display = 'none';
        drawCanvas();
    };
    baseImage.src = croppedCanvas.toDataURL('image/png');
});

// optional cancel
document.getElementById('cancelCrop').addEventListener('click', () => {
    cropModal.style.display = 'none';
    if (cropper) cropper.destroy();
});

/* ---------------------- THUMBNAIL → OVERLAY ---------------------- */

styleThumbs.forEach((thumb) => {
    thumb.addEventListener('click', () => {
        if (!isImageLoaded) {
            alert("Please upload and crop your photo first!");
            return;
        }

        // Remove highlight from previous selection (simple UX)
        if (selectedThumb) selectedThumb.classList.remove("selected-thumb");
        selectedThumb = thumb;
        thumb.classList.add("selected-thumb");

        // Store selected id per type (for saving)
        const type = thumb.dataset.type;
        const id = parseInt(thumb.dataset.id);
        if (type === "hairstyle") selectedHairstyleId = id;
        if (type === "facial") selectedFacialHairId = id;

        // Load overlay image
        const src = thumb.dataset.src;
        if (!src) {
            console.error("ERROR: Missing data-src for thumbnail");
            return;
        }

        overlayImage = new Image();
        overlayImage.onload = () => {
            overlayX = 0;
            overlayY = -60;
            overlayScale = 0.7;
            overlayRotation = 0;
            drawCanvas();
        };
        overlayImage.src = src;
    });
});

/* ------------------------------------------------------------------- */

// --- Draw Canvas ---
function drawCanvas() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    // Draw base
    if (baseImage) {
        ctx.drawImage(baseImage, 0, 0, canvas.width, canvas.height);
    }

    // Draw overlay
    if (overlayImage) {
        ctx.save();
        ctx.translate(canvas.width / 2 + overlayX, canvas.height / 2 + overlayY);
        ctx.rotate((overlayRotation * Math.PI) / 180);

        const w = overlayImage.width * overlayScale;
        const h = overlayImage.height * overlayScale;

        ctx.drawImage(overlayImage, -w / 2, -h / 2, w, h);
        ctx.restore();
    }
}

// --- Controls ---
const controls = {
    moveUp: () => overlayY -= 10,
    moveDown: () => overlayY += 10,
    moveLeft: () => overlayX -= 10,
    moveRight: () => overlayX += 10,
    zoomIn: () => overlayScale += 0.1,
    zoomOut: () => overlayScale = Math.max(0.1, overlayScale - 0.1),
    rotateLeft: () => overlayRotation -= 5,
    rotateRight: () => overlayRotation += 5,
};

Object.keys(controls).forEach(id => {
    document.getElementById(id)?.addEventListener("click", () => {
        if (!overlayImage) {
            alert("Select a hairstyle or beard first!");
            return;
        }
        controls[id]();
        drawCanvas();
    });
});

// Prevent background scroll during cropper
const body = document.body;
new MutationObserver(() => {
    body.style.overflow = cropModal.style.display === "flex" ? "hidden" : "";
}).observe(cropModal, { attributes: true, attributeFilter: ["style"] });

/* ---------------------- SAVE LOOK ---------------------- */

function getAntiForgeryToken() {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : "";
}

saveBtn.addEventListener("click", async () => {
    if (!isImageLoaded || !baseImage) {
        alert("Upload and crop a photo first!");
        return;
    }

    const title = (titleInput.value || "").trim();
    if (!title) {
        alert("Please enter a title for your look.");
        titleInput.focus();
        return;
    }

    // take final canvas as PNG
    const imageData = canvas.toDataURL("image/png");

    saveBtn.disabled = true;
    saveMsg.textContent = "Saving...";
    saveMsg.className = "small text-muted mt-2";

    try {
        const res = await fetch("/UserHairstyles/SaveLook", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": getAntiForgeryToken()
            },
            body: JSON.stringify({
                title,
                imageData,
                hairstyleId: selectedHairstyleId,
                facialHairId: selectedFacialHairId
            })
        });

        if (!res.ok) {
            const txt = await res.text();
            throw new Error(txt || "Save failed");
        }

        const data = await res.json();

        if (data.success) {
            saveMsg.textContent = "✅ Saved! Redirecting to My Gallery...";
            saveMsg.className = "small text-success mt-2";
            window.location.href = "/UserHairstyles";
        } else {
            saveMsg.textContent = data.message || "❌ Could not save.";
            saveMsg.className = "small text-danger mt-2";
        }

    } catch (err) {
        console.error(err);
        saveMsg.textContent = "❌ Error: " + err.message;
        saveMsg.className = "small text-danger mt-2";
    } finally {
        saveBtn.disabled = false;
    }
});

/* ------------------------------------------------------------------- */
