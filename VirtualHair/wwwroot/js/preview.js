// --- Elements ---
const uploadInput = document.getElementById('imageUpload');
const canvas = document.getElementById('previewCanvas');
const ctx = canvas.getContext('2d');
const styleThumbs = document.querySelectorAll('.style-thumb');

const saveBtn = document.getElementById("saveLookBtn");
const titleInput = document.getElementById("lookTitle");
const saveMsg = document.getElementById("saveLookMsg");
const resetBtn = document.getElementById("resetOverlay");

// track selected catalog ids (can be null)
let selectedHairstyleId = null;
let selectedFacialHairId = null;

let baseImage = null;
let overlayImage = null;
let overlayX = 0, overlayY = 0, overlayScale = 1, overlayRotation = 0;
let isImageLoaded = false;
let cropper = null;
let selectedThumb = null;

// fine steps (UX polish)
const STEP_MOVE = 5;        // px
const STEP_ZOOM = 0.05;     // scale
const STEP_ROTATE = 2;      // degrees

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
    if (!cropper) return;

    // ✅ crop exactly to canvas size (320x320)
    const croppedCanvas = cropper.getCroppedCanvas({ width: canvas.width, height: canvas.height });

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

function resetOverlayState() {
    overlayX = 0;
    // slightly above center fits better for hair/beard
    overlayY = Math.round(-canvas.height * 0.18); // ~ -58 for 320
    overlayScale = 0.65;
    overlayRotation = 0;
    drawCanvas();
}

styleThumbs.forEach((thumb) => {
    thumb.addEventListener('click', () => {
        if (!isImageLoaded) {
            alert("Please upload and crop your photo first!");
            return;
        }

        // Remove highlight from previous selection
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
            resetOverlayState();
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
    moveUp: () => overlayY -= STEP_MOVE,
    moveDown: () => overlayY += STEP_MOVE,
    moveLeft: () => overlayX -= STEP_MOVE,
    moveRight: () => overlayX += STEP_MOVE,
    zoomIn: () => overlayScale += STEP_ZOOM,
    zoomOut: () => overlayScale = Math.max(0.1, overlayScale - STEP_ZOOM),
    rotateLeft: () => overlayRotation -= STEP_ROTATE,
    rotateRight: () => overlayRotation += STEP_ROTATE,
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

// ✅ Reset button
resetBtn?.addEventListener("click", () => {
    if (!overlayImage) {
        alert("Select a hairstyle or beard first!");
        return;
    }
    resetOverlayState();
});

/* ---------------------- KEYBOARD CONTROLS (UX) ---------------------- */

function isTypingInInput() {
    const el = document.activeElement;
    if (!el) return false;
    const tag = (el.tagName || "").toLowerCase();
    return tag === "input" || tag === "textarea" || el.isContentEditable;
}

document.addEventListener("keydown", (e) => {
    if (!overlayImage) return;
    if (isTypingInInput()) return;

    // prevent page scroll with arrows
    const key = e.key.toLowerCase();

    let handled = true;

    switch (key) {
        case "arrowup": overlayY -= STEP_MOVE; break;
        case "arrowdown": overlayY += STEP_MOVE; break;
        case "arrowleft": overlayX -= STEP_MOVE; break;
        case "arrowright": overlayX += STEP_MOVE; break;

        case "+": // some keyboards
        case "=": overlayScale += STEP_ZOOM; break;

        case "-":
        case "_": overlayScale = Math.max(0.1, overlayScale - STEP_ZOOM); break;

        case "q": overlayRotation -= STEP_ROTATE; break;
        case "e": overlayRotation += STEP_ROTATE; break;

        case "r": resetOverlayState(); break;

        default:
            handled = false;
            break;
    }

    if (handled) {
        e.preventDefault();
        drawCanvas();
    }
});

/* ------------------------------------------------------------------- */

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

saveBtn?.addEventListener("click", async () => {
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
