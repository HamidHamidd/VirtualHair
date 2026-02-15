// --- Elements ---
const uploadInput = document.getElementById('imageUpload');
const canvas = document.getElementById('previewCanvas');
const ctx = canvas.getContext('2d');
const styleThumbs = document.querySelectorAll('.style-thumb');

const saveBtn = document.getElementById("saveLookBtn");
const titleInput = document.getElementById("lookTitle");
const saveMsg = document.getElementById("saveLookMsg");
const resetBtn = document.getElementById("resetOverlay");

let selectedHairstyleId = null;
let selectedFacialHairId = null;

let baseImage = null;
let isImageLoaded = false;
let cropper = null;
let selectedThumb = null;

/* ---------------- LAYERS ---------------- */

let hairstyleImage = null;
let facialImage = null;

let hairstyleState = { x: 0, y: 0, scale: 0.65, rotation: 0 };
let facialState = { x: 0, y: 0, scale: 0.4, rotation: 0 };

let activeLayer = null; // "hair" or "facial"

const STEP_MOVE = 5;
const STEP_ZOOM = 0.05;
const STEP_ROTATE = 2;

/* ---------------- CROP MODAL ---------------- */

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

/* ---------------- UPLOAD ---------------- */

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

    const croppedCanvas = cropper.getCroppedCanvas({ width: canvas.width, height: canvas.height });

    baseImage = new Image();
    baseImage.onload = () => {
        isImageLoaded = true;
        cropModal.style.display = 'none';
        drawCanvas();
    };
    baseImage.src = croppedCanvas.toDataURL('image/png');
});

document.getElementById('cancelCrop').addEventListener('click', () => {
    cropModal.style.display = 'none';
    if (cropper) cropper.destroy();
});

/* ---------------- THUMBNAILS ---------------- */

styleThumbs.forEach((thumb) => {
    thumb.addEventListener('click', () => {
        if (!isImageLoaded) {
            alert("Please upload and crop your photo first!");
            return;
        }

        if (selectedThumb) selectedThumb.classList.remove("selected-thumb");
        selectedThumb = thumb;
        thumb.classList.add("selected-thumb");

        const type = thumb.dataset.type;
        const id = parseInt(thumb.dataset.id);
        const src = thumb.dataset.src;

        if (type === "hairstyle") selectedHairstyleId = id;
        if (type === "facial") selectedFacialHairId = id;

        const img = new Image();
        img.onload = () => {

            if (type === "hairstyle") {
                hairstyleImage = img;
                activeLayer = "hair";
                hairstyleState = { x: 0, y: -canvas.height * 0.18, scale: 0.65, rotation: 0 };
            }

            if (type === "facial") {
                facialImage = img;
                activeLayer = "facial";
                facialState = { x: 0, y: -canvas.height * 0.05, scale: 0.4, rotation: 0 };
            }

            drawCanvas();
        };
        img.src = src;
    });
});

/* ---------------- DRAW ---------------- */

function drawCanvas() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    if (baseImage) {
        ctx.drawImage(baseImage, 0, 0, canvas.width, canvas.height);
    }

    if (hairstyleImage) {
        ctx.save();
        ctx.translate(canvas.width / 2 + hairstyleState.x, canvas.height / 2 + hairstyleState.y);
        ctx.rotate((hairstyleState.rotation * Math.PI) / 180);

        const w = hairstyleImage.width * hairstyleState.scale;
        const h = hairstyleImage.height * hairstyleState.scale;

        ctx.drawImage(hairstyleImage, -w / 2, -h / 2, w, h);
        ctx.restore();
    }

    if (facialImage) {
        ctx.save();
        ctx.translate(canvas.width / 2 + facialState.x, canvas.height / 2 + facialState.y);
        ctx.rotate((facialState.rotation * Math.PI) / 180);

        const w = facialImage.width * facialState.scale;
        const h = facialImage.height * facialState.scale;

        ctx.drawImage(facialImage, -w / 2, -h / 2, w, h);
        ctx.restore();
    }
}

/* ---------------- CONTROLS ---------------- */

const controls = {
    moveUp: () => activeLayer === "hair" ? hairstyleState.y -= STEP_MOVE : facialState.y -= STEP_MOVE,
    moveDown: () => activeLayer === "hair" ? hairstyleState.y += STEP_MOVE : facialState.y += STEP_MOVE,
    moveLeft: () => activeLayer === "hair" ? hairstyleState.x -= STEP_MOVE : facialState.x -= STEP_MOVE,
    moveRight: () => activeLayer === "hair" ? hairstyleState.x += STEP_MOVE : facialState.x += STEP_MOVE,
    zoomIn: () => activeLayer === "hair" ? hairstyleState.scale += STEP_ZOOM : facialState.scale += STEP_ZOOM,
    zoomOut: () => activeLayer === "hair"
        ? hairstyleState.scale = Math.max(0.1, hairstyleState.scale - STEP_ZOOM)
        : facialState.scale = Math.max(0.1, facialState.scale - STEP_ZOOM),
    rotateLeft: () => activeLayer === "hair" ? hairstyleState.rotation -= STEP_ROTATE : facialState.rotation -= STEP_ROTATE,
    rotateRight: () => activeLayer === "hair" ? hairstyleState.rotation += STEP_ROTATE : facialState.rotation += STEP_ROTATE,
};

Object.keys(controls).forEach(id => {
    document.getElementById(id)?.addEventListener("click", () => {
        if (!activeLayer) {
            alert("Select a hairstyle or beard first!");
            return;
        }
        controls[id]();
        drawCanvas();
    });
});

/* ---------------- RESET ---------------- */

resetBtn?.addEventListener("click", () => {
    if (!activeLayer) return;

    if (activeLayer === "hair") {
        hairstyleState = { x: 0, y: -canvas.height * 0.18, scale: 0.65, rotation: 0 };
    } else {
        facialState = { x: 0, y: -canvas.height * 0.05, scale: 0.4, rotation: 0 };
    }

    drawCanvas();
});

/* ---------------- SAVE ---------------- */

function getAntiForgeryToken() {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : "";
}

saveBtn?.addEventListener("click", async () => {

    if (!isImageLoaded) {
        alert("Upload and crop a photo first!");
        return;
    }

    const title = (titleInput.value || "").trim();
    if (!title) {
        alert("Please enter a title.");
        return;
    }

    const imageData = canvas.toDataURL("image/png");

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

    const data = await res.json();

    if (data.success) {
        window.location.href = "/UserHairstyles";
    } else {
        alert(data.message || "Save failed.");
    }
});
