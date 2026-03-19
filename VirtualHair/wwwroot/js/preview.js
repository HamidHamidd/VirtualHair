// --- Elements ---
const uploadInput = document.getElementById('imageUpload');
const canvas = document.getElementById('previewCanvas');
const ctx = canvas.getContext('2d');
const styleThumbs = document.querySelectorAll('.style-thumb');
const activeLayerIndicator = document.getElementById('activeLayerIndicator');

const saveBtn = document.getElementById("saveLookBtn");
const titleInput = document.getElementById("lookTitle");
const saveMsg = document.getElementById("saveLookMsg");
const resetBtn = document.getElementById("resetOverlay");

let selectedHairstyleId = null;
let selectedFacialHairId = null;

let baseImage = null;
let isImageLoaded = false;
let cropper = null;

/* ---------------- LAYERS ---------------- */

let hairstyleImage = null;
let facialImage = null;

let hairstyleState = { x: 0, y: 0, scale: 0.65, rotation: 0 };
let facialState = { x: 0, y: 0, scale: 0.4, rotation: 0 };

let activeLayer = null; // "hair" or "facial"

const STEP_MOVE = 5;
const STEP_ZOOM = 0.05;
const STEP_ROTATE = 2;

/* ---------------- AI / FACE DETECTION STATE ---------------- */
let detectedFace = null;
let modelsLoaded = false;

// Load Models from CDN
async function loadAIModels() {
    try {
        const MODEL_URL = 'https://justadudewhohacks.github.io/face-api.js/models/';
        // Use SsdMobilenetv1 for higher precision
        await faceapi.nets.ssdMobilenetv1.loadFromUri(MODEL_URL);
        await faceapi.nets.faceLandmark68Net.loadFromUri(MODEL_URL);
        modelsLoaded = true;
        console.log("AI Models Loaded Successfully");
    } catch (e) {
        console.error("AI Models failed to load", e);
    }
}
loadAIModels();

async function detectFace() {
    if (!modelsLoaded || !baseImage) return;
    
    const status = document.getElementById('aiDetectorStatus');
    const success = document.getElementById('aiSuccessStatus');
    
    status.classList.remove('d-none');
    success.classList.add('d-none');

    try {
        // High quality detection
        const detection = await faceapi.detectSingleFace(canvas).withFaceLandmarks();
        
        if (detection) {
            detectedFace = detection;
            console.log("AI Detected Face:", detectedFace);
            
            // Show success badge for 3 seconds
            success.classList.remove('d-none');
            setTimeout(() => success.classList.add('d-none'), 3000);
        } else {
            console.warn("AI could not find a face.");
        }
    } catch (e) {
        console.error("Detection error:", e);
    } finally {
        status.classList.add('d-none');
    }
}

document.getElementById('reScanFace')?.addEventListener('click', () => {
    detectFace();
});

/* ---------------- CROP MODAL ---------------- */

const cropModal = document.createElement('div');
cropModal.className = 'cropper-modal-overlay';
cropModal.innerHTML = `
<div class="cropper-content-box">
  <h4 class="fw-bold mb-3" style="color: #1e293b;">Adjust Photo</h4>
  <p class="text-secondary small mb-3">Center your face for best results.</p>
  <div class="cropper-img-container">
    <img id="cropperImage" style="max-width:100%; display:block;">
  </div>
  <div class="d-flex justify-content-end gap-2 mt-4">
    <button id="cancelCrop" class="btn btn-outline-secondary px-4 rounded-3">Cancel</button>
    <button id="confirmCrop" class="btn btn-primary px-4 rounded-3" style="background: var(--primary-gradient); color: white; border: none; font-weight: 600;">Confirm</button>
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
                viewMode: 1,
                dragMode: 'move',
                movable: true,
                zoomable: true,
                background: false,
                guides: false,
                center: true,
                highlight: false,
                cropBoxMovable: true,
                cropBoxResizable: true,
                toggleDragModeOnDblclick: false
            });
        };
    };
    reader.readAsDataURL(file);
});

document.getElementById('confirmCrop').addEventListener('click', () => {
    if (!cropper) return;

    const croppedCanvas = cropper.getCroppedCanvas({ width: 500, height: 500 });

    baseImage = new Image();
    baseImage.onload = () => {
        isImageLoaded = true;
        maskCanvas.width = 500;
        maskCanvas.height = 500;
        maskCtx.clearRect(0, 0, maskCanvas.width, maskCanvas.height);
        cropModal.style.display = 'none';
        drawCanvas();
        detectFace(); // Trigger AI analysis
    };
    baseImage.src = croppedCanvas.toDataURL('image/png');
});

document.getElementById('cancelCrop').addEventListener('click', () => {
    cropModal.style.display = 'none';
    if (cropper) cropper.destroy();
});

/* ---------------- THUMBNAILS ---------------- */

let selectedHairThumb = null;
let selectedFacialThumb = null;

function selectNone(type) {
    if (type === 'hairstyle') {
        if (selectedHairThumb) selectedHairThumb.classList.remove("selected-thumb");
        selectedHairstyleId = null;
        hairstyleImage = null;
        selectedHairThumb = null;
        activeLayer = "hair";
    } else {
        if (selectedFacialThumb) selectedFacialThumb.classList.remove("selected-thumb");
        selectedFacialHairId = null;
        facialImage = null;
        selectedFacialThumb = null;
        activeLayer = "facial";
    }
    updateActiveUI();
    drawCanvas();
}

styleThumbs.forEach((thumb) => {
    thumb.addEventListener('click', () => {
        if (!isImageLoaded) {
            alert("Please upload and crop your photo first!");
            return;
        }

        const type = thumb.dataset.type;
        const id = parseInt(thumb.dataset.id);
        const src = thumb.dataset.src;

        if (type === "hairstyle") {
            if (selectedHairThumb) selectedHairThumb.classList.remove("selected-thumb");
            selectedHairThumb = thumb.closest('.thumb-wrapper');
            selectedHairThumb.classList.add("selected-thumb");
            activeLayer = "hair";

            if (selectedHairstyleId !== id) {
                selectedHairstyleId = id;
                const img = new Image();
                img.onload = () => {
                    hairstyleImage = img;
                    
                    // --- AI MAGIC ---
                    if (detectedFace) {
                        const landmarks = detectedFace.landmarks.positions;
                        // Point 27 is the bridge of the nose. We want the center of the hair slightly above that.
                        // Point 8 is the chin (for vertical reference)
                        const noseBridge = landmarks[27];
                        const chin = landmarks[8];
                        const eyeLevel = (landmarks[36].y + landmarks[45].y) / 2;
                        
                        // Estimation for head top
                        const faceHeight = chin.y - eyeLevel;
                        const headTopY = eyeLevel - faceHeight * 0.4; // Approximating forehead

                        hairstyleState.x = noseBridge.x - canvas.width / 2;
                        hairstyleState.y = (eyeLevel - faceHeight * 0.7) - canvas.height / 2;
                        
                        // Scale based on face width (Points 0 to 16)
                        const faceWidth = Math.abs(landmarks[16].x - landmarks[0].x);
                        hairstyleState.scale = (faceWidth / hairstyleImage.width) * 1.55;
                        
                        // Rotation based on eyes
                        const leftEye = landmarks[36];
                        const rightEye = landmarks[45];
                        hairstyleState.rotation = Math.atan2(rightEye.y - leftEye.y, rightEye.x - leftEye.x) * (180 / Math.PI);
                    } else {
                        // Default position
                        hairstyleState.x = 0;
                        hairstyleState.y = -canvas.height * 0.18;
                        hairstyleState.scale = 0.65;
                        hairstyleState.rotation = 0;
                    }
                    drawCanvas();
                };
                img.src = src;
            } else {
                drawCanvas();
            }
        }

        if (type === "facial") {
            if (selectedFacialThumb) selectedFacialThumb.classList.remove("selected-thumb");
            selectedFacialThumb = thumb.closest('.thumb-wrapper');
            selectedFacialThumb.classList.add("selected-thumb");
            activeLayer = "facial";

            if (selectedFacialHairId !== id) {
                selectedFacialHairId = id;
                const img = new Image();
                img.onload = () => {
                    facialImage = img;
                    
                    // --- AI MAGIC ---
                    if (detectedFace) {
                        const landmarks = detectedFace.landmarks.positions;
                        // Point 8 is the chin.
                        const chin = landmarks[8];
                        const mouthTop = landmarks[33]; // Bottom of nose
                        
                        const centerX = (landmarks[16].x + landmarks[0].x) / 2;
                        const centerY = (chin.y + mouthTop.y) / 2;

                        facialState.x = chin.x - canvas.width / 2;
                        facialState.y = (chin.y - (chin.y - mouthTop.y) * 1.2) - canvas.height / 2;

                        const faceWidth = Math.abs(landmarks[16].x - landmarks[0].x);
                        facialState.scale = (faceWidth / facialImage.width) * 0.85;
                        
                        const leftEye = landmarks[36];
                        const rightEye = landmarks[45];
                        facialState.rotation = Math.atan2(rightEye.y - leftEye.y, rightEye.x - leftEye.x) * (180 / Math.PI);
                    } else {
                        facialState.x = 0;
                        facialState.y = canvas.height * 0.15;
                        facialState.scale = 0.45;
                        facialState.rotation = 0;
                    }
                    drawCanvas();
                };
                img.src = src;
            } else {
                drawCanvas();
            }
        }
        
        updateActiveUI();
    });
});

function updateActiveUI() {
    document.querySelectorAll('.thumb-wrapper').forEach(w => w.classList.remove("active-editing"));
    
    if (activeLayer === "hair" && selectedHairThumb) {
        selectedHairThumb.classList.add("active-editing");
        if (activeLayerIndicator) {
            activeLayerIndicator.textContent = "Editing: Hairstyle";
            activeLayerIndicator.style.background = "rgba(99, 102, 241, 0.8)";
        }
    } else if (activeLayer === "facial" && selectedFacialThumb) {
        selectedFacialThumb.classList.add("active-editing");
        if (activeLayerIndicator) {
            activeLayerIndicator.textContent = "Editing: Facial Hair";
            activeLayerIndicator.style.background = "rgba(168, 85, 247, 0.8)";
        }
    }
}

/* ---------------- DRAW ---------------- */

/* ---------------- BRUSH / RETOUCH TOOL ---------------- */

let isDrawing = false;
let brushEnabled = false;
const maskCanvas = document.createElement('canvas');
const maskCtx = maskCanvas.getContext('2d');
let maskHistory = [];
const MAX_HISTORY = 20;

function saveMaskState() {
    maskHistory.push(maskCtx.getImageData(0, 0, maskCanvas.width, maskCanvas.height));
    if (maskHistory.length > MAX_HISTORY) maskHistory.shift();
}

const toggleBrushBtn = document.getElementById('toggleBrush');
const brushOptions = document.getElementById('brushOptions');
const brushSizeInput = document.getElementById('brushSize');
const brushSizeVal = document.getElementById('brushSizeVal');
const brushColorInput = document.getElementById('brushColor');

toggleBrushBtn.addEventListener('click', () => {
    brushEnabled = !brushEnabled;
    toggleBrushBtn.classList.toggle('active', brushEnabled);
    brushOptions.classList.toggle('d-none', !brushEnabled);
    
    // Update indicator
    const indicator = document.getElementById('activeLayerIndicator');
    if (brushEnabled) {
        indicator.innerText = "Mode: Retouching";
        indicator.style.background = "#be185d";
    } else {
        updateActiveUI();
    }
});

brushSizeInput.addEventListener('input', () => {
    brushSizeVal.innerText = brushSizeInput.value;
});

canvas.addEventListener('mousedown', (e) => {
    if (!brushEnabled || !baseImage) return;
    saveMaskState(); // Save before drawing
    isDrawing = true;
    paint(e);
});

window.addEventListener('mousemove', (e) => {
    if (isDrawing) paint(e);
});

window.addEventListener('mouseup', () => {
    isDrawing = false;
});

function paint(e) {
    const rect = canvas.getBoundingClientRect();
    // Calculate scale factor in case CSS resized the canvas
    const scaleX = canvas.width / rect.width;
    const scaleY = canvas.height / rect.height;
    
    const x = (e.clientX - rect.left) * scaleX;
    const y = (e.clientY - rect.top) * scaleY;

    maskCtx.fillStyle = brushColorInput.value;
    maskCtx.beginPath();
    maskCtx.arc(x, y, brushSizeInput.value / 2, 0, Math.PI * 2);
    maskCtx.fill();
    drawCanvas();
}

/* ---------------- BRUSH ACTIONS ---------------- */

document.getElementById('aiAutoHide')?.addEventListener('click', () => {
    if (!detectedFace || !baseImage) {
        alert("AI is still scanning or no photo provided!");
        return;
    }

    saveMaskState();
    
    const landmarks = detectedFace.landmarks.positions;
    // Points 17-26 are eyebrows. Point 27 is nose bridge.
    // Let's sample skin color near the center of the forehead
    const sampleX = Math.round(landmarks[27].x);
    const sampleY = Math.round(landmarks[27].y - (landmarks[8].y - landmarks[27].y) * 0.1);

    // Sample color from canvas
    const p = ctx.getImageData(sampleX, sampleY, 1, 1).data;
    const hexColor = "#" + ((1 << 24) + (p[0] << 16) + (p[1] << 8) + p[2]).toString(16).slice(1);
    
    brushColorInput.value = hexColor;
    maskCtx.fillStyle = hexColor;

    // Create a large "cap" mask
    const faceWidth = Math.abs(landmarks[16].x - landmarks[0].x);
    const topY = landmarks[19].y; // Left eyebrow top
    
    maskCtx.beginPath();
    // Start from left ear area
    maskCtx.moveTo(landmarks[0].x - 20, landmarks[0].y);
    // Go up and around the head
    maskCtx.bezierCurveTo(
        landmarks[0].x - 50, landmarks[0].y - faceWidth,
        landmarks[16].x + 50, landmarks[16].y - faceWidth,
        landmarks[16].x + 20, landmarks[16].y
    );
    // Close through the forehead line
    maskCtx.lineTo(landmarks[16].x + 20, topY);
    maskCtx.lineTo(landmarks[0].x - 20, topY);
    maskCtx.closePath();
    
    maskCtx.fill();
    drawCanvas();
    
    alert("AI automatically masked part of the hair. You can finish with the brush!");
});

document.getElementById('undoBrush')?.addEventListener('click', () => {
    if (maskHistory.length > 0) {
        const lastState = maskHistory.pop();
        maskCtx.putImageData(lastState, 0, 0);
        drawCanvas();
    }
});

document.getElementById('clearBrush')?.addEventListener('click', () => {
    if (confirm("Are you sure you want to clear all retouching?")) {
        maskCtx.clearRect(0, 0, maskCanvas.width, maskCanvas.height);
        maskHistory = [];
        drawCanvas();
    }
});

function drawCanvas() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    
    if (baseImage) {
        ctx.drawImage(baseImage, 0, 0, canvas.width, canvas.height);
        
        // Draw the retouches (the mask) over the photo
        ctx.drawImage(maskCanvas, 0, 0);
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
            alert("Select a style from the menu on the right!");
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

/* ---------------- KEYBOARD ---------------- */

window.addEventListener('keydown', (e) => {
    if (!activeLayer || document.activeElement.tagName === 'INPUT') return;

    let handled = true;
    switch (e.key) {
        case 'ArrowUp': controls.moveUp(); break;
        case 'ArrowDown': controls.moveDown(); break;
        case 'ArrowLeft': controls.moveLeft(); break;
        case 'ArrowRight': controls.moveRight(); break;
        case '+': case '=': controls.zoomIn(); break;
        case '-': case '_': controls.zoomOut(); break;
        case 'q': case 'Q': controls.rotateLeft(); break;
        case 'e': case 'E': controls.rotateRight(); break;
        case 'r': case 'R': resetBtn.click(); break;
        default: handled = false;
    }

    if (handled) {
        e.preventDefault();
        drawCanvas();
    }
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
        alert("Please enter a name for your look.");
        return;
    }

    saveBtn.disabled = true;
    saveBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Saving...';

    const imageData = canvas.toDataURL("image/png");

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

        const data = await res.json();

        if (data.success) {
            window.location.href = "/UserHairstyles";
        } else {
            alert(data.message || "Error saving.");
            saveBtn.disabled = false;
            saveBtn.innerHTML = '<i class="fas fa-save me-2"></i>Save to Gallery';
        }
    } catch (e) {
        alert("An error occurred while saving.");
        saveBtn.disabled = false;
        saveBtn.innerHTML = '<i class="fas fa-save me-2"></i>Save to Gallery';
    }
});
