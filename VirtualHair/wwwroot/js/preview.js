// --- Elements ---
const uploadInput = document.getElementById('imageUpload');
const canvas = document.getElementById('previewCanvas');
const ctx = canvas.getContext('2d');
const styleThumbs = document.querySelectorAll('.style-thumb');

let baseImage = null;
let overlayImage = null;
let overlayX = 0, overlayY = 0, overlayScale = 1, overlayRotation = 0;
let isImageLoaded = false;
let cropper = null;

// --- Temporary elements for cropping ---
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

// --- Upload photo ---
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

// --- Confirm / Cancel Crop ---
document.getElementById('confirmCrop').addEventListener('click', () => {
    const croppedCanvas = cropper.getCroppedCanvas({ width: 480, height: 480 });
    const croppedDataUrl = croppedCanvas.toDataURL('image/png');

    baseImage = new Image();
    baseImage.onload = () => {
        isImageLoaded = true;
        cropModal.style.display = 'none';
        drawCanvas();
    };
    baseImage.src = croppedDataUrl;

    cropper.destroy();
});

document.getElementById('cancelCrop').addEventListener('click', () => {
    cropModal.style.display = 'none';
    if (cropper) cropper.destroy();
});

// --- Draw Canvas ---
function drawCanvas() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    if (baseImage) {
        const aspect = baseImage.width / baseImage.height;
        let drawW, drawH, offsetX, offsetY;
        if (aspect > 1) {
            drawW = canvas.width;
            drawH = canvas.width / aspect;
            offsetX = 0;
            offsetY = (canvas.height - drawH) / 2;
        } else {
            drawH = canvas.height;
            drawW = canvas.height * aspect;
            offsetX = (canvas.width - drawW) / 2;
            offsetY = 0;
        }
        ctx.drawImage(baseImage, offsetX, offsetY, drawW, drawH);
    }

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

// --- Select hairstyle/facial hair ---
styleThumbs.forEach((img) => {
    img.addEventListener('click', () => {
        if (!isImageLoaded) {
            alert("Please upload and crop your photo first!");
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
        overlayImage.src = img.dataset.src;
    });
});

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
    const btn = document.getElementById(id);
    if (btn) {
        btn.addEventListener('click', () => {
            if (!overlayImage) return alert("Select a hairstyle first!");
            controls[id]();
            drawCanvas();
        });
    }
});

// --- FIX: prevent background scroll and keep cropper above everything ---
const body = document.body;
const observer = new MutationObserver(() => {
    if (cropModal.style.display === 'flex') {
        body.style.overflow = 'hidden';
    } else {
        body.style.overflow = '';
    }
});
observer.observe(cropModal, { attributes: true, attributeFilter: ['style'] });
